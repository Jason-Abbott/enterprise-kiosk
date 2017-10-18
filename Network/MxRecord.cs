using System;
using System.Collections.Generic;

namespace Idaho.Network {
	public class MxRecord : IComparable<MxRecord> {

		private int _preference = -1;
		private string _hostName = string.Empty;

		public int Preference { get { return _preference; } set { _preference = value; } }
		public string HostName { get { return _hostName; } set { _hostName = value; } }

		public MxRecord() { _preference = -1; _hostName = string.Empty; }

		internal MxRecord(int preference, string hostName) {
			_preference = preference;
			_hostName = hostName;
		}

		public override string ToString() {
			return string.Format("Preference: {0}, Exchange: {1}", _preference, _hostName);
		}

		/// <summary>
		/// Infer mail server name from given hostname
		/// </summary>
		/// <param name="hostName"></param>
		/// <returns></returns>
		public static MxRecord Infer(string hostName) {
			int atIndex = hostName.IndexOf("@");
			if (atIndex >= 0) { hostName = hostName.Substring(atIndex + 1); }
			string[] parts = hostName.Split('.');
			int x = parts.Length - 1;
			MxRecord mx = new MxRecord();
			mx.HostName = string.Format("{0}.{1}", parts[x - 1], parts[x]);
			mx.Preference = 1;
			return mx;
		}

		public int CompareTo(MxRecord other) {
			if (_preference > other.Preference) {
				return 1;
			} else if (_preference == other.Preference) {
				return string.Compare(_hostName, other.HostName);
			} else {
				return -1;
			}
		}
	}
}