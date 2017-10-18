using Idaho.Attributes;
using Idaho.Draw;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace Idaho.Configuration {
	/// <summary>
	/// Configuration for a resources
	/// </summary>
	public class Resources : ConfigurationSection, ISelfLoad<Idaho.Resource> {

		internal Resources() { }

		public static Resources Current {
			get { return WebConfigurationManager.GetSection("resources") as Resources; }
		}

		/// <summary>
		/// Assemblies with resources to be used
		/// </summary>
		[ConfigurationProperty("assembly", IsRequired = true, IsDefaultCollection = true)]
		public AssemblyElementCollection Assemblies {
			get { return (AssemblyElementCollection)this["assembly"]; }
		}

		/// <summary>
		/// Load assembly references and add them to resource object
		/// </summary>
		public void ApplySettings() {
			foreach (AssemblyElement e in this.Assemblies) { Resource.Add(e.Assembly); }
		}
		public bool ApplySettings(string key, Resource entity) {
			throw new System.Exception("The method or operation is not implemented.");
		}
	}
}
