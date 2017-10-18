using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.Globalization;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;

namespace Idaho.Data {
	/// <summary>
	/// Wrap Active Directory functionality
	/// </summary>
	/// http://www.simple-talk.com/dotnet/.net-framework/building-active-directory-wrappers-in-.net/
	/// http://zytrax.com/books/ldap/apa/search.html
	public class ActiveDirectory {
		private static DirectoryEntry _root;
		private static DateTime _connectedOn;
		private static object _lockOn = new object();
		private static TimeSpan _timeout = TimeSpan.FromSeconds(3);
		private SearchResult _result;

		#region Static Fields

		private static TimeSpan _expireAfter = TimeSpan.MaxValue;
		private static string _serverPath = string.Empty;
		private static string _alternateServerPath = string.Empty;
		private static NetworkCredential _credentials = null;

		public static TimeSpan ExpireAfter { set { _expireAfter = value; } }
		public static string ServerPath { set { _serverPath = value; } }
		public static string AlternateServerPath { set { _alternateServerPath = value; } }
		public static NetworkCredential Credentials {
			set { _credentials = value; } get { return _credentials; } }

		#endregion

		#region Properties

		public TimeSpan Timeout { set { _timeout = value; } }

		public T Value<T>(string field) {
			if (_result != null && _result.Properties.Contains(field)) {
				return (T)_result.Properties[field][0];
			}
			return default(T);
		}
		public T Value<T>(SearchResult r, string field) {
			_result = r;
			return this.Value<T>(field);
		}

		/// <summary>
		/// List values matching pattern in given field
		/// </summary>
		/// <param name="pattern"></param>
		public List<string> List(string field, string pattern) {
			List<string> list = new List<string>();
			if (_result != null && _result.Properties.Contains(field)) {
				Regex re = new Regex(pattern);
				Match m;
				foreach (string g in _result.Properties[field]) {
					if (re.IsMatch(g)) { m = re.Match(g); list.Add(m.Groups[1].Value); }
				}
			}
			return list;
		}
		public List<string> List(SearchResult r, string field, string pattern) {
			_result = r;
			return this.List(field, pattern);
		}

		#endregion

		#region Constructors

		public ActiveDirectory() {
			if (NeedConnection()) {
				string[] path;

				if (!string.IsNullOrEmpty(_alternateServerPath)) {
					path = new string[] { _serverPath, _alternateServerPath };
				} else {
					path = new string[] { _serverPath };
				}
				Connect(path);
			} else {
				_connectedOn = DateTime.Now;
			}
		}
		public ActiveDirectory(string serverPath) {
			Connect(new string[] { serverPath });
		}
		public ActiveDirectory(string serverPath, string userName, string password) {
			_credentials = new NetworkCredential(userName, password);
			Connect(new string[] { serverPath });
		}

		#endregion

		#region Custom Searches

		// Users

		public bool LocateUser(Idaho.Network.Login networkName) {
			if (networkName == null) { return false; }
			string filter = Resource.SayFormat("LDAP_UserLogin", networkName.Name);
			return this.LoadEntry(filter, Fields.User);
		}
		public bool LocateUser(string name) {
			if (string.IsNullOrEmpty(name)) { return false; }
			string filter = Resource.SayFormat("LDAP_UserName", name);
			return this.LoadEntry(filter, Fields.User);
		}
		public bool LocateUser(Guid id) {
			if (id.Equals(Guid.Empty)) { return false; }
			string filter = Resource.SayFormat("LDAP_UserID", GuidToString(id));
			return this.LoadEntry(filter, Fields.User);
		}
		/// <summary>
		/// All users in the directory
		/// </summary>
		public SearchResultCollection Users(string[] fields) {
			string filter = Resource.Say("LDAP_Users");
			return this.GetEntries(filter, fields);
		}
		public SearchResultCollection Users() { return this.Users(Fields.User); }

		/// <summary>
		/// All users of a given directory group
		/// </summary>
		/// http://www.petri.co.il/ldap_search_samples_for_windows_2003_and_exchange.htm
		public SearchResultCollection GroupUsers(string groupName) {
			string filter = Resource.SayFormat("LDAP_GroupUsers", groupName);
			return this.GetEntries(filter, Fields.User);
		}

