using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	public class UniqueStringList : List<string> {
		public new void Add(string text) { if (!this.Contains(text)) { base.Add(text); } }
	}
}
