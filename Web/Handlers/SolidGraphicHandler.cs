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
	/// Render a solid color image
	/// </summary>
	/// <remarks>To use, first setup a handler in web.config.</remarks>
	/// <example><code>
	/// &lt;add verb="GET" path="images/solid.ashx"
	///		type="AMP.Handlers.SolidGraphic,IDCL.System" /&gt;
	///
	/// then, for this example, setup style rules as follows:
	/// background-image: url(../images/solid.ashx?c=036&a=70&h=20&w=20);
	/// </code></example>
	public class SolidGraphic : Handlers.Graphic {
		
		private int _alpha = 100;

		#region Static Fields

		private static Color _color;
		public static Color DefaultColor { set { _color = value; } }

		#endregion

		/// <summary>
		/// Build solid image for output
		/// </summary>
		public override void ProcessRequest(HttpContext context) {
			base.Initialize();
			if (!base.IsCached) {
				if (base.Query["c"] != null) {
					_color = ColorTranslator.FromHtml("#" + base.Query["c"]);
				}
				if (base.Query["a"] != null) {
					_alpha = int.Parse(base.Query["a"]);
				}
				this.MakeGraphic();
				base.ServerCache();
			}
			base.OutputCache();
		}

		protected override void MakeGraphic() {
			Graphics graphic = Graphics.FromImage(this.Bitmap);
			if (_alpha < 100) { _color = Draw.Utility.AdjustOpacity(_color, _alpha); }
			Rectangle background = new Rectangle(0, 0, this.Width, this.Height);
			SolidBrush brush = new SolidBrush(_color);
			graphic.FillRectangle(brush, background);
			graphic.Dispose();
		}
	}
}