using Idaho.Attributes;
using System;
using System.Configuration;
using System.Text;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class ExceptionGrid : Idaho.Web.Controls.Grid<Exception> {

		private ExceptionCollection _exceptions;
		private ExceptionFilter _filter = new ExceptionFilter();
		private Type _type = typeof(SiteActivity);

		#region Properties

		public ExceptionCollection Exceptions { set { _exceptions = value; } }

		[WebBindable]
		public Exception.Types Type {
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

		public ExceptionGrid() {
			GridColumn id = this.Columns.Add("ID");
			GridColumn ip = this.Columns.Add("IpAddress", "Address");
			GridColumn page = this.Columns.Add("Page");
			GridColumn message = this.Columns.Add("Message");
			GridColumn on = this.Columns.Add("On", "Time");

			id.IsDetailLink = true;
			page.TipTextField = "QueryString";
			message.MaxLength = 20;
			message.Style = "font-size: 12px;";
			on.Offset = this.Profile.TimeOffset;
			
			this.Columns.GroupBy = "On";
		}

		#region Events

		protected override void OnLoad(EventArgs e) {
			if (this.Ajax.IsRenderingContent) {
				if (_exceptions == null) {
					_exceptions = ExceptionCollection.Load(_filter);
				}
				if (_exceptions == null || _exceptions.Count == 0) {
					this.Visible = false;
				} else {
					_exceptions.Sort(ExceptionSort.Fields.Date, Entity.SortDirections.Descending);
					base.Data = _exceptions;
				}
			}
			base.OnLoad(e);
		}

		#endregion

	}
}