using Idaho.Data;
using System;
using System.Collections.Generic;
using System.Data;

namespace Idaho {
	public class ExceptionCollection : List<Exception> {

		public Idaho.Exception this[Int64 id] {
			get {
				if (id != 0) {
					foreach (Idaho.Exception e in this) { if (e.ID == id) { return e; } }
				}
				return null;
			}
		}
		// no public construction
		private ExceptionCollection() { }

		#region ORM

		/// <summary>
		/// Load exceptions from DB
		/// </summary>
		public static ExceptionCollection Load(ExceptionFilter filter) {
			ExceptionCollection exceptions = new ExceptionCollection();
			Idaho.Exception e;
			Sql db = new Sql();
			SqlReader reader;

			BuildCommand(db, filter);

			try {
				reader = db.GetReader();
			} catch (System.Exception ex) {
				db.Finish(true);
				Idaho.Exception.Log(ex);
				return null;
			}

			while (reader.Read()) {
				e = new Idaho.Exception();
				e.MachineName = reader.GetString("Machine");
				e.On = reader.GetDateTime("HappenedOn");
				e.Page = reader.GetString("Page");
				e.QueryString = reader.GetString("QueryString");
				e.Browser = reader.GetString("Browser");
				e.IpAddress = reader.GetIpAddress("IpAddress");
				e.Stack = reader.GetString("Stack");
				e.ID = reader.GetInt64("ID");
				e.InnerID = reader.GetInt64("InnerID");
				e.Message = reader.GetString("Message");
				e.Note = reader.GetString("Note");

				exceptions.Add(e);
			}
			reader.Close();
			db.Finish(true);

			foreach (Exception ex in exceptions) {
				// To be more thorough, could load missing inner exception
				ex.Inner = exceptions[ex.InnerID];
			}
			return exceptions;
		}

		/// <summary>
		/// Build command best suiting filter
		/// </summary>
		private static void BuildCommand(Idaho.Data.Sql db, ExceptionFilter filter) {
			if (filter.ID != 0) {
				db.ProcedureName = "ExceptionByID";
				db.Parameters.Add("id", filter.ID);
			} else {
				db.ProcedureName = "ExceptionByAny";
				if (filter.Type != Idaho.Exception.Types.Unknown) { db.Parameters.Add("type", filter.Type); }
				if (filter.After != DateTime.MinValue) { db.Parameters.Add("after", filter.After); }
				if (filter.Before != DateTime.MaxValue) { db.Parameters.Add("before", filter.Before); }
				if (filter.IpAddress != null) { db.Parameters.Add("ipAddress", filter.IpAddress.ToInt32()); }
				if (filter.User != null) { db.Parameters.Add("userID", filter.User.ID); }
				if (!string.IsNullOrEmpty(filter.Page)) { db.Parameters.Add("page", filter.Page); }
				if (!string.IsNullOrEmpty(filter.Browser)) { db.Parameters.Add("browser", filter.Browser); }
			}
		}

		#endregion

		/// <summary>
		/// Get all exceptions from today
		/// </summary>
		public static ExceptionCollection Today {
			get {
				ExceptionFilter filter = new ExceptionFilter();
				filter.After = DateTime.Now.AddDays(-1).EndOfDay();
				return ExceptionCollection.Load(filter);
			}
		}

		public void Sort(ExceptionSort.Fields field) {
			this.Sort(new ExceptionSort(field));
		}
		public void Sort(ExceptionSort.Fields field, Entity.SortDirections direction) {
			this.Sort(new ExceptionSort(field, direction));
		}
	}
}