using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Idaho.Web {
	/// <summary>
	/// Fields to be stateful during user session
	/// </summary>
	/// <remarks>
	/// This entire object is serialized into session state.
	/// </remarks>
	[Serializable]
	public class Profile {

		private User _user;
		private Guid _userID = Guid.Empty;
		private bool _allowCredentialsCookie = true;
		private bool _authenticated = false;
		private bool _handlesTransparency = true;
		private DateTime _lastLogin = DateTime.MinValue;
		private TimeSpan _timeOffset = TimeSpan.Zero;
		private Idaho.Network.IpAddress _ipAddress;
		private string _message = string.Empty;
		private string _destinationPage = string.Empty;
		private Dictionary<string, NameValue<string, Entity.SortDirections>> _gridSort;
		[NonSerialized()] private HttpContext _context;
		[NonSerialized()] const string _key = "profile";
		[NonSerialized()] const string _userIdKey = "UserID";
		[NonSerialized()] const string _offsetKey = "TimeOffset";
		[NonSerialized()] const string _sortKey = "GridSort";

		#region Properties

		/// <summary>
		/// Keep track of sorting preference by column name and grid control ID
		/// </summary>
		public Dictionary<string, NameValue<string, Entity.SortDirections>> GridSort {
			get {
				if (_gridSort == null) { _gridSort = new Dictionary<string, NameValue<string, Entity.SortDirections>>(); }
				return _gridSort;
			}
			set {
				// id=col=direction|id2=col2=direction2
				_gridSort = value;

				// write cookie
				StringBuilder list = new StringBuilder();
				foreach (string key in _gridSort.Keys) {
					list.Append(key);
					list.Append("=");
					list.Append(_gridSort[key].Name);
					list.Append("=");
					list.Append(_gridSort[key].Value.ToString());
					list.Append("|");
				}
				this.SetCookie(_sortKey, list.ToString().TrimEnd(new char[] { '|' }));
			}
		}

		/// <summary>
		/// Does the current user's browser natively render PNG transparency
		/// </summary>
		public bool HandlesTransparency { get { return _handlesTransparency; } }
		public DateTime LastLogin { get { return _lastLogin; } set { _lastLogin = value; } }
		public bool Authenticated { get { return _authenticated; } set { _authenticated = value; } }
		//protected HttpCookieCollection Cookies { get { return this.Context.Request.Cookies; } }

		/// <summary>
		/// Write a cookie to persist user ID
		/// </summary>
		/// <remarks>
		/// Based on a login form checkbox. Should be left false for public computers.
		/// </remarks>
		public bool AllowCredentialsCookie {
			get { return _allowCredentialsCookie; } set { _allowCredentialsCookie = value; } }

		/// <summary>
		/// Setting the client time updates the time offset only
		/// </summary>
		public DateTime ClientTime {
			set { this.TimeOffset = DateTime.Now - value; }
			get { return DateTime.Now + this.TimeOffset; }
		}

		/// <summary>
		/// User ID
		/// </summary>
		/// <remarks>
		/// Tracked separately from user object so it can be stored in a cookie.
		/// </remarks>
		public Guid UserID {
			get { return _userID; }
			set {
				_userID = value;
				if (_allowCredentialsCookie) { this.SetCookie(_userIdKey, _userID); }
			}
		}
		
		protected HttpContext Context {
			get {
				if (_context == null) { _context = System.Web.HttpContext.Current; }
				return _context;
			}
			set { _context = value; }
		}

		protected static string Key { get { return _key; } }

		public Idaho.Network.IpAddress IPAddress {
			get {
				if (_ipAddress == null) {
					_ipAddress = Idaho.Network.IpAddress.Parse(this.Context.Request.UserHostAddress);
				}
				return _ipAddress;
			}
		}

		/// <summary>
		/// Message displayed to the user
		/// </summary>
		/// <remarks>
		/// The base page always displays a message if it's non-null, so after
		///	returning a new message, always nullify so it doesn't get repeated.
		/// </remarks>
		public string Message {
			get {
				string messageCopy = _message;
				if (!string.IsNullOrEmpty(_message)) { _message = string.Empty; }
				return messageCopy;
			}
			set {
				_message = value;
				// store in cookie to be used for output cache variance
				//this.SetCookie("message", HttpUtility.HtmlEncode(value), false);
			}
		}

		/// <summary>
		/// Time difference between client and server
		/// </summary>
		/// <remarks>
		/// Used to display times relative to client locale.
		/// </remarks>
		public TimeSpan TimeOffset {
			get { return _timeOffset; }
			set { _timeOffset = value; this.SetCookie(_offsetKey, value); }
		}

		public User User {
			get { return _user; }
			set {
				_user = value;
				if (_user != null) { _authenticated = true; this.UserID = _user.ID; }
			}
		}

		/// <summary>
		/// Target page name
		/// </summary>
		/// <remarks>
		/// If a login is required before navigation to requested page then store that
		/// page in profile so redirection can happen after authentication.
		/// </remarks>
		public string DestinationPage { 
			get { return _destinationPage; } set { _destinationPage = value; }
		}

		#endregion

		#region Constructors

		protected Profile(HttpContext context) {
			this.LoadCookieValues();
			_handlesTransparency = Utility.HandlesTransparency(context.Request.Browser);
		}

		#endregion

		#region Factory

		/// <summary>
		/// Load profile from session
		/// </summary>
		public static Profile Load(HttpContext context) {
			if (context != null && context.Session != null) {
				Profile profile;
				if (context.Session[Profile.Key] != null) {
					profile = (Profile)context.Session[Profile.Key];
				} else {
					profile = new Profile(context);
					profile.Save(context);
				}
				profile.Context = context;
				return profile;
			} else {
				return null;
			}
		}
		protected void Save(HttpContext context) { context.Session[_key] = this; }

		/// <summary>
		/// Load values stored in cookies
		/// </summary>
		protected virtual void LoadCookieValues() {
			HttpCookie user = this.Context.Request.Cookies[_userIdKey];
			HttpCookie offset = this.Context.Request.Cookies[_offsetKey];
			HttpCookie sorting = this.Context.Request.Cookies[_sortKey];
			
			if (user != null) { _userID = new Guid(user.Value); }
			_timeOffset = (offset == null) ? TimeSpan.Zero : TimeSpan.Parse(offset.Value);

			if (sorting != null) {
				// id=col=direction|id2=col2=direction2
				NameValue<string, Entity.SortDirections> columnSort;
				string[] parts;
				string id;
				string[] grids = sorting.Value.Split(new char[] { '|' });

				foreach (string g in grids) {
					parts = g.Split(new char[] { '=' });
					id = parts[0];

					columnSort = new NameValue<string, Entity.SortDirections>();
					columnSort.Name = parts[1];
					columnSort.Value = parts[2].ToEnum<Entity.SortDirections>();

					this.GridSort.Add(id, columnSort);
				}
			}
		}

		#endregion

		public virtual void Clear() {
			_user = null;
			this.UserID = Guid.Empty;	// this will also nullify cookie
			_authenticated = false;
			_allowCredentialsCookie = false;
			_timeOffset = TimeSpan.Zero;
			_ipAddress = null;
			_message = string.Empty;
			_destinationPage = string.Empty;
		}

		/// <summary>
		/// Localize to client time based on captured offset
		/// </summary>
		public DateTime LocalizeTime(DateTime date) { return (date + this.TimeOffset); }

		/// <summary>
		/// Write a cookie
		/// </summary>
		/// <param name="permanent">Should the cookie be permanent</param>
		protected void SetCookie(string name, string value, bool permanent) {
			HttpCookie cookie = new HttpCookie(name, value);
			cookie.Expires = permanent ? DateTime.Now.AddYears(5) : DateTime.Now.AddHours(1);
			this.Context.Response.Cookies.Add(cookie);
		}
		protected void SetCookie(string name, object value, bool permanent) {
			this.SetCookie(name, value.ToString(), permanent);
		}
		protected void SetCookie(string name, string value) {
			this.SetCookie(name, value, true);
		}
		protected void SetCookie(string name, object value) {
			this.SetCookie(name, value, true);
		}

		/// <summary>
		/// Write cookies used to vary output caching
		/// </summary>
		public virtual void WriteCacheCookies(User user) {
			//Me.SetCookie("section", user.Section.ToString, False)
			//Me.SetCookie("role", user.Roles.ToString, False)
		}
	}
}