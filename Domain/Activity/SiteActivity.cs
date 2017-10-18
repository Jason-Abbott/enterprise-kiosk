using Idaho.Data;
using Idaho.Network;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Web;

namespace Idaho {
	public class SiteActivity : ILinkable {

		private Types _type = Types.Empty;
		private Idaho.Network.IpAddress _ipAddress = null;
		private Guid _userID = Guid.Empty;
		private string _clientMachine = string.Empty;
		private string _note = string.Empty;
		private DateTime _on = DateTime.MinValue;

		public enum Types {
			Empty = 0,
			StartedApplication = 1,
			CreatedSession = 2,
			SerializedEntityConversion = 3,
			RedirectToOtherUrl = 4,
			SavedRootEntity = 5,
			ApplicationEnded = 6,
			ChangedTheme = 7,
			RunScheduledTask = 8,
			PageView = 9,
			// security
			SuccessfulLogin = 10,
			UnknownUsername = 11,
			IncorrectPassword = 17,
			TriedDisabledAccountLogin = 12,
			AutoLoginFromCookie = 13,
			Register = 15,
			ResetPassword = 16,
			ImpersonateUser = 19,
			StopImpersonation = 18,
			// commerce
			InitiateCheckout = 30,
			SendCartResponse = 31,
			// google
			GoogleNotification = 40,
			GoogleFileSave = 41

		}

		#region Properties

		/// <summary>
		/// The date is set in and then read back from the database
		/// </summary>
		public DateTime On { set { _on = value; } get { return _on; } }
		public Types Type { set { _type = value; } get { return _type; } }
		public Idaho.Network.IpAddress IpAddress { set { _ipAddress = value; } get { return _ipAddress; } }
		public User User { set { if (value != null) { _userID = value.ID; } } }
		private Guid UserID { set { _userID = value; } }
		public string Note { set { _note = value.SafeForWeb(250); } get { return _note; } }

		/// <summary>
		/// Name of client or user's computer on the network
		/// </summary>
		public string ClientMachine {
			set { _clientMachine = value; } get { return _clientMachine; }
		}

		#endregion

		internal SiteActivity() { }

		#region ORM

		/// <summary>
		/// Queue a thread for the save request
		/// </summary>
		private void QueueSave() { ThreadPool.QueueUserWorkItem(Save); }

		/// <summary>
		/// Log activity
		/// </summary>
		/// <param name="state">Optional callback state</param>
		private void Save(object state) {
			Sql sql = new Sql() { ProcedureName = "SaveSiteActivity" };
			sql.Parameters.Add("type", _type, SqlDbType.Int);
			sql.Parameters.Add("typeName", _type.ToWords(), SqlDbType.VarChar);
			sql.Parameters.Add("ipAddress", _ipAddress.ToInt32(), SqlDbType.Int);
			sql.Parameters.Add("clientMachine", _clientMachine, SqlDbType.VarChar);
			if (_userID != Guid.Empty) {
				sql.Parameters.Add("userID", _userID, SqlDbType.UniqueIdentifier);
			}
			if (!string.IsNullOrEmpty(_note)) {
				sql.Parameters.Add("note", _note, SqlDbType.VarChar);
			}

			try {
				sql.ExecuteOnly();
			} catch (System.Exception e) {
				Idaho.Exception.Log(e, Exception.Types.Database, sql.Command.ToString());
			}
		}

		#endregion

		#region Log

		public static void Log(Types type, Guid userID, string note) {
			SiteActivity a = new SiteActivity();
			HttpContext context = HttpContext.Current;

			a.Type = type;
			if (userID != Guid.Empty) { a.UserID = userID; }
			
			if (context != null && !IpAddress.Client.IsLocal) {
				a.ClientMachine = context.Request.UserHostName;
				a.IpAddress = IpAddress.Client;
			} else {
				a.ClientMachine = Environment.MachineName;
				a.IpAddress = IpAddress.Host;
			}
			if (type == Types.PageView && context != null) {
				note = context.Request.Url.LocalPath;
				note = note.Substring(note.LastIndexOf("/") + 1);
			}
			a.Note = note;
			a.QueueSave();
		}
		public static void Log(Types type, User user, string note) {
			Log(type, user.ID, note);
		}
		public static void Log(Types type, User user) {
			Log(type, user.ID, string.Empty);
		}
		public static void Log(Types type, Guid userID) {
			Log(type, userID, string.Empty);
		}
		public static void Log(Types type, Idaho.Web.Profile profile) {
			Log(type, profile, string.Empty);
		}
		public static void Log(Types type, string note) {
			Log(type, Guid.Empty, note);
		}
		public static void Log(Types type, Idaho.Web.Profile profile, string note) {
			Log(type, profile.UserID, note);
		}
		public static void Log(Types type) { Log(type, string.Empty); }

		#endregion

		#region ILinkable Members

		public string DetailUrl {
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		public string DetailLink {
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		#endregion
	}
}
