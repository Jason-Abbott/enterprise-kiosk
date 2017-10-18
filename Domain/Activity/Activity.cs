using AMP.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;

namespace AMP {
	public class Activity {

		private Types _type;
		private AMP.Network.IpAddress _ipAddress;
		private Guid _userID = Guid.Empty;
		private Guid _entityID = Guid.Empty;
		private string _note = string.Empty;
		private Tau _on;

		public enum Types {
			Empty = 0,
			StartedApplication = 1,
			CreatedSession = 2,
			SerializedEntityConversion = 3,
			RedirectToOtherUrl = 4,
			SavedRootEntity = 5,
			ApplicationEnded = 6,
			// security
			SuccessfulLogin = 10,
			UnknownUsername = 11,
			IncorrectPassword = 17,
			TriedDisabledAccountLogin = 12,
			AutoLoginFromCookie = 13,
			Register = 15,
			ResetPassword = 16,
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
		public Tau On { set { _on = value; } get { return _on; } }
		public Types Type { set { _type = value; } get { return _type; } }
		public AMP.Network.IpAddress IpAddress { set { _ipAddress = value; } get { return _ipAddress; } }
		public User User { set { if (value != null) { _userID = value.ID; } } }
		private Guid UserID { set { _userID = value; } }
		public Entity Entity { set { if (value != null) { _entityID = value.ID; } } }
		public string Note { set { _note = Format.SafeString(value, 250); } get { return _note; } }

		#endregion

		internal Activity() { }

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
			return;
			int ip = (_ipAddress == null) ? 0 : _ipAddress.ToInt32();
			object[] values = new object[] { (int)_type, ip, _userID, _entityID, _note };
			Data.Sql db = new Data.Sql("ActivityInsert", values);

			try {
				db.ExecuteOnly();
			} catch (System.Exception e) {
				AMP.Exception.Log(e, Exception.Types.Database, db.Command.ToString());
			}
		}

		#endregion

		#region Log

		public static void Log(Types type, Idaho.Web.Profile profile, Entity e, string note) {
			Activity a = new Activity();
			a.Type = type;
			a.Entity = e;
			a.Note = note;
			if (profile != null) { a.User = profile.User; }
			a.IpAddress = IpAddress.Client ?? IpAddress.Host;
			a.QueueSave();
		}
		public static void Log(Types type, Guid userID, string note) {
			Activity a = new Activity();
			a.Type = type;
			a.UserID = userID;
			a.Note = note;
			a.IpAddress = IpAddress.Client ?? IpAddress.Host;
			a.QueueSave();
		}
		public static void Log(Types type, User user, string note) {
			Log(type, user.ID, note);
		}
		public static void Log(Types type, Guid userID) {
			Log(type, userID, string.Empty);
		}
		public static void Log(Types type, Idaho.Web.Profile profile) {
			Log(type, profile, null, string.Empty);
		}
		public static void Log(Types type, string note) {
			Log(type, null, null, note);
		}
		public static void Log(Types type, Idaho.Web.Profile profile, string note) {
			Log(type, profile, null, note);
		}
		public static void Log(Types type) { Log(type, null, null, string.Empty); }

		#endregion

	}
}
