using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	public class SiteActivityFilter : IFilter {

		private SiteActivity.Types _type = SiteActivity.Types.Empty;
		private Idaho.Network.IpAddress _ipAddress = null;
		private User _user = null;
		private Entity _entity = null;
		private string _note = string.Empty;
		private DateTime _before = DateTime.MaxValue;
		private DateTime _after = DateTime.MinValue;
		
		#region Properties

		public SiteActivity.Types Type { set { _type = value; } get { return _type; } }
		public Idaho.Network.IpAddress IpAddress { set { _ipAddress = value; } get { return _ipAddress; } }
		public User User { set { _user = value; } get { return _user; } }
		public Entity Entity { set { _entity = value; } get { return _entity; } }
		public DateTime Before { set { _before = value; } get { return _before; } }
		public DateTime After { set { _after = value; } get { return _after; } }

		#endregion

		#region IFilter Members

		public bool IsEmpty {
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		#endregion

	}
}