		/// <summary>
		/// All organizational units that contain users
		/// </summary>
		public SearchResultCollection OrganizationalUnits(string[] fields) {
			string filter = Resource.Say("LDAP_UserOrganizations");
			string path = string.Empty;
			SearchResultCollection users = 
				this.GetEntries(filter, Fields.OrganizationalUnit);

			if (users.Count > 0) {
				// find the organizations these user ou's belong to
				// OU=Users,OU=Boise,DC=tax,DC=state,DC=id,DC=us
				Regex re = new Regex("OU=([^,]+),", RegexOptions.IgnoreCase);
				MatchCollection matches = null;
				List<string> names = new List<string>();
				string value = string.Empty;

				foreach (SearchResult r in users) {
					path = this.Value<string>(r, Fields.FullPath);
					if (!string.IsNullOrEmpty(path)) {
						matches = re.Matches(path);
						
						foreach (Match m in matches) {
							value = m.Groups[1].Captures[0].Value;
							if (value != "Users" && !names.Contains(value)) {
								names.Add(value);
							}
						}
					}
				}
				if (names.Count > 0) {
					// build filter for user containing organizations
					// (&(objectClass=organizationalUnit)(|(ou=Boise)(ou=Boise)))
					filter = string.Empty;
					foreach (string n in names) {
						filter += string.Format("(ou={0})", n);
					}
					filter = string.Format("(&(objectClass=organizationalUnit)(|{0}))", filter);
					return this.GetEntries(filter, fields);
				}
			}
			return null;
		}
		public SearchResultCollection OrganizationalUnits() {
			return this.OrganizationalUnits(Fields.OrganizationalUnit);
		}


		// Groups

		/// <summary>
		/// All groups in the directory
		/// </summary>
		public SearchResultCollection Groups(string[] fields) {
			string filter = Resource.Say("LDAP_Groups");
			return this.GetEntries(filter, fields);
		}
		public SearchResultCollection Groups() { return this.Groups(Fields.Group); }

		public bool LocateGroup(Guid id) {
			if (id.Equals(Guid.Empty)) { return false; }
			string filter = Resource.SayFormat("LDAP_GroupID", GuidToString(id));
			return this.LoadEntry(filter, Fields.Group);
		}

		public bool LocateGroup(string name) {
			if (string.IsNullOrEmpty(name)) { return false; }
			string filter = Resource.SayFormat("LDAP_GroupName", name);
			return this.LoadEntry(filter, Fields.Group);
		}

		#endregion

		#region Connection

		/// <summary>
		/// Get connection to AD root, from singleton if available
		/// </summary>
		/// <remarks>
		/// connectivity needs usually come in bursts so use cached
		/// connection for up to the number of seconds specified in web.config
		/// </remarks>
		private static void Connect(string[] paths) {
			if (NeedConnection()) {
				// allow only one thread in this section at a time
				if (Monitor.TryEnter(_lockOn, _timeout) && (NeedConnection())) {
					// dispose of expired connection, if present
					if (_root != null) { _root.Close(); _root.Dispose(); }
					bool success = false;
					int x = 0;

					while (paths.Length > x && !success) {
						try {
							_root = new DirectoryEntry(
								paths[x], _credentials.UserName, _credentials.Password,
								AuthenticationTypes.Secure);
							string name = _root.Name;
							// test the connection
							success = true;
						} catch (System.Exception e) {
							Idaho.Exception.Log(e);
							if (_root != null) { _root.Close(); }
							success = false;
							x++;
						}
					}
					Monitor.Exit(_lockOn);
				}
			}
			_connectedOn = DateTime.Now;
		}

		/// <summary>
		/// Connection needed if none exists or if last connect time was more
		/// than specified number of seconds ago
		/// </summary>
		private static bool NeedConnection() {
			return _root == null || (DateTime.Now.Subtract(_connectedOn).CompareTo(_expireAfter) > 0);
		}

		#endregion

		#region Searching

		/// <summary>
		/// Mtching directory entries
		/// </summary>
		/// <param name="filter">LDAP filter</param>
		/// <param name="properties">List of element properties to retrieve</param>
		public SearchResultCollection GetEntries(string filter, string[] properties) {
			Assert.NoNull(filter, "NullLdapFilter");
			DirectorySearcher search = new DirectorySearcher(_root, filter, properties);
			SearchResultCollection matches;
			search.ServerTimeLimit = _timeout;
			matches = search.FindAll();
			return matches;
		}
		public SearchResultCollection GetEntries(string filter) {
			Assert.NoNull(filter, "NullLdapFilter");
			return this.GetEntries(filter, null);
		}

		/// <summary>
		/// Load single active directory entry into local variable
		/// </summary>
		/// <param name="filter">LDAP filter</param>
		/// <param name="properties">List of element properties to retrieve</param>
		public bool LoadEntry(string filter, string[] properties) {
			SearchResult match = this.GetEntry(filter, properties);
			if (match != null && match.Properties.Count > 0) {
				_result = match; return true;
			}
			return false;
		}

