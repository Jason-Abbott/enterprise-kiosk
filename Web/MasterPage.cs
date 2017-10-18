using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Idaho.Web {
	public abstract class MasterPage : System.Web.UI.MasterPage {

		#region Properties

		public abstract PlaceHolder ScriptBlock { get; }
		public abstract PlaceHolder StyleBlock { get; }
		public abstract PlaceHolder ScriptFiles { get; }
		public abstract PlaceHolder StyleSheets { get; }
		public abstract Controls.Title Title { get; }

		#endregion
		
	}
}
