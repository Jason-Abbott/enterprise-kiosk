using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Configuration {
	/// <summary>
	/// Element indicating amount of time to cache an entity
	/// </summary>
	public class CacheElement : ConfigurationElement {

		/// <summary>
		/// Name of cache item
		/// </summary>
		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\")]
		public string Name {
			get { return this["name"] as string; }
			set { this["name"] = value; }
		}

		/// <summary>
		/// Duration for which to cache the entity
		/// </summary>
		[ConfigurationProperty("duration", IsRequired = true)]
		[TimeSpanValidator(MinValueString = "0:0:0", MaxValueString = "23:59:59")]
		public TimeSpan Duration {
			get { return (TimeSpan)this["duration"]; }
			set { this["duration"] = value.ToString(); }
		}
	}
}
