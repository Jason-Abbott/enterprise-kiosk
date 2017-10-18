using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	/// <summary>
	/// A brief placeholder for a business entity
	/// </summary>
	[Serializable]
	public class Stub : ILinkable {
		private Guid _id = Guid.Empty;
		private string _name = string.Empty;
		private int _dataSource = 0;
		private StubCollection _collection = null;

		#region Properties

		internal StubCollection Collection { set { _collection = value; } }
		public string Name { get { return _name; } set { _name = value; } }
		public Guid ID { get { return _id; } internal set { _id = value; } }

		/// <summary>
		/// For entities that come from multiple data source, identify the source for this one
		/// </summary>
		public int DataSourceID { get { return _dataSource; } set { _dataSource = value; } }

		#endregion

		public Stub() { }
		public Stub(Guid id) { _id = id; }
		public Stub(Guid id, int source) : this(id) { _dataSource = source; }

		#region ILinkable

		public string DetailUrl {
			get { return string.Format(_collection.UrlFormat, _id); }
		}

		public string DetailLink {
			get { return string.Format("<a href=\"{0}\">{1}</a>", this.DetailUrl, _name); }
		}

		#endregion
	}
}
