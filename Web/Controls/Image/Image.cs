using Idaho;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Caching;

namespace Idaho.Web.Controls {
	public class Image : HtmlControl, IImage {

		private bool _generatedToDisk = false;
		private string _tag = string.Empty;
		private string _tagCacheKey = string.Empty;
		private string _byteCacheKey = string.Empty;
		private bool _submitForm = false;
		private bool _resetForm = false;
		private bool _resize = false;
		private ImageFormat _format = ImageFormat.Jpeg;
		private bool _transparency = false;
		// image source as file path
		private string _src = string.Empty;
		// image source as byte array
		private byte[] _imageBytes;
		// image source as object
		private System.Drawing.Image _image;
		// image source as FileInfo
		private FileInfo _file;
		private int _height = 0;
		private int _width = 0;
		private int _sharpenRadius = 0;
		private float _sharpenIntensity = 1;
		private string _rotate = string.Empty;
		private const string _blankImage = "/images/blank.gif";
		private const string _cacheUrl = "images/cache.ashx?key={0}&type={1}";
		private const string _alphaFilter = "DXImageTransform.Microsoft.AlphaImageLoader";
		private bool _initialized = false;
		private bool _forceRegeneration = false;

		#region Properties

		/// <summary>
		/// Enable or disable mouse over scripts
		/// </summary>
		public bool IsMouseOver {
			set {
				if (value) {
					this.OnMouseOver = Idaho.Resource.SayFormat(
						"Script_ButtonMouseOver", true.ToJSON());
					this.OnMouseOut = Idaho.Resource.SayFormat(
						"Script_ButtonMouseOver", false.ToJSON());
				} else {
					this.OnMouseOver = string.Empty;
					this.OnMouseOut = string.Empty;
				}
			}
		}

		/// <summary>
		/// Force generation of a new graphic
		/// </summary>
		public bool ForceNew {
			set { _forceRegeneration = value; }
			protected get { return _forceRegeneration; }
		}

		/// <summary>
		/// Specify image from assembly resources
		/// </summary>
		public string Resource {
			set { _src = Handlers.Resource.GetURL(value); }
		}

		/// <summary>
		/// Indicate if image should be rotated
		/// </summary>
		/// <remarks>This is only coded for use with resized images.</remarks>
		public string Rotate {
			set {
				if (!string.IsNullOrEmpty(value)) {
					value = value.ToLower();
					if (value == "cw" || value == "ccw") {
						_rotate = value;
					}
				}
			}
		}
		public int SharpenRadius {
			set { _sharpenRadius = value; }
			get { return _sharpenRadius; }
		}
		public float SharpenIntensity {
			set { _sharpenIntensity = value; }
			get { return _sharpenIntensity; }
		}
		public bool Generated { set { _generatedToDisk = value; } }
		public System.Drawing.Image ImageGraphic { get { return _image; } set { _image = value; } }
		public byte[] ImageBytes { get { return _imageBytes; } set { _imageBytes = value; } }

		/// <summary>
		/// The generated string used to identify image bytes in cache
		/// </summary>
		/// <remarks>
		/// Key may be stored in page view state to ensure multiple requests
		/// on the same page (even for post-backs) access the same image bytes.
		/// </remarks>
		public string ByteCacheKey {
			get {
				if (string.IsNullOrEmpty(_byteCacheKey)) {
					if (this.ViewState["CacheKey"] == null) {
						_byteCacheKey = Guid.NewGuid().ToString();
						this.ViewState["CacheKey"] = _byteCacheKey;
					} else {
						_byteCacheKey = this.ViewState["CacheKey"].ToString();
					}
				}
				return _byteCacheKey;
			}
		}

		/// <summary>
		/// The string used to identify an HTML tag in cache
		/// </summary>
		public string TagCacheKey { set { _tagCacheKey = value; } }

		/// <summary>
		/// Resize image to specified height and width
		/// </summary>
		public bool Resize {
			set {
				_resize = value;
				if (_resize) { base.Style.Remove("width"); base.Style.Remove("height"); }
			}
			get { return _resize; }
		}
		
