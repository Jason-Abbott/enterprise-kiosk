using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Represents the content of a single tab in a tabbed form
	/// </summary>
	public class Tab : HtmlContainerControl {
		private string _label = string.Empty;

		#region Properties

		/// <summary>
		/// Tab selection text
		/// </summary>
		public string Label { set { _label = value; } internal get { return _label; } }
	

		#endregion

		public Tab() : base("div") { base.AllowSelfClose = true; base.CssClass = "tabContent"; }

	}
}