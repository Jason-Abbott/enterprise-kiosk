using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace Idaho.Web.Handlers {
	/// <summary>
	/// Render a corner graphic image
	/// </summary>
	/// <remarks>To use, first setup a handler in web.config.</remarks>
	/// <example><code>
	/// &lt;add verb="GET" path="images/solid.ashx"
	///		type="AMP.Handlers.SolidGraphic,IDCL.System" /&gt;
	///
	/// then, for this example, setup style rules as follows:
	/// background-image: url(../images/corner.ashx?c=036&a=70&h=20&w=20);
	/// </code></example>
	public class CornerGraphic : Handlers.Graphic {

		private Color _color = Color.Empty;
		private Color _borderColor = Color.Empty;
		private Color _shadowColor = Color.Black;
		private int _radius = 0;
		private int _opacity = 100;
		private int _borderOpacity = 100;
		private int _borderWidth = 2;
		private int _shadowWidth = 0;
		private int _shadowStartOpacity = 50;
		private int _extendWidth = 0;
		private int _extendHeight = 0;
		private Draw.Corner.Positions _orientation;

		/// <summary>
		/// Build corner image for output
		/// </summary>
		public override void ProcessRequest(HttpContext context) {
			base.Initialize();
			if (!base.IsCached) {
				if (base.Query["c"] != null) {
					_color = ColorTranslator.FromHtml("#" + base.Query["c"]);
				}
				if (base.Query["bc"] != null) {
					_borderColor = ColorTranslator.FromHtml("#" + base.Query["bc"]);
				}
				if (base.Query["sc"] != null) {
					_shadowColor = ColorTranslator.FromHtml("#" + base.Query["sc"]);
				}
				if (base.Query["a"] != null) { _opacity = int.Parse(base.Query["a"]); }
				if (base.Query["bo"] != null) { _borderOpacity = int.Parse(base.Query["bo"]); }
				if (base.Query["r"] != null) { _radius = int.Parse(base.Query["r"]); }
				if (base.Query["bw"] != null) { _borderWidth = int.Parse(base.Query["bw"]); }
				if (base.Query["sw"] != null) { _shadowWidth = int.Parse(base.Query["sw"]); base.Width += _shadowWidth; }
				if (base.Query["so"] != null) { _shadowStartOpacity = int.Parse(base.Query["so"]); }

				// extensions
				if (base.Query["ew"] != null) { _extendWidth = int.Parse(base.Query["ew"]); }
				if (base.Query["eh"] != null) { _extendHeight = int.Parse(base.Query["eh"]); }

				switch (base.Query["o"]) {
					case "tl": _orientation = Draw.Corner.Positions.TopLeft; break;
					case "tr": _orientation = Draw.Corner.Positions.TopRight; break;
					case "br": _orientation = Draw.Corner.Positions.BottomRight; break;
					case "bl": _orientation = Draw.Corner.Positions.BottomLeft; break;
					default: _orientation = Draw.Corner.Positions.TopLeft; break;
				}
				this.MakeGraphic();
				base.ServerCache();
			}
			base.Save(true);
		}

		protected override void MakeGraphic() {
			if (_opacity < 100) { _color = Draw.Utility.AdjustOpacity(_color, _opacity); }
			if (_borderColor == Color.Empty) { _borderColor = _color; }
			if (_borderOpacity < 100) {
				_color = Draw.Utility.AdjustOpacity(_borderColor, _borderOpacity);
			}
			if (_shadowStartOpacity < 100) {
				_shadowColor = Draw.Utility.AdjustOpacity(_shadowColor, _shadowStartOpacity);
			}
			Idaho.Draw.Corner draw = new Idaho.Draw.Corner();
			draw.Position = _orientation;
			draw.Radius = _radius;
			// default height and width match radius
			draw.Width = draw.Radius + _shadowWidth;
			draw.Height = draw.Radius + _shadowWidth;
			draw.ExtendWidth = _extendWidth;
			draw.ExtendHeight = _extendHeight;
			draw.ShadowWidth = _shadowWidth;
			if (_shadowColor != Color.Empty) { draw.Color.Shadow = _shadowColor; }
			draw.Color.BackGround = System.Drawing.Color.Transparent;
			draw.Color.ForeGround = _color;
			draw.Color.Border = _borderColor;
			draw.BorderWidth = _borderWidth;
			draw.Create();
			this.Bitmap = draw.Bitmap;
		}
	}
}