using System;
using System.Collections.Generic;
using System.Reflection;

namespace Idaho.Web.Controls {
	public interface IAjaxControl {
		AjaxBase Ajax { get; }
		void RenderControl(System.Web.UI.HtmlTextWriter writer);
		string ID { get; set; }
		string CssClass { set; }
		string CssStyle { set; }
		// these duplicate what's in AjaxBase but are necessary
		// for declarative property setting
		AjaxBase.RenderModes RenderMode { set; }
		string BindProperty { set; }
	}
}
