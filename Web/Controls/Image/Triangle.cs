using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using Directions = Idaho.Draw.Triangle.Directions;

namespace Idaho.Web.Controls {
	public class Triangle : Image {

		private Directions _direction = Directions.Right;
		private Color _color;
		private Color _activeColor;
		private int _opacity = 100;
		private int _activeOpacity = 0;

		#region Properties

		public Directions Direction { set { _direction = value; } }
		public Directions Point { set { this.Direction = value; } }
		public string Color { set { _color = ColorTranslator.FromHtml(value); } }
		public int Opacity { set { _opacity = value; } }

		/// <summary>
		/// The alpha value when mouse over image
		/// </summary>
		/// <remarks>
		/// If this value is non-zero then a second image is generated
		/// to create the mouse-over effect
		/// </remarks>
		public int ActiveOpacity { set { _activeOpacity = value; } }

		#endregion

		/// <summary>
		/// Generate image tag for triangle
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			if (_activeOpacity > 0) {
				_activeColor = Draw.Utility.AdjustOpacity(_color, _activeOpacity);
			}
			if (_opacity < 100) {
				_color = Draw.Utility.AdjustOpacity(_color, _opacity);
			}
			string cacheKey = string.Format("triangle{0}{1}{2}{3}",
				_direction, this.Width, this.Height, _color.ToArgb());

			if (!this.TagInCache(cacheKey)) {
				Idaho.Draw.Triangle draw = new Idaho.Draw.Triangle(_direction);

				draw.Width = this.Width;
				draw.Height = this.Height;
				draw.Color.ForeGround = _color;
				draw.Color.Highlight = _activeColor;
				this.Src = draw.Url;
				this.Style.Add("width", string.Format("{0}px", this.Width));
				this.Style.Add("height", string.Format("{0}px", this.Height));
			}

			// attributes that aren't cached
			this.Transparency = true;
			this.AlternateText = "";
			this.Generated = true;
			base.OnPreRender(e);
		}
	}
}
