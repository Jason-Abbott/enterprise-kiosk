using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.UI;

namespace Idaho.Web.Handlers {
	/// <summary>
	/// Render a solid color image
	/// </summary>
	/// <remarks>To use, first setup a handler in web.config.</remarks>
	/// <example><code>
	/// &lt;add verb="GET" path="images/text.ashx"
	///		type="IDCL.Handlers.TextGraphic,IDCL.System" /&gt;</code>
	///
	/// then, for this example, setup style rules as follows:
	/// <code>background-image: url(../images/text.ashx?t=Text&f=Arial);</code>
	/// </example>
	public class TextGraphic : Handlers.Graphic {

		private string _text;
		private Color _textColor = Color.Black;
		private string _fontName = "Tahoma";
		private int _opacity = 100;
		private float _angle = 0;
		private int _size = 10;

		/// <summary>
		/// Build solid image for output
		/// </summary>
		/// <param name="context"></param>
		public override void ProcessRequest(HttpContext context) {
			base.Initialize();
			bool transparency = false;

			if (base.Query["a"] != null) { _opacity = int.Parse(base.Query["a"]); }
			transparency = _opacity < 100;

			if (!this.IsCached) {
				_text = base.Query["t"];

				if (base.Query["f"] != null) { _fontName = base.Query["f"]; }
				if (base.Query["c"] != null) {
					_textColor = ColorTranslator.FromHtml("#" + base.Query["c"]);
				}
				if (transparency) {
					_textColor = Draw.Utility.AdjustOpacity(_textColor, _opacity);
				}
				if (base.Query["g"] != null) {
					_angle = float.Parse(base.Query["g"]);
				}
				if (base.Query["s"] != null) {
					_size = int.Parse(base.Query["s"]);
				}

				this.MakeGraphic();
				base.ServerCache();
			}
			base.Save(true);
		}

		protected override void MakeGraphic() {
			Idaho.Draw.TextGraphic draw = new Idaho.Draw.TextGraphic();
			draw.Text = _text;
			draw.Rotate = _angle;
			draw.Color.Text = _textColor;
			draw.FontName = _fontName;
			draw.Size = _size;
			draw.Generate();
			this.Bitmap = draw.Bitmap;
		}
	}
}