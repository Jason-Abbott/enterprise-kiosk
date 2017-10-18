using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public interface IInputControl : IPostBackDataHandler {

		string Resx { set; }
		Validation.Types ValidationType { get; set; }
		string ValidationAlert { get; set; }
		string Label { get; set; }
		string Note { get; set; }
		bool ShowLabel { get; set; }
		bool Required { get; set; }
		bool HasFocus { set; }
		InputControl.NoteDisplay ShowNote { get; set; }
	}
}
