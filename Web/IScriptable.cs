using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idaho.Web {
	public interface IScriptable {
		/// <summary>
		/// Return implementing object in JavaScript Object Notation
		/// </summary>
		string ToJSON();
	}
}
