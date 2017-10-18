using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Configuration;

namespace Idaho.Web.Controls {
	public class RatingStars : Idaho.Web.Controls.Image {

		private float _rating = 0;
		private int _radius = 0;
		private Color _foreGroundColor;
		private Color _burstColor;
		private Color _borderColor;
		private int _borderWidth = 0;
		private int _points = 5;
		private float _sharpness = 0.58F;

		#region Properties

		public float Rating { set { _rating = value; } }
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
		private void Rating_PreRender(object sender, System.EventArgs e) {
			if (_rating > 0) {
				if (_radius == 0) { _radius = Star.DefaultRadius; }
				string cacheKey = string.Format("rating{0}_{1}", _rating, _radius);

				if (!this.TagInCache(cacheKey)) {
					Idaho.Draw.Star draw = new Idaho.Draw.Star(5, _rating, _points, _sharpness);

					draw.Width = _radius * 10;
					draw.Height = _radius * 2;
					draw.Color.ForeGround = Utility.NoNull<Color>(_foreGroundColor, Star.DefaultColor);
					draw.Color.Border = Utility.NoNull<Color>(_borderColor, Star.DefaultBorderColor);
					draw.Color.Highlight = Utility.NoNull<Color>(_burstColor, Star.DefaultBurstColor);
					draw.BorderWidth = Utility.NoNull<int>(_borderWidth, Star.DefaultBorderWidth);
					this.Src = draw.Url;

					this.Style.Add("width", string.Format("{0}px", _radius * 10));
					this.Style.Add("height", string.Format("{0}px", _radius * 2));
					//Me.Style.Add("border", "1px solid red")
					this.Transparency = true;
				}

				// attributes that aren't cached
				this.AlternateText = "";
				this.Generated = true;
			}
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			if (_rating > 0) { base.Render(writer); }
		}
	}
}