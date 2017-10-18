using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using color = System.Drawing.Color;

namespace Idaho.Draw {
	/// <summary>
	/// Base class for generated graphics cached to file system
	/// </summary>
	[Serializable]
	public abstract class GraphicBase {

		#region Collections

		/// <summary>
		/// Internal class used to store color information
		/// </summary>
		[Serializable]
		public class Colors {
			public Color BackGround = color.Transparent;
			public Color ForeGround = color.Empty;
			public Color Border = color.Empty;
			public Color Text = color.Empty;
			/// <summary>
			/// Color of text that is selected or the mouse is over
			/// </summary>
			public Color ActiveText = color.Empty;
			/// <summary>
			/// Color of graphic that is selected or the mouse is over
			/// </summary>
			public Color Highlight = color.Empty;
			public Color Shadow = color.Black;
		}

		#endregion

		private Pen _borderPen = null;
		private System.Drawing.Drawing2D.GraphicsPath _path;
		private Brush _foregroundBrush = null;
		private Brush _highlightBrush = null;
		private PathGradientBrush _gradientBrush = null;
		private System.Drawing.Bitmap _bitmap = null;
		private Graphics _graphic = null;
		private string _appPath = string.Empty;
		private string _absolutePath = string.Empty;
		private ImageFormat _format = ImageFormat.Png;
		private Size _size = Size.Empty;
		private int _borderWidth = 2;
		private bool _forceRegeneration = false;
		public GraphicBase.Colors Color = new GraphicBase.Colors();
		private int _shadowSize = 0;
		private Point _shadowOffset;

		public enum Shapes {
			Rectangle,
			Ellipse
		}

		#region Static Fields

		private static int _jpegQuality = 100;
		private static DirectoryInfo _generatedImageFolder;
		public static DirectoryInfo GeneratedImageFolder {
			set { _generatedImageFolder = value; }
			get { return _generatedImageFolder; }
		}
		public static int JpegQuality {
			set {
				_jpegQuality = value;
				if (_jpegQuality < 0) { _jpegQuality = 0; }
				if (_jpegQuality > 100) { _jpegQuality = 100; }
			}
		}

		#endregion

		#region Protected Properties

		protected string PathAndName { get { return _appPath; } }
		protected string AbsolutePath { get { return _absolutePath; } set { _absolutePath = value; } }

		public int ShadowSize { set { _shadowSize = value; } }
		public Point ShadowOffset { set { _shadowOffset = value; } }

		/// <summary>
		/// Used for GDI+ draw graphics
		/// </summary>
		protected Graphics Graphic {
			get {
				if (_graphic == null) {
					_bitmap = new Bitmap(_size.Width, _size.Height);
					_graphic = Graphics.FromImage(_bitmap);
					_graphic.SmoothingMode = SmoothingMode.HighQuality;
					if (!this.Transparent) { _graphic.Clear(this.Color.BackGround); }
				}
				return _graphic;
			}
			set {
				if (value == null) { _bitmap = null; _graphic = null; }
				else { _graphic = value; }
			}
		}

		/// <summary>
		/// Used for images such as photographs
		/// </summary>
		protected Image Image { set { _bitmap = new Bitmap(value); } }

		public Bitmap Bitmap {
			protected internal set { _bitmap = value; }
			get { return _bitmap; }
		}

		protected Pen BorderPen {
			get {
				if (_borderPen == null) {
					_borderPen = new Pen(this.Color.Border, _borderWidth);
					_borderPen.DashStyle = DashStyle.Solid;
					//_borderpen.
				}
				return _borderPen;
			}
		}

		protected Brush ForegroundBrush {
			get {
				if (_foregroundBrush == null) {
					_foregroundBrush = new SolidBrush(this.Color.ForeGround);
				}
				return _foregroundBrush;
			}
		}

		protected Brush HighlightBrush {
			get {
				if (_highlightBrush == null) {
					_highlightBrush = new SolidBrush(this.Color.Highlight);
				}
				return _highlightBrush;
			}
		}


		protected GraphicsPath Path { get { return _path; } set { _path = value; } }

		protected PathGradientBrush GradientBrush {
			get {
				if (_gradientBrush == null && (_path != null)) {
					_gradientBrush = new PathGradientBrush(_path);
					Blend b = new Blend();

					_gradientBrush.CenterColor = this.Color.ForeGround;
					_gradientBrush.SurroundColors = new Color[] { this.Color.Highlight };

					b.Factors = new float[] { 1, 0 };
					b.Positions = new float[] { 0, 1 };

					_gradientBrush.Blend = b;
					//.Transform = New Matrix(1, 0, 0, 1, CSng(_height / 2), CSng(_height / 2))
				}
				return _gradientBrush;
			}
		}

		#endregion

		#region Public Properties

 		public bool Transparent {
			get {
				return Color.BackGround.Equals(System.Drawing.Color.Transparent) ||
					Color.BackGround.A.Equals(0x0);
			}
		}
		/// <summary>
		/// Filename extension
		/// </summary>
		public string Extension { get { return _format.ToString().ToLower(); } }
		public ImageFormat Format { set { _format = value; } }

