using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Idaho {
	public class SiteActivityCollection : List<SiteActivity> {

		// no public construction
		private SiteActivityCollection() { }

		#region ORM

		/// <summary>
		/// Load activities from DB
		/// </summary>
		public static SiteActivityCollection Load(SiteActivityFilter filter) {
			SiteActivityCollection activities = new SiteActivityCollection();
			SiteActivity a;
			Data.Sql db = new Data.Sql();
			Data.SqlReader reader;

			BuildCommand(db, filter);

			try {
				reader = db.GetReader();
			} catch (System.Exception ex) {
				Idaho.Exception.Log(ex);
				return null;
			}

			while (reader.Read()) {
				a = new SiteActivity();
				a.Type = (SiteActivity.Types)reader.GetInt32("Type");
				a.IpAddress = reader.GetIpAddress("IpAddress");
				a.Note = reader.GetString("Note");
				a.On = reader.GetDateTime("HappenedOn");
				activities.Add(a);
			}
			reader.Close();
			db.Finish(true);

			return activities;
		}

		/// <summary>
		/// Build command best suiting filter
		/// </summary>
		private static void BuildCommand(Idaho.Data.Sql db, SiteActivityFilter filter) {
			db.ProcedureName = "ActivityByAny";
			if (filter.Type != SiteActivity.Types.Empty) { db.Parameters.Add("type", filter.Type); }
			if (filter.After != DateTime.MinValue) { db.Parameters.Add("after", filter.After); }
			if (filter.Before != DateTime.MaxValue) { db.Parameters.Add("before", filter.Before); }
			if (filter.IpAddress != null) { db.Parameters.Add("ipAddress", filter.IpAddress.ToInt32()); }
			if (filter.User != null) { db.Parameters.Add("userID", filter.User.ID); }
			if (filter.Entity != null) { db.Parameters.Add("entityID", filter.Entity.ID); }
		}

		#endregion

		public void Sort(SiteActivitySort.Fields field) {
			this.Sort(new SiteActivitySort(field));
		}
		public void Sort(SiteActivitySort.Fields field, Entity.SortDirections direction) {
			this.Sort(new SiteActivitySort(field, direction));
		}

	}
}
