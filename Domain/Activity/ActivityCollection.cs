using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AMP {
	public class ActivityCollection : List<Activity> {

		// no public construction
		private ActivityCollection() { }

		#region ORM

		/// <summary>
		/// Load activities from DB
		/// </summary>
		public static ActivityCollection Load(ActivityFilter filter) {
			ActivityCollection activities = new ActivityCollection();
			Activity a;
			Data.Sql db = new Data.Sql();
			Data.Reader reader;

			BuildCommand(db, filter);

			try {
				reader = db.GetReader();
			} catch (System.Exception ex) {
				AMP.Exception.Log(ex);
				return null;
			}

			while (reader.Read()) {
				a = new Activity();
				a.Type = (Activity.Types)reader.GetInt32("Type");
				a.IpAddress = reader.GetIpAddress("IpAddress");
				a.Note = reader.GetString("Note");
				a.On = reader.GetTau("HappenedOn");
				activities.Add(a);
			}
			reader.Close();
			db.Finish(true);

			return activities;
		}

		/// <summary>
		/// Build command best suiting filter
		/// </summary>
		private static void BuildCommand(AMP.Data.Sql db, ActivityFilter filter) {
			db.ProcedureName = "ActivityByAny";
			if (filter.Type != Activity.Types.Empty) { db.Parameters.Add("type", filter.Type); }
			if (filter.After != Tau.MinValue) { db.Parameters.Add("after", filter.After.ToDateTime()); }
			if (filter.Before != Tau.MaxValue) { db.Parameters.Add("before", filter.Before.ToDateTime()); }
			if (filter.IpAddress != null) { db.Parameters.Add("ipAddress", filter.IpAddress.ToInt32()); }
			if (filter.User != null) { db.Parameters.Add("userID", filter.User.ID); }
			if (filter.Entity != null) { db.Parameters.Add("entityID", filter.Entity.ID); }
		}

		#endregion

		public void Sort(ActivitySort.Fields field) {
			this.Sort(new ActivitySort(field));
		}
		public void Sort(ActivitySort.Fields field, Entity.SortDirections direction) {
			this.Sort(new ActivitySort(field, direction));
		}

	}
}
