using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idaho {
	/// <summary>
	/// A collection with items related hierarchically
	/// </summary>
	public interface IHierarchicalCollection<T> : IList<T>
		where T: Entity, IHierarchical<T> {

		/// <summary>
		/// Count of all children, their children, etc.
		/// </summary>
		int TotalCount { get; }

		/// <summary>
		/// Member entity with given GUID
		/// </summary>
		T this[Guid id] { get; }
	}
}
