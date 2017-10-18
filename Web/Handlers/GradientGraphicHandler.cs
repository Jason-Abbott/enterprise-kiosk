using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;

/*--COMMENT--------------------------------------------------------------
	Render a gradient. To use, first setup a handler in web.config
	
	<add verb="GET" path="images/gradient.ashx"
	type="AMP.Handlers.GradientGraphic,AMP.System" />

	then, for this example, setup style rules as follows:

	background-image: url(../images/gradient.ashx?c1=036&c2=036&a1=70&a2=0&w=1&h=20&d=ttb);
------------------------------------------------------------------------*/
namespace Idaho.Web.Handlers {
	public class GradientGraphic : Handlers.Graphic {
	   
		private Color _startColor;
		private Color _endColor;
		// as whole number percentage
		private int _startOpacity = 100;
		private int _endOpacity = 0;
		private float _angle;
		private bool _orthogonal = true;
		// as whole number percentage
		private int _midPoint = 50;
		private bool _endOpacityGiven = false;

		/// <summary>
		/// Build gradient for output
		/// </summary>
		public override void ProcessRequest(HttpContext context) {
			base.Initialize();
			bool transparency = false;
			if (base.Query["m"] != null) { _midPoint = int.Parse(base.Query["m"]); }
			if (base.Query["a1"] != null) { _startOpacity = int.Parse(base.Query["a1"]); }
			if (base.Query["a2"] != null) { _endOpacity = int.Parse(base.Query["a2"]); _endOpacityGiven = true; }

			if (!_endOpacityGiven && base.Query["c2"] != null) {
				// if end color given but no end opacity, default opaque
				_endOpacity = 100;
			}
			transparency = (_startOpacity < 100 || _endOpacity < 100);
		   
			if (!this.IsCached) {
				if (base.Query["c1"] != null) {
					_startColor = ColorTranslator.FromHtml("#" + base.Query["c1"]);
				} else {
					// nothing to draw without color
					return;
				}
				if (base.Query["c2"] != null) {
					_endColor = ColorTranslator.FromHtml("#" + base.Query["c2"]);
				} else {
					// if no end color specified then fade to transparent
					if (!_endOpacityGiven) { _endOpacity = 0; }
					_endColor = _startColor;
				}
			   
				if (_startOpacity < 100)
					_startColor = Draw.Utility.AdjustOpacity(_startColor, _startOpacity);
				if (_endOpacity < 100)
					_endColor = Draw.Utility.AdjustOpacity(_endColor, _endOpacity);

				switch (base.Query["d"]) {
					case "ttb": _angle = 90; break;
					case "rtl": _angle = 180; break;
					case "btt": _angle = 270; break;
					case "ltr": _angle = 0; break;
					case "sunrise":
						_angle = (float)(Math.Atan(this.Width / this.Height) * (180 / Math.PI));
						_orthogonal = false;
						_midPoint = 25;
						break;
					case "sunset":
						_angle = (float)(Math.Atan(this.Height / this.Width) * (180 / Math.PI) + 90);
						_orthogonal = false;
						_midPoint = 25;
						break;
					default:
						//_angle = Integer.Parse(.QueryString("d"))
						//If _angle = Nothing Then _angle = 0
						_angle = 0;
						break;
				}
				this.MakeGraphic();
				base.ServerCache();
			}
			base.Save(transparency);
		}
	   
		protected override void MakeGraphic() {
			Graphics graphic = Graphics.FromImage(this.Bitmap);
			Rectangle background = new Rectangle(0, 0, this.Width, this.Height);
			LinearGradientBrush gb = new LinearGradientBrush(background, _startColor, _endColor, _angle);
		   
			if (_midPoint != 50) {
				ColorBlend cb = new ColorBlend();
				Color midColor;
				float midBlend;
			   
				if (_midPoint < 50) {
					midColor = _endColor;
					midBlend = (float)_midPoint / 50;
				} else {
					midColor = _startColor;
					midBlend = 1 - (float)_midPoint / 50;
				}
				cb.Positions = new float[] {0, midBlend, 1};
				cb.Colors = new Color[] {_startColor, midColor, _endColor};
				gb.InterpolationColors = cb;
			}
			graphic.FillRectangle(gb, background);
			gb.Dispose();
		}
	}
}
