using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Text;

namespace Idaho {
	/// <summary>
	/// All stubs for a particular collection of entities
	/// </summary>
	[Serializable]
	public class StubCollection : List<Stub>, IEditableObject {
		private DateTime _synchronizedOn = DateTime.MinValue;
		[NonSerialized()] private ReaderWriterLock _lock;
		[NonSerialized()] private LockCookie _cookie;
		private string _urlFormat = "default.aspx?id={0}";

		#region Properties

		/// <summary>
		/// The string format to use with ToString
		/// </summary>
		public string UrlFormat {
			set { _urlFormat = value; }
			internal get { return _urlFormat; }
		}

		public Stub this[Guid id] {
			get {
				foreach (Stub s in this) {
					if (s.ID.Equals(id)) { return s; }
				}
				return null;
			}
		}
		public Stub this[string name] {
			get {
				foreach (Stub s in this) {
					if (s.Name.Equals(name, Format.IgnoreCase)) { return s; }
				}
				return null;
			}
		}

		private ReaderWriterLock Lock {
			get {
				if (_lock == null) { _lock = new ReaderWriterLock(); }
				return _lock;
			}
		}

		#endregion

		internal StubCollection(string urlFormat) { _urlFormat = urlFormat; }
		internal StubCollection() { }

		public StubCollection StartsWith(string text) {
			StubCollection matches = new StubCollection();
			foreach (Stub s in this) {
				if (s.Name.StartsWith(text, Format.IgnoreCase)) { matches.Add(s); }
			}
			return matches;
		}

		public new void Add(Stub s) {
			s.Collection = this;
			base.Add(s);
		}
		public StubCollection Create() { return new StubCollection(_urlFormat); }

		#region IEditableObject Members

		public void BeginEdit() {
			_cookie = this.Lock.UpgradeToWriterLock(TimeSpan.FromSeconds(5));
		}

		public void CancelEdit() {
			this.Lock.DowngradeFromWriterLock(ref _cookie);
		}

		public void EndEdit() {
			this.Lock.DowngradeFromWriterLock(ref _cookie);
			_synchronizedOn = DateTime.Now;
		}

		#endregion

	}
}
