using System.Drawing;
using System.Web.UI;
using System.Configuration;

namespace Idaho.Web.Controls {
	public class Corner : HtmlControl {

		private int _radius = 0;
		private Color _foreGroundColor;
		private Color _backGroundColor;
		private Color _borderColor;
		private int _borderWidth = 0;
		private int _extendWidth = 0;
		private int _extendHeight = 0;
		private int _shadowWidth = 0;
		private Idaho.Draw.Corner _draw;

		#region Static Fields

		private static Color _defaultColor;
		private static Color _defaultBackgroundColor;
		private static Color _defaultBorderColor;
		private static int _defaultRadius = 0;
		private static int _defaultBorderWidth = 0;

		public static Color DefaultColor { set { _defaultColor = value; } }
		public static Color DefaultBackgroundColor { set { _defaultBackgroundColor = value; } }
		public static Color DefaultBorderColor { set { _defaultBorderColor = value; } }
		public static int DefaultRadius { set { _defaultRadius = value; } }
		public static int DefaultBorderWidth { set { _defaultBorderWidth = value; } }

		#endregion

		#region Properties

		/// <summary>
		/// Extend the corner to accomodate, for example, a title bar
		/// </summary>
		public int ExtendWidth { set { _extendWidth = value; } }

		/// <summary>
		/// Extend the corner to accomodate, for example, a title bar
		/// </summary>
		public int ExtendHeight { set { _extendHeight = value; } }

		public bool ForceNew { set { _draw.ForceRegeneration = value; } }
		public int Radius { set { _radius = value; } }
		public string Color { set { _foreGroundColor = ColorTranslator.FromHtml(value); } }
		public string BackGround { set { _backGroundColor = ColorTranslator.FromHtml(value); } }
		public string BorderColor { set { _borderColor = ColorTranslator.FromHtml(value); } }
		public int BorderWidth { set { _borderWidth = value; } }
		public string Orientation {
			set {
				switch (value) {
					case "tl":
						_draw.Position = Draw.Corner.Positions.TopLeft;
						break;
					case "tr":
						_draw.Position = Draw.Corner.Positions.TopRight;
						break;
					case "br":
						_draw.Position = Draw.Corner.Positions.BottomRight;
						break;
					case "bl":
						_draw.Position = Draw.Corner.Positions.BottomLeft;
						break;
					default:
						_draw.Position = Draw.Corner.Positions.TopLeft;
						break;
				}
			}
		}
		public int ShadowWidth { set { _shadowWidth = value; } }

		#endregion

		public Corner() { _draw = new Idaho.Draw.Corner(); }

		/// <summary>
		/// Get default values when none specified
		/// </summary>
		protected override void OnPreRender(System.EventArgs e) {
			_draw.Radius = Utility.NoNull<int>(_radius, _defaultRadius);
			// default height and width equal the radius
			_draw.Width = _draw.Radius + _shadowWidth;
			_draw.Height = _draw.Radius + _shadowWidth;
			_draw.ShadowWidth = _shadowWidth;
			// colors
			_draw.Color.BackGround = Utility.NoNull<Color>(
				_backGroundColor, _defaultBackgroundColor, System.Drawing.Color.Transparent);
			_draw.Color.ForeGround = Utility.NoNull<Color>(_foreGroundColor, _defaultColor);
			_draw.Color.Border = Utility.NoNull<Color>(
				_borderColor, _defaultBorderColor, _foreGroundColor);

			_draw.BorderWidth = Utility.NoNull<int>(_borderWidth, _defaultBorderWidth);
			// optional extensions
			_draw.ExtendWidth = _extendWidth;
			_draw.ExtendHeight = _extendHeight;
			base.OnPreRender(e);
		} 

		/// <summary>
		/// Generate image tag for graphic
		/// </summary>
		protected override void Render(HtmlTextWriter writer) {
			writer.Write("<img id=\"{0}\" src=\"", this.ID);
			writer.Write(_draw.Url);
			writer.Write("\" width=\"");
			writer.Write(_draw.Width);
			writer.Write("\" height=\"");
			writer.Write(_draw.Height);
			writer.Write("\" alt=\"\" />");
		}
	}
}