		/// <summary>
		/// Get single active directory entry
		/// </summary>
		/// <param name="filter">LDAP filter</param>
		/// <param name="properties">List of element properties to retrieve</param>
		public SearchResult GetEntry(string filter, string[] properties) {
			SearchResultCollection matches = this.GetEntries(filter, properties);
			SearchResult match = null;
			if (matches.Count > 0) { match = matches[0]; }
			matches.Dispose();
			return match;
		}
		public SearchResult GetEntry(string filter) { return this.GetEntry(filter, null); }

		#endregion

		/// <summary>
		/// Convert GUID to string representation of byte array
		/// </summary>
		/// <remarks>
		/// Based on
		/// http://www.simple-talk.com/dotnet/.net-framework/building-active-directory-wrappers-in-.net/
		/// </remarks>
		public static string GuidToString(Guid id) {
			byte[] bytes = id.ToByteArray();
			int length = bytes.GetUpperBound(0);
			StringBuilder result = new StringBuilder((length + 1) * 2);
			
			for (int x = 0; x <= length; x++) {
				result.Append("\\" + bytes[x].ToString("x2"));
			}
			return result.ToString();
		}

		public void Finish() { if (_root != null) { _root.Dispose(); } }

		/// <summary>
		/// Is the given search result a valid user entry
		/// </summary>
		public static bool IsValidUser(SearchResult r) {
			if (r.Properties.Contains(Fields.EntityID)
				&& r.Properties.Contains(Fields.FirstName)
				&& r.Properties.Contains(Fields.LastName)
				&& r.Properties.Contains(Fields.FullPath)) {

				string path = (string)r.Properties[Fields.FullPath][0];

				if (!(path.Contains("OU=Shared") ||
					  path.Contains("OU=Resource") ||
					  path.Contains("OU=TEST") ||
					  path.Contains("System Object") ||
					  path.Contains("CN=_"))) { return true; }
			}
			return false;
		}

		/// <summary>
		/// Map directory property names to readable constants
		/// </summary>
		public class Fields {
			// common fields
			public const string EntityID = "objectGUID";
			public const string CreatedOn = "whenCreated";
			public const string LastLogon = "lastLogon";
			public const string LogonCount = "logonCount";
			public const string FullPath = "distinguishedName";
			// user fields
			public const string LastName = "sn";
			public const string FirstName = "givenName";
			public const string PhoneNumber = "telephoneNumber";
			public const string FaxNumber = "facsimileTelephoneNumber";
			public const string Email = "mail";
			public const string ZipCode = "postalCode";
			public const string State = "st";
			public const string Country = "c";
			public const string City = "l";
			public const string Title = "description";
			public const string Department = "department";
			public const string Address = "streetAddress";
			public const string Groups = "memberOf";
			public const string AccountName = "sAMAccountName";
			public const string Controls = "userAccountControl";
			public const string Organization = "physicalDeliveryOfficeName";
			public const string HomeDirectory = "homeDirectory";
			public const string AlternateEmail = "userPrincipalName";
			// group fields
			public const string GroupName = "cn";
			public const string GroupEmail = "mail";
			public const string GroupType = "groupType";
			public const string GroupMembers = "member";
			public const string GroupDescription = "description";
			// organization fields
			public const string OrganizationName = "name";
			// computer fields
			public const string NetworkName = "cn";
			public const string ComputerUser = "description";
			public const string ComputerManager = "managedBy";
			public const string DnsName = "dNSHostName";
			public const string OperatingSystem = "operatingSystem";
			public const string OperatingSystemVersion = "operatingSystemVersion";
			public const string ServicePack = "operatingSystemServicePack";
			// printer fields
			public const string PrinterName = "printerName";
			public const string PrinterType = "driverName";
			public const string PrinterLocation = "location"; // could infer org from here
			public const string AssetTag = "description";	  // inferred

			public static string[] Group = { EntityID, GroupName, CreatedOn, GroupEmail,
				GroupType, GroupMembers, GroupDescription };
			public static string[] Computer = { EntityID, NetworkName, CreatedOn,
				ComputerUser, ComputerManager, OperatingSystem, OperatingSystemVersion,
				ServicePack, LogonCount, LastLogon, DnsName, FullPath };
			public static string[] Printer = { EntityID, PrinterName, PrinterType,
				PrinterLocation, CreatedOn };
			public static string[] User = { EntityID, FirstName, LastName, CreatedOn, 
				PhoneNumber, FaxNumber, Email, AlternateEmail, ZipCode, State, Country,
				City, Department, Address, Groups, AccountName, Title, LastLogon, Controls,
				LogonCount, Organization, HomeDirectory, FullPath };
			public static string[] OrganizationalUnit = { EntityID, OrganizationName,
				FullPath, CreatedOn };
			public static string[] UserGroup = { EntityID, Groups };
		}
	}
}
