using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	/// <summary>
	/// For entities that can be bound to a selection list
	/// </summary>
	public interface IListable {
		SortedDictionary<string, string> KeysAndValues { get; }
	}
}
