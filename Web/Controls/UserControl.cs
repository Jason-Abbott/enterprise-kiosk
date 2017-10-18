using Idaho.Attributes;
using System;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public abstract class UserControl : System.Web.UI.UserControl {

		private Idaho.Web.Page _hostPage = null;
		private HttpContext _context = null;
		private Profile _profile = null;

		#region Properties

		public new Idaho.Web.Page Page {
			get {
				if (_hostPage == null) { _hostPage = (Idaho.Web.Page)base.Page; }
				return _hostPage;
			}
			set { _hostPage = value; }
		}
		public string CssStyle {
			set { this.Attributes.CssStyle.Value = value; }
			get { return this.Attributes.CssStyle.Value; }
		}
		[WebBindable]
		public string CssClass {
			set { this.Attributes.Add("class", value); }
			get { return this.Attributes["class"]; }
		}
		public Profile Profile {
			get {
				if (_profile == null) {
					if (_hostPage != null) {
						_profile = _hostPage.Profile;
					} else {
						_profile = Profile.Load(_context);
					}
				}
				return _profile;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Context needs to be provided if called on a custom thread
		/// </summary>
		public UserControl(HttpContext context) { _context = context; }
		public UserControl() { _context = base.Context; }

		#endregion

		#region Events

		protected override void OnInit(EventArgs e) {
			_hostPage = (Idaho.Web.Page)base.Page;
			base.OnInit(e);
		}

		#endregion

	}
}