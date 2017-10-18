using System;
using System.Configuration;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Idaho.Web.Modules {
	/// <summary>
	/// Determine if user is authorized to view a given page
	/// </summary>
	public class Authorization : IHttpModule {

		private static IAuthorize _authorizer = null;

		/// <summary>
		/// The object used to authorize application and page access
		/// </summary>
		public static IAuthorize Authorizer { set { _authorizer = value; } }

		/// <summary>
		/// Called by IIS
		/// </summary>
		public void Init(HttpApplication Application) {
			if (_authorizer == null) {
				throw new NullReferenceException("No security authorizor specified for the Authorization module");
			}
			Application.AuthorizeRequest += new EventHandler(this.OnAuthorizeRequest);
		}

		public void OnAuthorizeRequest(object source, EventArgs eventArgs) {
			HttpApplication app = (HttpApplication)source;
			string page = app.Request.Path;
			page = page.Substring(page.LastIndexOf('/') + 1);

			if (!_authorizer.IsAllowed(app.Context.User.Identity, page)) {
				app.Response.StatusCode = 401;
				app.Response.StatusDescription = "Access Denied";
				app.Response.Write("You are not authorized to view this page");
				app.CompleteRequest();
			}
		}

		public void Dispose() { }
	}
}
