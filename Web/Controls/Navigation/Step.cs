using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Represents the content of a single step in a multi-step workflow
	/// </summary>
	public class Step : HtmlContainerControl {
		private string _label = string.Empty;
		//private int _linkID = -1;
		private List<Button> _buttons = null;

		#region Properties

		/// <summary>
		/// Text to display above step area
		/// </summary>
		public string Label { set { _label = value; } get { return _label; } }

		/// <summary>
		/// Used to easily identify this step for linking from other pages
		/// </summary>
		/// <remarks>
		/// Could use regular HTML ID but an INT makes it simpler
		/// </remarks>
		//public int LinkID { set { _linkID = value; } get { return _linkID; } }
		
		public List<Button> Buttons {
			get {
				if (_buttons == null) { _buttons = new List<Button>(); }
				return _buttons;
			}
		}

		#endregion

		public Step() : base("fieldset") { base.AllowSelfClose = true; }
		
	}
}