		public string RollOver {
			set {
				base.OnMouseOut = string.Format(value, EcmaScript.False);
				base.OnMouseOver = string.Format(value, EcmaScript.True);
			}
		}
		/// <summary>
		/// Is the image transparent
		/// </summary>
		public bool Transparency {
			set { _transparency = value; if (_transparency) { _format = ImageFormat.Png; } }
		}
		/// <summary>
		/// Alternate text to display as tip for mouse-over
		/// </summary>
		public string Alt { set { base.Attributes.Add("alt", value); } }

		/// <summary>
		/// Alternate text to display as tip for mouse-over
		/// </summary>
		public string AlternateText {
			get { return base.Attributes["alt"]; }
			set { base.Attributes.Add("alt", value); }
		}
		/// <summary>
		/// Render image as input type="image"
		/// </summary>
		public bool SubmitForm {
			set {
				_submitForm = value;
				if (_submitForm) { _resetForm = false; }
			}
		}
		/// <summary>
		/// Render image as input type="reset"
		/// </summary>
		/// <remarks>
		/// In lieu of rendering a true reset button, add script to
		/// reset the form.
		/// </remarks>
		public bool ResetForm {
			set {
				_resetForm = value;
				if (_resetForm) {
					_submitForm = false;
					this.OnClick += "Page.Form.reset(); return false;";
				}
			}
		}
		/// <summary>
		/// Local path to image, generating if necessary
		/// </summary>
		public string Src {
			internal get {
				if (!string.IsNullOrEmpty(_src)) {
					return _src;
				} else if (this.ImageGraphic != null || this.ImageBytes != null) {
					// generate src for cached image
					return string.Format(_cacheUrl, this.ByteCacheKey, _format);
				} else {
					return null;
				}
			}
			set {
				_src = value.Replace("~", Utility.BasePath);
				_tag = string.Empty;
				// reset tag for new source
			}
		}
		/// <summary>
		/// Return image source path without generating or caching
		/// </summary>
		public string Path { get { return _src; } }

		/// <summary>
		/// Render image based on FileInfo
		/// </summary>
		/// <remarks>
		/// This is presently only coded to support resized images. This does not
		/// result in a viable src attribute apart from the cached resized image.
		/// </remarks>
		public FileInfo File {
			set {
				_file = value;
				if (_file != null) {
					_src = _file.Name;
					_file.Refresh();
				}
				_tag = string.Empty;
			}
		}
		/// <summary>
		/// Width in pixels
		/// </summary>
		public int Width {
			get { return _width; }
			set {
				if (value != 0) {
					if (!_resize) { base.Style.Add("width", string.Format("{0}px", value)); }
					_width = value;
				}
			}
		}

		/// <summary>
		/// Height in pixels
		/// </summary>
		public int Height {
			get { return _height; }
			set {
				if (value != 0) {
					if (!_resize) { base.Style.Add("height", string.Format("{0}px", value)); }
					_height = value;
				}
			}
		}

		/// <summary>
		/// Add an onlick event to direct to the given URL
		/// </summary>
		public string Url {
			set {
				this.OnClick = Idaho.Resource.SayFormat(
					"Script_Redirect", value, ObjectExtension.ToJSON(null));
			}
		}

		#endregion

		// default constructor
		public Image() { }
		// for programmatic control creation
		public Image(Idaho.Web.Page page) { this.Page = page; }

		/// <summary>
		/// Prepare image tag
		/// </summary>
		/// <remarks>
		/// Images not physically present on disk are cached in memory.
		/// </remarks>
		protected override void OnPreRender(EventArgs e) {
			// images passed as bytes
			if (this.ImageGraphic != null) {
				this.CacheBytes(this.ImageGraphic);
				_width = this.ImageGraphic.Width;
				_height = this.ImageGraphic.Height;
			} else if (this.ImageBytes != null) {
				this.CacheBytes(this.ImageBytes);
			} else if (_resize) {
				// invoke drawing object to resize image
				_tagCacheKey = string.Format("{0}_{1}x{2}_{3}at{4}_{5}",
					_src.Replace(" ", ""), _width, _height, _sharpenRadius, _sharpenIntensity, _rotate);
				if (!this.TagInCache(_tagCacheKey)) {
					Idaho.Draw.ScaledImage draw;
					if (_file != null) {
						draw = new Idaho.Draw.ScaledImage(_file, _width, _height);
					} else {
						draw = new Idaho.Draw.ScaledImage(_src, _width, _height);
					}
					draw.Format = (_transparency) ? ImageFormat.Png : ImageFormat.Jpeg;
					draw.Rotate = _rotate;
					draw.SharpenRadius = _sharpenRadius;
					draw.SharpenIntensity = _sharpenIntensity;
					_src = draw.Url;
				}
				// attributes that aren't cached
				//this.Style.Add("width", string.Format("{0}px", this.Width));
				//this.Style.Add("height", string.Format("{0}px", this.Height));
				this.AlternateText = "";
				this.Generated = true;
			}
			base.OnPreRender(e);
			_initialized = true;
		}
		//internal void OnPreRender() { this.OnPreRender(new EventArgs()); }

