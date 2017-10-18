using Idaho;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web.UI;
using System.Configuration;
using web=System.Web.UI.WebControls;

namespace Idaho.Web.Controls {
	public class Text : Idaho.Web.Controls.Image {

		private string _text = string.Empty;
		private float _rotate = 0;
		private string _fontName = "Tahoma";
		private int _size = 10;
		private int _opacity = -1;
		private Color _color = System.Drawing.Color.Black;
		private FontStyle _fontStyle = FontStyle.Regular;
		private string _resourceKey = string.Empty;

		#region Properties

		public FontStyle FontStyle { set { _fontStyle = value; } }

		/// <summary>
		/// The text to be rendered
		/// </summary>
		public string Value { set { _text = value; } }
		public string FontName { set { _fontName = value; } }
		public new float Rotate { set { _rotate = value; } }
		public int Size { set { _size = value; } }
		public string Color { set { _color = ColorTranslator.FromHtml(value); } }
		public int Opacity { set { _opacity = Utility.Constrain(value, 0, 100); } }
		public string Resx { set { _resourceKey = value; } }

		/// <summary>
		/// Resource key to look up button text
		/// </summary>
		public string ResourceKey { set { this.Resx = value; } }

		#endregion

		#region Constructors

		public Text() { }
		public Text(string id) { this.ID = id; }
		public Text(string id, string resx) : this(id) { _resourceKey = resx; }

		#endregion

		#region Events

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
		}

		/// <summary>
		/// Get values needed to render text
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			string cacheKey = Utility.NoNull<string>(_resourceKey, _text);

			if (!this.TagInCache(cacheKey) || this.ForceNew) {
				Assert.NoNull(_fontName, "NullButtonFontName");

				// load resource string if specified
				if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_resourceKey)) {
					string resx = string.Format("Label_{0}", _resourceKey);
					_text = Idaho.Resource.Say(resx);
					Assert.NoNull(_text, "NullResource", resx);
				}
				if (_opacity > 0 && _opacity < 100) {
					Draw.Utility.AdjustOpacity(_color, _opacity);
				}
				Idaho.Draw.TextGraphic draw = new Idaho.Draw.TextGraphic();
				draw.Text = _text;
				draw.Rotate = _rotate;
				draw.Color.Text = _color;
				draw.FontName = _fontName;
				draw.FontStyle = _fontStyle;
				draw.Size = _size;
				draw.ForceRegeneration = this.ForceNew;
				this.Transparency = true;
				this.Src = draw.Url;
				this.Height = draw.Height;
				// height computed after .Url method runs
			}
			// attributes that aren't cached
			this.Generated = true;
			this.CssClass = "text";
			base.OnPreRender(e);
		}

		#endregion
	}
}