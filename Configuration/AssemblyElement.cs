using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace Idaho.Configuration {
	public class AssemblyElement : ConfigurationElement {

		[ConfigurationProperty("sampleType", IsRequired = true)]
		private String TypeName {
			get { return this["sampleType"] as string; }
			set { this["sampleType"] = value; }
		}

		[ConfigurationProperty("name", IsRequired = false)]
		internal string AssemblyName {
			get { return this["name"] as string; }
			set { this["name"] = value; }
		}

		/// <summary>
		/// Reference to assembly
		/// </summary>
		public Assembly Assembly {
			get {
				string fullType = this.TypeName + "," + this.AssemblyName;
				return Assembly.GetAssembly(Type.GetType(fullType));
			}
		}
	}
}