		/// <summary>
		/// Render image
		/// </summary>
		/// <remarks>
		/// Images that are generated by GDI+ and written to disk have their
		/// HTML tag cached to avoid recomputing the generated filename.
		/// Static images or images generated only in memory do not cache
		/// their HTML tag since the name is not computed.
		/// </remarks>
		protected override void Render(HtmlTextWriter writer) {
			if (!_initialized) { this.OnPreRender(new EventArgs()); }
			if (_generatedToDisk) {
				// cache tags for all generated and persisted images
				if (string.IsNullOrEmpty(_tag)) {
					StringBuilder sb = new StringBuilder();
					StringWriter sw = new StringWriter(sb);
					HtmlTextWriter html = new HtmlTextWriter(sw);

					this.RenderTag(html);
					_tag = sb.ToString();
					this.CacheTag();
				}
				writer.Write(_tag);
			} else {
				// directly write uncached tags
				this.RenderTag(writer);
			}
			this.RenderAttributes(writer);
			writer.Write(" />");
		}

		/// <summary>
		/// Render image tag
		/// </summary>
		private void RenderTag(HtmlTextWriter writer) {
			bool filter = !this.Profile.HandlesTransparency && _transparency;
			StringBuilder image = new StringBuilder();
			writer.Write(_submitForm ? "<input type=\"image\" " : "<img ");
			writer.Write("src=\"");
			if (filter) {
				writer.Write(Utility.BasePath);
				writer.Write(_blankImage);
			} else {
				writer.Write(this.Src);
			}
			writer.Write("\"");
			if (filter) {
				// add DX transform for IE6 and older
				writer.Write(" style=\"filter:progid:");
				writer.Write(_alphaFilter);
				writer.Write("(src='");
				writer.Write(this.Src);
				writer.Write("', sizingMethod='image')\"");
			}
		}

		/// <summary>
		/// Cache image tag
		/// </summary>
		/// <remarks>
		/// This cache depends on a physically present image source.
		/// </remarks>
		private void CacheTag() {
			this.Context.Cache.Insert(_tagCacheKey, _tag,
				new CacheDependency(HttpRuntime.AppDomainAppPath + _src));
		}

		/// <summary>
		/// Check if image tag is cached
		/// </summary>
		/// <remarks>
		/// This is typically utilized by controls that write generated images
		/// to disk so that the only value cached in memory is the image tag
		/// with the generated file name.
		/// </remarks>
		protected bool TagInCache(string cacheKey) {
			if (string.IsNullOrEmpty(cacheKey)) { return false; }
			if (this.Profile.HandlesTransparency) { cacheKey += "dx"; }
			if (_submitForm) { cacheKey += "submit"; }
			_tagCacheKey = cacheKey;
			object tag = this.Context.Cache[cacheKey];
			if (tag == null) {
				return false;
			} else {
				_tag = tag.ToString();
				return true;
			}
		}

		/// <summary>
		/// Cache image bytes
		/// </summary>
		/// <remarks>
		/// This is typically utilized by controls that store generated images
		/// in memory rather than persisting them to disk.
		/// </remarks>
		private void CacheBytes(object image) {
			if (this.Context.Cache[_byteCacheKey] == null) {
				this.Context.Cache.Add(_byteCacheKey, image, null,
					System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5),
					CacheItemPriority.High, null);
			}
		}
	}
}