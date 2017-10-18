using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	/// <summary>
	/// A single name and value
	/// </summary>
	public class NameValue<N, V> {
		private N _name;
		private V _value;

		public N Name { get { return _name; } set { _name = value; } }
		public V Value { get { return _value; } set { _value = value; } }

		public bool IsEmpty {
			get {
				return (_name == null || _name.Equals(default(N)))
					&& (_value == null || _value.Equals(default(V)));
			}
		}
		public bool IsValid {
			get {
				return !(_name == null || _name.Equals(default(N)))
					&& !(_value == null || _value.Equals(default(V))); }
		}
	}
}
