using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class ActivityGrid : Idaho.Web.Controls.Grid<SiteActivity> {

		private SiteActivityCollection _activities;
		private SiteActivityFilter _filter = new SiteActivityFilter();
		private Type _type = typeof(SiteActivity);

		#region Properties

		public SiteActivityCollection Activities { set { _activities = value; } }

		[WebBindable]
		public SiteActivity.Types Type {
			set { _filter.Type = value; }
			get { return _filter.Type; }
		}

		[WebBindable]
		public DateTime Before {
			set { _filter.Before = value.EndOfDay(); }
			get { return _filter.Before; }
		}

		[WebBindable]
		public DateTime After {
			set { _filter.After = value.StartOfDay(); }
			get { return _filter.After; }
		}
		
		[WebBindable]
		public User User {
			set { _filter.User = value; }
			get { return _filter.User; }
		}

		[WebBindable]
		public Network.IpAddress IpAddress {
			set { _filter.IpAddress = value; }
			get { return _filter.IpAddress; }
		}

		#endregion

		public ActivityGrid() {
			this.Columns.Add("IpAddress", "Address");
			GridColumn type = this.Columns.Add("Type");
			GridColumn on = this.Columns.Add("On", "Time");
			this.Columns.GroupBy = "On";

			type.Style = "text-align: center;";
			on.Offset = this.Profile.TimeOffset;
		}

		#region Events

		protected override void OnLoad(EventArgs e) {
			if (this.Ajax.IsRenderingContent) {
				if (_activities == null) {
					_activities = SiteActivityCollection.Load(_filter);
				}
				if (_activities == null || _activities.Count == 0) {
					this.Visible = false;
				} else {
					_activities.Sort(SiteActivitySort.Fields.Date, Entity.SortDirections.Descending);
					base.Data = _activities;
				}
			}
			base.OnLoad(e);
		}

		#endregion

	}
}
