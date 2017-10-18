using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class Label : HtmlControl {
		private string _format = string.Empty;
		private string _text = string.Empty;
		private string _tagName = "span";
		private DateTime _date = DateTime.MinValue;

		#region Properties

		public string Format { set { _format = value; } }
		public string Text { set { _text = value; } }
		public new string TagName { set { _tagName = value; } }

		#endregion

		protected override void Render(HtmlTextWriter writer) {
			bool style = this.HasStyle;

			if (style) {
				writer.Write("<");
				writer.Write(_tagName);
				base.RenderAttributes(writer);
				writer.Write(">");
			}
			if (string.IsNullOrEmpty(_format)) {
				writer.Write(_text);
			} else {
				writer.Write(_format, _text);
			}
			if (style) {
				writer.Write("</");
				writer.Write(_tagName);
				writer.Write(">");
			}

			base.Render(writer);
		}
	}
}
