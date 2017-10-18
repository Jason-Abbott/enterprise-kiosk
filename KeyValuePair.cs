using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	/// <summary>
	/// Key-value pair
	/// </summary>
	/// <remarks>
	/// The .Net type of the same name makes the Value property read-only.
	/// This overcomes that.
	/// </remarks>
	public class KeyValuePair<K, V> : Web.IScriptable {
		private K _key = default(K);
		private V _value = default(V);

		public K Key { get { return _key; } set { _key = value; } }
		public V Value { get { return _value; } set { _value = value; } }

		public KeyValuePair() { }
		public KeyValuePair(K key) { _key = key; }
		public KeyValuePair(K key, V value) : this(key) { _value = value; }

		public string ToJSON() {
			return string.Format("[{0},{1}]", _key.ToJSON(), _value.ToJSON());
		}
	}
}
