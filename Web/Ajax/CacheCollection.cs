using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Web.Ajax {
	/// <summary>
	/// Collection of cached AJAX responses (output cache)
	/// </summary>
	/// <remarks>
	/// Includes mechanism for expiring cached items based on underlying
	/// data changes.
	/// </remarks>
	public class CacheCollection : List<CacheItem> {

		public CacheItem this[string key] {
			get {
				foreach (CacheItem i in this) {
					if (i.Key.Equals(key)) { return i; }
				}
				return null;
			}
		}

		/// <summary>
		/// Remove the specified item from the collection
		/// </summary>
		public void Expire(string typeName) {
			CacheCollection match = new CacheCollection();

			foreach (CacheItem i in this) {
				if (i.DataTypeNames.Contains(typeName)) { match.Add(i); break; }
			}
			this.Remove(match);
		}
		
		/// <summary>
		/// Respond to entity collection change
		/// </summary>
		/// <param name="source">If wired correctly, should be an EntityCollection</param>
		/// <param name="e">Null event arguments</param>
		public void OnControlDataChange(object source, EventArgs e) {
			this.Expire(source.GetType().Name);
		}
		
		public void Add(string key, byte[] value, bool compressed) {
			this.Add(new CacheItem(key, value, compressed));
		}
		public void Add(string key, byte[] value, bool compressed, List<Type> dependencies) {
			this.Add(new CacheItem(key, value, compressed, dependencies));
		}

		public void Remove(CacheCollection c) {
			if (c != null) { foreach (CacheItem i in c) { this.Remove(i); } }
		}
		public bool Contains(string key) {
			foreach (CacheItem i in this) {
				if (i.Key.Equals(key)) { return true; }
			}
			return false;
		}
	}
}
