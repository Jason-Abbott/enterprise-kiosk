using Idaho.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Idaho {
	/// <summary>
	/// A dictionary of values that can be sorted
	/// </summary>
	/// <remarks>
	/// The .NET SortedList and SortedDictionary seem to have troubles with mis-identifying
	/// whether a given key is present or not. Also, sorting while items are added, as they
	/// do, causes complications.
	/// </remarks>
	public class SortedDictionary<K, V> : List<KeyValuePair<K, V>>, Web.IScriptable {
		private bool _sortWhileAdding = false;
		private IComparer<K> _comparer = null;
		private bool _resolveKeyConflicts = false;
		private int _resolveTries = 0;
		private int _maximumResolveTries = 100;

		public delegate KeyValuePair<K, V> KeyValue<T>(T e);

		#region Properties

		public IComparer<K> Comparer { get { return _comparer; } set { _comparer = value; } }

		/// <summary>
		/// Attempt to automatically resolve key conflicts
		/// </summary>
		public bool ResolveKeyConflicts {
			set { _resolveKeyConflicts = value; }
		}

		/// <summary>
		/// Find highest key value among members
		/// </summary>
		public K NextAvailableKey {
			get {
				if (typeof(K) == typeof(int)) {
					int max = 0;
					int key = 0;

					foreach (KeyValuePair<K, V> p in this) {
						key = System.Convert.ToInt32(p.Key);
						if (key > max) { max = key; }
					}
					max++;
					return (K)(object)max;
				} else {
					throw new InvalidCastException("Cannot find maximum for non-numeric keys");
				}
			}
		}

		/// <summary>
		/// Value at given position
		/// </summary>
		public new V this[int index] {
			get { return base[index].Value; }
			set { base[index].Value = value; }
		}

		/// <summary>
		/// Value for given key
		/// </summary>
		public V this[K key] {
			get {
				foreach (KeyValuePair<K, V> pair in this) {
					if (pair.Key.Equals(key)) { return pair.Value; }
				}
				return default(V);
			}
			set {
				foreach (KeyValuePair<K, V> pair in this) {
					if (pair.Key.Equals(key)) { pair.Value = value; }
				}
			}
		}
		/// <summary>
		/// All keys in the dictionary
		/// </summary>
		public List<K> Keys {
			get {
				List<K> list = new List<K>();
				foreach (KeyValuePair<K, V> pair in this) {
					list.Add(pair.Key);
				}
				return list;
			}
		}
		/// <summary>
		/// All values in the dictionary
		/// </summary>
		public List<V> Values {
			get {
				List<V> list = new List<V>();
				foreach (KeyValuePair<K, V> pair in this) {
					list.Add(pair.Value);
				}
				return list;
			}
		}

		#endregion

		public SortedDictionary() : this(false) { }
		public SortedDictionary(bool sortWhileAdding) { _sortWhileAdding = sortWhileAdding; }

		#region Factory

		/// <summary>
		/// Construct sorted dictionary from a list using a delegate to create pairs
		/// </summary>
		public static SortedDictionary<K, V> Load<T>(List<T> list, KeyValue<T> fn) {
			SortedDictionary<K, V> sorted = new SortedDictionary<K, V>();
			foreach (T t in list) { sorted.Add(fn(t)); }
			return sorted;
		}

		/// <summary>
		/// Construct sorted dictionary from SQL object and key/value field names
		/// </summary>
		public static SortedDictionary<K, V> Load(Data.Sql sql, string keyField, string valueField) {
			SortedDictionary<K, V> sorted = new SortedDictionary<K, V>();
			SqlReader reader = sql.GetReader(true);
			while (reader.Read()) {
				sorted.Add(reader.GetValue<K>(keyField), reader.GetValue<V>(valueField));
			}
			return sorted;
		}
		/// <summary>
		/// Construct sorted dictionary from SQL object and key/value field name
		/// </summary>
		public static SortedDictionary<K, V> Load(Data.Sql sql, string field) {
			return Load(sql, field, field);
		}

		#endregion

		public void Add(K key, V value) {
			if (!this.Contains(key)) {
				this.Add(new KeyValuePair<K, V>(key, value));
				return;
			} else if (_resolveKeyConflicts && _resolveTries < _maximumResolveTries) {
				object newKey = null;

				if (key is int) {
					newKey = this.NextAvailableKey;
				} else if (key is Guid) {
					newKey = Guid.NewGuid();
				} else if (key is string) {
					newKey = System.Convert.ToString(key) + "b";
				}
				if (newKey != null) { _resolveTries++; this.Add((K)newKey, value); }
				return;
			}
			throw new DuplicateNameException("Cannot add \"" + value.ToString() +
				"\" because key " + key + " is already assigned to \"" + this[key] + "\"");
		}

		/// <summary>
		/// Add values from another dictionary to this one
		/// </summary>
		public void Add(SortedDictionary<K, V> list) {
			foreach (KeyValuePair<K, V> value in list) { this.Add(value); }
		}

		/// <summary>
		/// Remove item with given key
		/// </summary>
		/// <param name="key"></param>
		public void Remove(K key) {
			KeyValuePair<K, V> match = null;

			foreach (KeyValuePair<K, V> pair in this) {
				if (pair.Key.Equals(key)) { match = pair; break; }
			}
			if (match != null) { base.Remove(match); }
		}

		/// <summary>
		/// Remove all listed items
		/// </summary>
		public void Remove(SortedDictionary<K, V> remove) {
			if (remove != null) {
				foreach (KeyValuePair<K, V> p in remove) { this.Remove(p.Key); }
			}
		}

		/// <summary>
		/// Does list contain item with given key
		/// </summary>
		public bool Contains(K key) {
			foreach (KeyValuePair<K, V> pair in this) {
				if (pair.Key.Equals(key)) { return true; }
			}
			return false;
		}

		public new void Sort() {
			if (_comparer != null) { base.Sort(new KeyValueComparer(_comparer)); }
			this.Sort();
		}
		public void Sort(IComparer<K> comparer) {
			base.Sort(new KeyValueComparer(comparer));
		}
		public void SortValue(IComparer<V> comparer) {
			if (_comparer != null) { this.Sort(new KeyValueComparer(_comparer)); }
			this.Sort();
		}

		/// <summary>
		/// Wrapper comparer to simplify comparison to just key or value
		/// </summary>
		internal class KeyValueComparer : System.Collections.Generic.IComparer<KeyValuePair<K, V>> {
			IComparer<K> _keyComparer = null;
			IComparer<V> _valueComparer = null;
			bool _compareValue = false;

			internal KeyValueComparer(IComparer<K> comparer) { _keyComparer = comparer; }
			internal KeyValueComparer(IComparer<V> comparer) {
				_valueComparer = comparer;
				_compareValue = true;
			}

			public int Compare(KeyValuePair<K, V> pair1, KeyValuePair<K, V> pair2) {
				if (_compareValue) {
					return _valueComparer.Compare(pair1.Value, pair2.Value);
				} else {
					return _keyComparer.Compare(pair1.Key, pair2.Key);
				}
			}
		}

		#region IScriptable

		public string ToJSON() {
			StringBuilder script = new StringBuilder("[");
			foreach (KeyValuePair<K, V> p in this) {
				script.Append(p.ToJSON());
				script.Append(",");
			}
			script.TrimEnd();
			script.Append("]");
			return script.ToString();
		}

		#endregion
	}
}
