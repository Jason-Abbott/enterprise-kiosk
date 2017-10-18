using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idaho {
	/// <summary>
	/// For entities related hierarchically
	/// </summary>
	public interface IHierarchical<T> where T: Entity, IHierarchical<T> {
		T Parent { get; }
		IHierarchicalCollection<T> Children { get; }
	}
}
