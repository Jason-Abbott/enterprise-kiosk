using System;
using System.Configuration;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Idaho.Web.Modules {

	/// <summary>
	/// Validate a page using basic authenitcation
	/// </summary>
	/// <remarks>
	/// This is necessary because Windows Authentication strips the 
	/// authentication header used by basic authentication. Disabling
	/// Windows Authentication disables debugging. The only way to have
	/// both is this custom module.
	/// </remarks>
	public class BasicAuthentication : IHttpModule {

		#region Static Values

		private static string _page = string.Empty;
		private static bool _authenticate = false;
		private static Regex _re = null;
		private static string _userName = string.Empty;
		private static string _password = string.Empty;

		public static string UserName { set { _userName = value; } }
		public static string Password { set { _password = value; } }

		/// <summary>
		/// Name of the page to be authenticated, if any
		/// </summary>
		public static string Page {
			set {
				_page = value;
				_authenticate = !string.IsNullOrEmpty(_page);
			}
		}

		#endregion

		/// <summary>
		/// Called by IIS
		/// </summary>
		public void Init(HttpApplication Application) {
			Application.AuthenticateRequest +=
				new EventHandler(this.OnAuthenticateRequest);
			Application.EndRequest += new EventHandler(this.OnEndRequest);
		}

		public void OnAuthenticateRequest(object source, EventArgs eventArgs) {
			HttpApplication app = (HttpApplication)source;
			if (_authenticate && app.Request.FilePath.EndsWith(_page)) {
				string header = app.Request.Headers["Authorization"];
				if (!string.IsNullOrEmpty(header) && header.StartsWith("Basic ")) {
					string encoded = header.Substring(6);
					byte[] decoded = Convert.FromBase64String(encoded);
					string[] pair = new ASCIIEncoding().GetString(decoded).Split(':');
					string userName = pair[0];
					string password = pair[1];

					if (userName == _userName && password == _password) {
						app.Context.User = new GenericPrincipal(
							new GenericIdentity(userName, "Google.Checkout.Basic"),
								new string[1] { "User" });
						return;
					}
				}
				// if we made it here 
				app.Response.StatusCode = 401;
				app.Response.StatusDescription = "Access Denied";
				app.Response.Write("401 Access Denied");
				app.CompleteRequest();
			}
		}

		/// <summary>
		/// Called by the system when the HTTP request ends.
		/// </summary>
		/// <param name="source">The calling HttpApplication.</param>
		/// <param name="eventArgs">
		/// The <see cref="System.EventArgs"/> instance 
		/// containing the event data.
		/// </param>
		public void OnEndRequest(object source, EventArgs eventArgs) {
			HttpApplication app = (HttpApplication)source;
			if (app.Response.StatusCode == 401) {
				app.Response.AppendHeader(
				  "WWW-Authenticate", "Basic Realm=\"CheckoutCallbackRealm\"");
			}
		}

		#region IHttpModule

		public void Dispose() { _re = null; }

		#endregion

	}
}
