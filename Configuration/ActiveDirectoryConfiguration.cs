using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Net;
using System.Text;

namespace Idaho.Configuration {
	public class ActiveDirectory : ConfigurationSection, ISelfLoad<Data.ActiveDirectory> {

		internal ActiveDirectory() { }

		public static ActiveDirectory Current {
			get {
				return WebConfigurationManager.GetSection("activeDirectory") as ActiveDirectory;
			}
		}

		/// <summary>
		/// Active Directory server path
		/// </summary>
		[ConfigurationProperty("server", IsRequired = true)]
		//[UriValidator(UriKind.Absolute)]
		public string Server { get { return (string)this["server"]; } }

		[ConfigurationProperty("alternateServer", IsRequired = false)]
		//[PatternValidator("LDAP")]
		public string AlternateServer { get { return this["alternateServer"] as string; } }

		[ConfigurationProperty("userName", IsRequired = true)]
		//[RegexStringValidator(".{5,40}")]
		private string UserName { get { return this["userName"] as string; } }

		[ConfigurationProperty("password", IsRequired = true)]
		private string Password { get { return this["password"] as string; } }

		/// <summary>
		/// Username and password to connect to Active Directory server
		/// </summary>
		public NetworkCredential Credentials {
			get { return new NetworkCredential(this.UserName, this.Password); }
		}

		/// <summary>
		/// Duration to cache various Active Directory entities
		/// </summary>
		[ConfigurationProperty("cache")]
		public CacheElementCollection Cache {
			get { return this["cache"] as CacheElementCollection; }
		}

		#region ISelfLoad

		public void ApplySettings() {
			Data.ActiveDirectory.ServerPath = this.Server;
			Data.ActiveDirectory.Credentials = this.Credentials;
			Data.ActiveDirectory.ExpireAfter = this.Cache["connection"].Duration;
		}
		public bool ApplySettings(string key, Idaho.Data.ActiveDirectory ad) {
			throw new System.Exception("The method or operation is not implemented.");
		}

		#endregion

	}
}
