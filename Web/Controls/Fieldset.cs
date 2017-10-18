using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Web.Controls {
	public class FieldSet : HtmlContainerControl {
		public FieldSet() : base("fieldset") { base.AllowSelfClose = false; }
	}
}