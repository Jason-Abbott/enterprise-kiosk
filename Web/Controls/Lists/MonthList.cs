using System;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Render a drop-down list of months
	/// </summary>
	public class MonthList : Idaho.Web.Controls.SelectList {

		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			this.RenderLabel(writer);
			this.RenderBeginTag(writer);
			for (int x = 1; x <= 12; x++) {
				this.RenderOption(
					(new DateTime(1973, x, 1)).ToString("MMMM"), x, writer);
			}
			this.RenderEndTag(writer);
		}
	}
}