		/// <summary>
		/// Generate graphic on each request rather than caching
		/// </summary>
		public bool ForceRegeneration { set { _forceRegeneration = value; } }
		public int BorderWidth { get { return _borderWidth; } set { _borderWidth = value; } }
		public Size Size { get { return _size; } set { _size = value; } }
		public int Width { get { return _size.Width; } set { _size.Width = value; } }
		public int Height { get { return _size.Height; } set { _size.Height = value; } }

		#endregion

		/// <summary>
		/// Generate URL for this graphic
		/// </summary>
		/// <remarks>
		/// Builds absolute path of, for example,
		///		D:\Whatever\Name.Space\images\generated\btn_Reload_FFF.png
		/// And Url of
		///		images/generated/btn_Reload_FFF.png
		/// </remarks>
		public string Url {
			get {
				Assert.NoNull(_generatedImageFolder, "NullGeneratedImageFolder");
				string fileName = this.GenerateFileName();
				string path = string.Format("{0}\\{1}", _generatedImageFolder.FullName, fileName);
				_absolutePath = path;

				if (_forceRegeneration || !File.Exists(_absolutePath)) {
					this.Create();
					this.Save();
				}
				_appPath = "/" + path.Replace(HttpRuntime.AppDomainAppPath, "").Replace("\\", "/");
				return _appPath;
			}
		}
		protected abstract string GenerateFileName();
		internal abstract void Create();

		/// <summary>
		/// Save current graphic and prepare to create the mouse-over graphic
		/// </summary>
		protected void ResetForNextImage() {
			this.Save(false);
			this.AbsolutePath = this.Rename(this.AbsolutePath);
		}

		/// <summary>
		/// Rename this graphic to be the mouse-over version
		/// </summary>
		/// <param name="name">The non-mouse-over graphic name</param>
		protected string Rename(string name) {
			return name.Replace("." + this.Extension, "_on." + this.Extension);
		}

		/// <summary>
		/// Draw line for given points
		/// </summary>
		protected void SetPathFromPoints(PointF[] points) {
			_path = new GraphicsPath();
			_path.AddLines(points);
		}

		/// <summary>
		/// Get Hex string name for color
		/// </summary>
		/// <remarks>Will shorten to RGB instead of RRGGBB format if possible.</remarks>
		[Obsolete()]
		protected string ColorName(System.Drawing.Color color) {
			string rgb = ColorTranslator.ToHtml(color).Replace("#", "");
			string r = rgb.Substring(0, 2);
			string g = rgb.Substring(2, 2);
			string b = rgb.Substring(4, 2);

			if (this.EqualHex(r) && this.EqualHex(g) && this.EqualHex(b)) {
				rgb = string.Format("{0}{1}{2}",
					r.Substring(0, 1), g.Substring(0, 1), b.Substring(0, 1));
			}
			return rgb;
		}

		/// <summary>
		/// Indicate if two parts of color hex value are equal
		/// </summary>
		/// <example>The red value in 442315 is equal but the blue is not.</example>
		protected bool EqualHex(string hex) { return hex.Substring(0, 1) == hex.Substring(1, 1); }

		/// <summary>
		/// Get the next color evenly spaced between two colors
		/// </summary>
		/// <param name="thisStep">This step in relation to total</param>
		/// <param name="steps">Total number of transitional colors</param>
		/// <remarks>Used to create gradients</remarks>
		protected Color PickColorBetween(Color c1, Color c2, int thisStep, int steps) {
			int r;
			int g;
			int b;

			r = c1.R + (int)thisStep * ((-c1.R + c2.R) / steps);
			g = c1.G + (int)thisStep * ((-c1.G + c2.G) / steps);
			b = c1.B + (int)thisStep * ((-c1.B + c2.B) / steps);

			return System.Drawing.Color.FromArgb(r, g, b);
		}

		/// <summary>
		/// Save generated graphic
		/// </summary>
		/// <param name="dispose">If done generating graphics, use to dispose of drawing objects</param>
		protected void Save(bool dispose) {
			try {
				if (dispose) {
					if (_format == ImageFormat.Jpeg && _jpegQuality < 100) {
						// save compressed JPEG
						Utility.SaveJpegWithCompression(_bitmap, _absolutePath, _jpegQuality);
					} else {
						_bitmap.Save(_absolutePath, _format);
					}
					if (_borderPen != null) { _borderPen.Dispose(); }
					if (_foregroundBrush != null) { _foregroundBrush.Dispose(); }
					if (_graphic != null) { _graphic.Dispose(); }
					_bitmap.Dispose();
				} else {
					// save clone only so bitmap can be re-used
					Bitmap copy = (Bitmap)_bitmap.Clone();
					copy.Save(_absolutePath, _format);
				}
			} catch (System.Exception e) {
				Idaho.Exception.Log(e, Idaho.Exception.Types.FileSystem);
			}
		}
		protected void Save() { this.Save(true); }
	}
}