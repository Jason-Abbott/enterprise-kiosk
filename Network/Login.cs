using System;
using System.Web;

namespace Idaho.Network {
	/// <summary>
	/// Current logon information
	/// </summary>
	/// <remarks>Basically just username and domain</remarks>
	[Serializable()]
	public class Login : IEquatable<Login> {
	   
		private string _name = string.Empty;
		private string _domain = string.Empty;
		private static string _defaultDomain = string.Empty;
		private const char _splitOn = '\\';
	   
		#region Properties

		public static string DefaultDomain { set { _defaultDomain = value; } }
		/// <summary>
		/// Network name without domain
		/// </summary>
		public string Name { get { return _name; } set { _name = value; } }
		public string Domain { get { return _domain; } set { _domain = value; } }
	   
		#endregion

		public Login(string name) {
			_name = name.ToLower();
			_domain = _defaultDomain;
		}
		public Login(string name, string domain) {
			_name = name.ToLower();
			_domain = domain;
		}
		private Login() { }
	   
		public static Login Parse(string raw) {
			if (string.IsNullOrEmpty(raw)) { return null; }
			Login login = new Login();
		   
			if (raw.IndexOf(_splitOn) > 0) {
				string[] pair = raw.Split(_splitOn);
				login.Domain = pair[0];
				login.Name = pair[1];
			} else { login.Name = raw; }
			return login;
		}
	   
		public static Login Current {
			get {
				return Login.Parse(HttpContext.Current.Request.ServerVariables["LOGON_USER"]);
			}
		}
	   
		public override string ToString() {
			if (_domain != null) {
				return string.Format("{0}{1}{2}", _domain, _splitOn, _name);
			} else { return _name; }
		}
	   
		public bool Equals(Login other) {
			return _name == other.Name && _domain == other.Domain;
		}
	}
}
