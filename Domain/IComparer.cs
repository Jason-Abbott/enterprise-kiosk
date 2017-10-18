using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	public interface IComparer<T> : System.Collections.Generic.IComparer<T> {
		Entity.SortDirections Direction { get; set; }
	}
}
