using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;

namespace Idaho.Web.Controls {
	public class Star : Idaho.Web.Controls.Image {

		private int _radius = 0;
		private Color _foreGroundColor;
		private Color _burstColor;
		private Color _borderColor;
		private int _borderWidth = 0;
		private int _points = 5;
		private float _sharpness = 0.58F;

		#region Static Fields

		private static int _defaultRadius = 0;
		private static int _defaultBorderWidth = 0;
		private static Color _defaultColor;
		private static Color _defaultBorderColor;
		private static Color _defaultBurstColor;

		public static int DefaultRadius {
			set { _defaultRadius = value; }
			internal get { return _defaultRadius; }
		}
		public static int DefaultBorderWidth {
			set { _defaultBorderWidth = value; }
			internal get { return _defaultBorderWidth; }
		}
		public static Color DefaultColor {
			set { _defaultColor = value; }
			internal get { return _defaultColor; }
		}
		public static Color DefaultBorderColor {
			set { _defaultBorderColor = value; }
			internal get { return _defaultBorderColor; }
		}
		public static Color DefaultBurstColor {
			set { _defaultBurstColor = value; }
			internal get { return _defaultBurstColor; }
		}

		#endregion

		#region Properties

		public Unit Sharpness {
			set {
				switch (value.Type) {
					case UnitType.Percentage:
						_sharpness = (float)value.Value / 100; break;
					default:
						_sharpness = (float)value.Value; break;
				}
			}
		}
		public int Points { set { _points = value; } }
		public int Radius { get { return _radius; } set { _radius = value; } }
		public string Color { set { _foreGroundColor = ColorTranslator.FromHtml(value); } }
		public string BurstColor { set { _burstColor = ColorTranslator.FromHtml(value); } }
		public string BorderColor { set { _borderColor = ColorTranslator.FromHtml(value); } }
		public int BorderWidth { set { _borderWidth = value; } }

#endregion

		/// <summary>
		/// Generate image tag for rating stars
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			if (_radius == 0) { _radius = _defaultRadius; }
			string cacheKey = string.Format("star{0}", _radius);

			if (!this.TagInCache(cacheKey)) {
				Idaho.Draw.Star draw = new Idaho.Draw.Star(1, 0, _points, _sharpness);

				draw.Width = _radius * 10;
				draw.Height = _radius * 2;
				draw.Color.ForeGround = Utility.NoNull<Color>(_foreGroundColor, _defaultColor);
				draw.Color.Border = Utility.NoNull<Color>(_borderColor, _defaultBorderColor);
				draw.Color.Highlight = Utility.NoNull<Color>(_burstColor, _defaultBurstColor);
				draw.BorderWidth = Utility.NoNull<int>(_borderWidth, _defaultBorderWidth);
				this.Src = draw.Url;

				this.Style.Add("width", string.Format("{0}px", _radius * 10));
				this.Style.Add("height", string.Format("{0}px", _radius * 2));
				this.Transparency = true;
			}

			// attributes that aren't cached
			this.AlternateText = "";
			this.Generated = true;
			base.OnPreRender(e);
		}

		protected override void Render(HtmlTextWriter writer) {
			writer.Write("<img src=\"");
			writer.Write(this.Src);
			writer.Write("\" width=\"");
			writer.Write(_radius * 2);
			writer.Write("\" height=\"");
			writer.Write(_radius * 2);
			writer.Write("\"");
			this.RenderAttributes(writer);
			writer.Write("/>");
		}
	}
}