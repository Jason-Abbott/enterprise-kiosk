using Idaho.Web.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Events {
	/// <summary>
	/// Used to update dependants for cached controls
	/// </summary>
	public class ControlEventArgs : EventArgs {
		private IAjaxControl _control;
		
		public IAjaxControl Control { get { return _control; } }

		public ControlEventArgs(IAjaxControl c) { _control = c; }
	}
}
