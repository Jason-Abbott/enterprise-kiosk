using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Idaho.Network {
	public class Web {

		#region Static

		private static string _defaultDomain = string.Empty;

		/// <summary>
		/// Default domain for e-mail, etc.
		/// </summary>
		public static string DefaultDomain {
			set { _defaultDomain = value; }
			get { return _defaultDomain; }
		}

		#endregion

		/// <summary>
		/// Does the given URL exist
		/// </summary>
		public static bool Exists(string url) {
			WebRequest request;
			HttpWebResponse response;
			try {
				request = WebRequest.Create(url);
				//request.Credentials = CredentialCache.DefaultNetworkCredentials;
				request.Credentials = Data.ActiveDirectory.Credentials;
				response = (HttpWebResponse)request.GetResponse();
				if (response.StatusCode == HttpStatusCode.OK) { return true; }
			} catch {
				return false;
			}
			return false;
		}
	}
}
