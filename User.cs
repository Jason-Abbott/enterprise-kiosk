using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Idaho {
	/// <summary>
	/// A user of an application
	/// </summary>
	[Serializable()]
	public abstract class User : Person, IPrincipal, IComparable<User> {

		[NonSerialized()] private string[] _roles = new string[] { };
		[NonSerialized()] private string[] _permissions = new string[] { };
		private Idaho.Network.IpAddress _lastIpAddress;
		private Idaho.Identity _identity;

		#region Properties

		public Idaho.Network.IpAddress LastIpAddress {
			get { return _lastIpAddress; }
			set { _lastIpAddress = value; }
		}

		//protected Idaho.Identity Identity { set { _identity = value; } }

		protected string[] Roles {
			get {
				if (_roles == null) { _roles = this.GetRoles(); }
				return _roles;
			}
			set { _roles = value; }
		}

		private string[] Permissions {
			get {
				if (_permissions == null) { _permissions = this.GetPermissions(); }
				return _permissions;
			}
		}

		#endregion

		#region Constructors

		protected User(TimeSpan _cacheDuration) : base(_cacheDuration) { }
		protected User(IIdentity identity) { _identity = new Idaho.Identity(identity); }
		protected User(IIdentity identity, TimeSpan cacheDuration) : base(cacheDuration) {
			_identity = new Idaho.Identity(identity);
		}
		/// <summary>
		/// Create a new user with an optional new ID
		/// </summary>
		protected User(bool createID) : base(createID) { }

		/// <summary>
		/// Construct an object for an existing user with given ID
		/// </summary>
		protected User(Guid id) : base(id) { }
		protected User(Guid id, TimeSpan cacheDuration) : base(id, cacheDuration) { }

		#endregion

		#region IPrincipal

		public IIdentity Identity {
			get { return _identity; }
			protected set { _identity = new Idaho.Identity(value); }
		}
		public bool IsInRole(string r) {
			return (Array.BinarySearch<string>(_roles, r) > -1);
		}

		#endregion

		#region Permissions

		/// <summary>
		/// Subclass must load permissions from data store
		/// </summary>
		protected abstract string[] GetPermissions();

		public bool HasPermission(string permission) {
			return (Array.BinarySearch<string>(this.Permissions, permission) > -1);
		}

		#endregion

		#region Roles

		/// <summary>
		/// Subclass must load roles from data store
		/// </summary>
		protected abstract string[] GetRoles();

		/// <summary>
		/// Add a new role to the user
		/// </summary>
		public void AddRole(string role) {
			if (this.Roles == null) {
				_roles = new string[] { role };
			} else {
				List<string> list = new List<string>();
				foreach (string r in _roles) { list.Add(r); }
				list.Add(role);
				// must be sorted for BinarySearch()
				_roles = list.ToArray();
				Array.Sort<string>(_roles);
			}
		}

		/// <summary>
		/// Does the user belong to any of the listed roles
		/// </summary>
		public bool IsInAnyRole(string[] roles) {
			foreach (string r in roles) {
				if (this.IsInRole(r)) { return true; }
			}
			return false;
		}

		/// <summary>
		/// Does the user belong to exactly the listed roles
		/// </summary>
		public bool IsOnlyInRoles(string[] roles) {
			foreach (string r in _roles) {
				if (Array.BinarySearch<string>(roles, r) > -1) {
					// user belongs to role (r) not in listed roles
					return false;
				}
			}
			return true;
		}
		public bool IsOnlyInRoles(string role) {
			return this.IsOnlyInRoles(new string[] { role });
		}

		/// <summary>
		/// Does user belong to all listed roles
		/// </summary>
		public bool IsInAllRoles(string[] roles) {
			foreach (string r in roles) {
				if (!this.IsInRole(r)) { return false; }
			}
			return true;
		}

		/// <summary>
		/// Return roles as string list
		/// </summary>
		/// <remarks>Usually for cache key</remarks>
		public string RoleList() { return string.Join(",", _roles); }

		#endregion

		#region IComparable

		public int CompareTo(User other) {
			int result = 0;

			result = this.LastName.CompareTo(other.LastName);
			if (result == 0) { result = this.FirstName.CompareTo(other.FirstName); }
			if (result == 0) { result = this.Identity.Name.CompareTo(other.Identity.Name); }
			return result;
		}

		#endregion

	}
}