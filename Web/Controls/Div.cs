using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Web.Controls {
	public class Div : HtmlContainerControl {
		public Div() : base("div") { base.AllowSelfClose = false; }
	}
}
