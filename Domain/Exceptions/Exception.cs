using Idaho.Data;
using Idaho.Network;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace Idaho {
	public class Exception : ILinkable, IEquatable<Exception> {

		private Int64 _id;
		private Types _type = Types.Unknown;
		private DateTime _on = DateTime.MinValue;
		private bool _redirect = false;
		private string _note = string.Empty;
		private User _user = null;
		private string _stack = string.Empty;
		private string _message = string.Empty;
		private string _source = string.Empty;
		private string _process = string.Empty;
		private string _machineName = string.Empty;
		private Idaho.Network.IpAddress _userIP = null;
		private string _browser = string.Empty;
		private string _cookies = string.Empty;
		private string _pageName = string.Empty;
		private Int64 _innerID;
		private string _queryString = string.Empty;
		private Int16 _recurseCount = -1;
		private bool _sendEmail = false;
		private Exception _inner = null;
		// track how many times an error has occurred
		private static Dictionary<string, Stack<DateTime>> _occurrences
				 = new Dictionary<string, Stack<DateTime>>();

		[Flags()]
		public enum Types {
			SendEmail = HostEmail | Serialization | Scheduler | Database |
				Fulfillment | Billing | Google | ActiveDirectory,
			HostEmail = FileSystem,
			Unknown = 0x0,
			FileSystem = 0x1,
			Syntax = 0x2,
			Serialization = 0x4,
			Custom = 0x8,
			Url = 0x10,
			MailServer = 0x20,
			Scheduler = 0x40,
			Database = 0x80,
			Fulfillment = 0x100,
			Billing = 0x200,
			InvalidArgument = 0x400,
			Google = 0x800,
			ActiveDirectory = 0x1000
		}

		#region Static Fields

		private static MailAddress _emailTo = null;
		private static TimeSpan _queueTime = TimeSpan.FromMinutes(2);
		private static string _defaultCustomerName = string.Empty;
		private static MailAddress _hostMailTo = null;
		private static string _mailTemplateName = string.Empty;

		/// <summary>
		/// Template for sending error message
		/// </summary>
		internal static string MailTemplate {
			get { return File.Content(_mailTemplateName); }
		}
		public static string MailTemplateName { set { _mailTemplateName = value; } }

		/// <summary>
		/// Address to send errors to
		/// </summary>
		public static MailAddress MailTo { set { _emailTo = value; } get { return _emailTo; } }
		
		/// <summary>
		/// How often should the same error generate e-mail
		/// </summary>
		public static TimeSpan QueueTime { set { _queueTime = value; } }
		public static string DefaultCustomerName {
			set { _defaultCustomerName = value; }
			internal get { return _defaultCustomerName; }
		}

		/// <summary>
		/// Who should receive messages about errors needing the web host's attention
		/// </summary>
		public static MailAddress HostMailTo { set { _hostMailTo = value; } }

		#endregion

		#region Properties

		public Exception Inner { get { return _inner; } internal set { _inner = value; } }

		/// <summary>
		/// Name of the process active when exception occurred
		/// </summary>
		public string Process { get { return _process; } set { _process = value; } }

		/// <summary>
		/// Custom note added at exception handling
		/// </summary>
		public string Note { get { return _note; } set { _note = value; } }

		/// <summary>
		/// Message from the System.Exception
		/// </summary>
		public string Message { get { return _message; } set { _message = value; } }
		public Types Type { get { return _type; } set { _type = value; } }

		/// <summary>
		/// Stack trace from the System.Exception
		/// </summary>
		public string Stack { get { return _stack; } set { _stack = value; } }

		/// <summary>
		/// Source from the System.Exception
		/// </summary>
		public string Source { get { return _source; } set { _source = value; } }

		public Int64 ID { get { return _id; } set { _id = value; } }
		public Int64 InnerID { get { return _innerID; } set { _innerID = value; } }
		public User User { get { return _user; } set { _user = value; } }

		/// <summary>
		/// When the error occurred
		/// </summary>
		public DateTime On { get { return _on; } set { _on = value; } }

		/// <summary>
		/// Page on which the error occurred, if any (could be server only process)
		/// </summary>
		public string Page { get { return _pageName; } set { _pageName = value; } }

		public string Cookies { get { return _cookies; } set { _cookies = value; } }
		public string Browser { get { return _browser; } set { _browser = value; } }
		public string QueryString { get { return _queryString; } set { _queryString = value; } }
		public Network.IpAddress IpAddress { get { return _userIP; } set { _userIP = value; } }
		public string MachineName { get { return _machineName; } set { _machineName = value; } }

		#endregion

		#region Constructor

		internal Exception() { }

		/// <summary>
		/// Load values used to supplement exception information
		/// </summary>
		/// <param name="redirect">Should user be redirected to an error page</param>
		private Exception(System.Exception ex, bool redirect, string note, Types type) {
			_message = ex.Message;
			_stack = ex.StackTrace;
			_source = ex.Source;
			_type = type;
			_redirect = redirect;
			_note = note;
			_process = AppDomain.CurrentDomain.FriendlyName;
			_machineName = Environment.MachineName;
			_sendEmail = (_emailTo != null);

			if (ex.InnerException != null) {
				_inner = new Exception(ex.InnerException, redirect, note, type);
			}

			if (HttpContext.Current != null) {
				Web.Profile profile = Web.Profile.Load(HttpContext.Current);
				if (profile != null) { _user = profile.User; }
				_browser = HttpContext.Current.Request.UserAgent;
				_cookies = this.CookieString();
				_userIP = Idaho.Network.IpAddress.Client;
				_pageName = HttpContext.Current.Request.Url.LocalPath;
				_pageName = _pageName.Substring(_pageName.LastIndexOf("/") + 1);
				_queryString = HttpUtility.UrlDecode(HttpContext.Current.Request.Url.Query).TrimStart('?');
			} else {
				_browser = "[Server]";
				_userIP = Idaho.Network.IpAddress.Host;
				_pageName = _source;
			}
		}

		#endregion

		#region Factory

		/// <summary>
		/// Load single exception from DB
		/// </summary>
		public static Idaho.Exception Load(Int64 id) {
			Idaho.ExceptionFilter filter = new Idaho.ExceptionFilter();
			filter.ID = id;
			Idaho.ExceptionCollection exceptions = Idaho.ExceptionCollection.Load(filter);
			if (exceptions.Count > 0) { return exceptions[id]; }
			return null;
		}

		//private Int64 Save() { return this.Save(this); }

		/// <summary>
		/// Persist exception data
		/// </summary>
		/// <returns>Log number</returns>
		private Int64 Save() {
			_recurseCount += 1;

			// recursively save inner exceptions
			if (_inner != null) {
				_innerID = _inner.Save();
				_recurseCount -= 1;
			}
			Data.Sql sql = new Data.Sql();
			sql.ProcedureName = "SaveException";
			sql.Parameters.IgnoreNull = true;

			sql.Parameters.Add("type", (int)_type, SqlDbType.Int);
			sql.Parameters.Add("message", _message, SqlDbType.VarChar);
			sql.Parameters.Add("stack", _stack, SqlDbType.VarChar);
			sql.Parameters.Add("machine", _machineName, SqlDbType.VarChar);
			sql.Parameters.Add("process", _process, SqlDbType.VarChar);

			if (_recurseCount == 0 && !string.IsNullOrEmpty(_note)) {
				// only save note for outermost exception
				sql.Parameters.Add("note", _note.SafeForWeb(500), SqlDbType.VarChar);
			}
			// client
			if (_user != null) {
				sql.Parameters.Add("userID", _user.ID, SqlDbType.UniqueIdentifier);
			}
			if (_userIP != null) {
				sql.Parameters.Add("ipAddress", _userIP.ToInt32(), SqlDbType.Int);
			}
			sql.Parameters.Add("browser", _browser, SqlDbType.VarChar);
			sql.Parameters.Add("cookies", _cookies, SqlDbType.VarChar);
			// meta
			sql.Parameters.Add("innerID", _innerID, SqlDbType.BigInt);
			//sql.Parameters.Add("emailed", ex);
			// page
			sql.Parameters.Add("context", _pageName, SqlDbType.VarChar);
			sql.Parameters.Add("queryString", _queryString, SqlDbType.VarChar);

			try { _id = sql.GetReturn();	}
			catch { _id = 0; }
			finally { sql.Finish(true); }

			return _id;
		}

		#endregion

		#region Log

		/// <summary>
		/// Log an error and return the error number
		/// </summary>
		/// <param name="redirect">Redirect after logging?</param>
		/// <returns>Error number</returns>
		public static Int64 Log(System.Exception ex, Types type, bool redirect, string note) {
			Idaho.Exception exception = new Idaho.Exception(ex, redirect, note, type);
			Int64 id = exception.Save();

			if (type.Contains(Types.SendEmail)) {
				Assert.NoNull(_emailTo, "NullMailTo");
				string key = ex.Message;
				if (!_occurrences.ContainsKey(key)) {
					_occurrences.Add(key, new Stack<DateTime>());
				}
				Stack<DateTime> oc = _occurrences[key];

				if (oc.Count == 0 || TimeSpan.Compare(
					DateTime.Now.Subtract(oc.Peek()), _queueTime) >= 0) {
					// at least frequency number of minutes have passed since last mail

					Idaho.Web.Email mail = new Idaho.Web.Email();
					List<MailAddress> addresses = new List<MailAddress>();

					addresses.Add(_emailTo);

					if (type.Contains(Types.HostEmail) && _hostMailTo != null
						&& !addresses.Contains(_hostMailTo)) { addresses.Add(_hostMailTo); }

					mail.Error(exception, addresses, oc.Count);

					// reset the stack
					if (oc.Count > 0) { oc.Clear(); }
				}
				oc.Push(DateTime.Now);
			}
			return id;
		}
		public static Int64 Log(System.Exception ex, Types type) {
			return Idaho.Exception.Log(ex, type, false, null);
		}
		public static Int64 Log(System.Exception ex) {
			return Idaho.Exception.Log(ex, Types.Unknown, false, null);
		}
		public static Int64 Log(System.Exception ex, Types type, bool redirect) {
			return Idaho.Exception.Log(ex, type, redirect, null);
		}
		public static Int64 Log(System.Exception ex, Types type, string note) {
			return Idaho.Exception.Log(ex, type, false, note);
		}
		public static Int64 Log(string message, Types type) {
			return Idaho.Exception.Log(new System.Exception(message), type);
		}
		public static Int64 Log(string message) {
			return Idaho.Exception.Log(message, Types.Custom);
		}

		#endregion

		/// <summary>
		/// Create string of cookies for logging
		/// </summary>
		private string CookieString() {
			StringBuilder cookies = new StringBuilder();
			foreach (string key in HttpContext.Current.Request.Cookies) {
				cookies.AppendFormat("{0}={1};", key,
					HttpContext.Current.Request.Cookies[key].Value);
			}
			return cookies.ToString().TrimEnd(';');
		}

		public bool Equals(Idaho.Exception other) { return _id == other.ID; }

		#region ILinkable

		public string DetailUrl {
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		public string DetailLink {
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		#endregion

	}
}