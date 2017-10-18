using Idaho.Web.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace Idaho.Web.Ajax {
	public abstract class Request {

		private Ajax.Response _response;
		private string _key = string.Empty;
		private Profile _profile;

		#region Properties

		internal protected Profile Profile {
			get {
				if (_profile == null) { _profile = Profile.Load(_response.Context); }
				return _profile;
			}
		}

		protected string this[string key] {
			get { return _response.Context.Request.QueryString[key]; }
		}
		internal protected Response Response { get { return _response; } }
		protected HttpContext Context { get { return _response.Context; } }
		protected NameValueCollection Parameters { get { return _response.Parameters; } }

		/// <summary>
		/// Build cache key for those controls that allow cached output
		/// </summary>
		protected string Key {
			get {
				if (_key == string.Empty) {
					_key = _response.Parameters.ToString()
						+ _response.Context.Request.QueryString.ToString()
						+ ((this.Profile.HandlesTransparency) ? "png" : "nopng");
				}
				return _key;
			}
			set { _key = value; }
		}

		#endregion

		#region Constructors

		protected Request(Ajax.Response r) { _response = r; }

		#endregion

		/// <summary>
		/// Determine if type is valid for AJAX calls
		/// </summary>
		protected bool ValidType(Type t) {
			return typeof(IAjaxControl).IsAssignableFrom(t);
		}

		/// <summary>
		/// Process the request, generating a response
		/// </summary>
		internal abstract void Process();
	}
}