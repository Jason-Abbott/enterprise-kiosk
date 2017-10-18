using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	/// <summary>
	/// Generic event data container
	/// </summary>
	/// <remarks>Limited to containing a single data object</remarks>
	public class EventArgs<T> : EventArgs {
		private T _entity;
		private EventType _type;

		#region Properties

		public T Entity { get { return _entity; } }
		public EventType Type { get { return _type; } }

		#endregion

		public EventArgs(T entity, EventType type) { _entity = entity; _type = type; }
	}
	public enum EventType { Added, Removed, NewValue, Imported }
}
