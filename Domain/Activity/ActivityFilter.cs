using System;
using System.Collections.Generic;
using System.Text;

namespace AMP {
	public class ActivityFilter : IFilter {

		private Activity.Types _type = Activity.Types.Empty;
		private AMP.Network.IpAddress _ipAddress;
		private User _user;
		private Entity _entity;
		private string _note = string.Empty;
		private Tau _before = Tau.MaxValue;
		private Tau _after = Tau.MinValue;
		
		#region Properties

		public Activity.Types Type { set { _type = value; } get { return _type; } }
		public AMP.Network.IpAddress IpAddress { set { _ipAddress = value; } get { return _ipAddress; } }
		public User User { set { _user = value; } get { return _user; } }
		public Entity Entity { set { _entity = value; } get { return _entity; } }
		public Tau Before { set { _before = value; } get { return _before; } }
		public Tau After { set { _after = value; } get { return _after; } }

		#endregion

		#region IFilter Members

		public bool IsEmpty {
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		#endregion

	}
}
