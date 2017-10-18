using System;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public interface ISelect : IInputControl {

		string[] Selections { get; set; }
		bool ShowLink { get; set; }
		//ReadOnly Property BitMask() As Integer
		string Selection { get; }
		bool Posted { get; }
		int Rows { set; }
	}
}