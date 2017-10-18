using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Web.Ajax {
	/// <summary>
	/// Data for a cached AJAX response
	/// </summary>
	public class CacheItem {

		private string _key = string.Empty;
		private byte[] _value;
		private bool _compressed = false;
		private string _contentType = string.Empty;
		// this must be supplied by the control
		private List<string> _dataTypeNames = new List<string>();

		#region Properties

		/// <summary>
		/// Indicate if the response bytes have been GZIPped
		/// </summary>
		public bool Compressed { get { return _compressed; } set { _compressed = value; } }
		public string ContentType { get { return _contentType; } set { _contentType = value; } }
		public string Key { get { return _key; } set { _key = value; } }
		public byte[] Value { get { return _value; } set { _value = value; } }
		public List<string> DataTypeNames { get { return _dataTypeNames; } set { _dataTypeNames = value; } }

		#endregion

		public CacheItem(string key, byte[] data, bool compressed) {
			_key = key;
			_value = data;
			_compressed = compressed;
		}

		public CacheItem(string key, byte[] data, bool compressed, List<Type> types)
			: this(key, data, compressed) {

			foreach (Type t in types) { _dataTypeNames.Add(t.Name); }
		}
	}
}
