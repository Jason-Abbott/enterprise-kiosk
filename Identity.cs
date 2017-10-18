using System;
using System.Security.Principal;

namespace Idaho {
	/// <summary>
	/// Implement Principal Identity
	/// </summary>
	[Serializable()]
	public class Identity : IIdentity, IEquatable<Identity> {

		private Idaho.Network.Login _login;
		private bool _authenticated = false;
		private string _authenticationType;

		/// <summary>
		/// The method by which this user was authenticated
		/// </summary>
		/// <remarks>
		/// For ASP.NET, specified by authentication mode in web.config. Values
		/// might be "Forms" or "Windows" authentication.
		/// </remarks>
		public string AuthenticationType { get { return _authenticationType; } }
		public bool IsAuthenticated { get { return _authenticated; } }
		public string Name { get { return _login.ToString(); } }
		public Idaho.Network.Login Login { get { return _login; } }

		#region Constructors

		/// <summary>
		/// Create Identity from generic Identity
		/// </summary>
		public Identity(IIdentity oldIdentity) {
			_authenticated = oldIdentity.IsAuthenticated;
			_authenticationType = oldIdentity.AuthenticationType;
			_login = Idaho.Network.Login.Parse(oldIdentity.Name);
			//this.Synchronized();
		}

		/// <summary>
		/// Create temporary identity
		/// </summary>
		public Identity(Idaho.Network.Login login) {
			_authenticated = false;
			_authenticationType = "None";
			_login = login;
		}

		#endregion

		#region IEquatable

		public bool Equals(Identity other) { return _login.Equals(other.Login); }
		public bool Equals(IIdentity other) {
			return (_login.ToString().ToLower() == other.Name.ToLower());
		}

		#endregion

	}
}