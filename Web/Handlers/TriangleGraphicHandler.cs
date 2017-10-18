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
	/// Render a triangle graphic image
	/// </summary>
	/// <remarks>To use, first setup a handler in web.config.</remarks>
	/// <example><code>
	/// &lt;add verb="GET" path="images/triangle.ashx"
	///		type="AMP.Handlers.TriangleGraphic,IDCL.System" /&gt;
	///
	/// then, for this example, setup style rules as follows:
	/// background-image: url(../images/triangle.ashx?c=036&a=70&h=20&w=20);
	/// </code></example>
	public class TriangleGraphic : Handlers.Graphic {

		private Color _color;
		private int _alpha = 100;
		private Draw.Triangle.Directions _direction;

		/// <summary>
		/// Build triangle image for output
		/// </summary>
		public override void ProcessRequest(HttpContext context) {
			base.Initialize();
			if (!base.IsCached) {
				if (base.Query["c"] != null) {
					_color = ColorTranslator.FromHtml("#" + base.Query["c"]);
				}
				if (base.Query["a"] != null) { _alpha = int.Parse(base.Query["a"]); }

				switch (base.Query["d"]) {
					case "up": _direction = Draw.Triangle.Directions.Up; break;
					case "down": _direction = Draw.Triangle.Directions.Down; break;
					case "right": _direction = Draw.Triangle.Directions.Right; break;
					case "left": _direction = Draw.Triangle.Directions.Left; break;
					default: _direction = Draw.Triangle.Directions.Right; break;
				}
				this.MakeGraphic();
				base.ServerCache();
			}
			base.Save(true);
		}

		protected override void MakeGraphic() {
			if (_alpha < 100) { _color = Draw.Utility.AdjustOpacity(_color, _alpha); }

			Idaho.Draw.Triangle draw = new Idaho.Draw.Triangle(_direction);
			draw.Width = base.Width;
			draw.Height = base.Height;
			draw.Color.BackGround = System.Drawing.Color.Transparent;
			draw.Color.ForeGround = _color;
			draw.Create();
			this.Bitmap = draw.Bitmap;
		}
	}
}