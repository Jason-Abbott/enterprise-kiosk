using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Configuration {
	[ConfigurationCollection(typeof(CacheElement), AddItemName="add",
	CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public class CacheElementCollection : ConfigurationElementCollection {

		
		/// <summary>
		/// Cache value with given name
		/// </summary>
		public new CacheElement this[string key] {
			get {
				foreach (ConfigurationElement e in this) {
					CacheElement c = (CacheElement)e;
					if (c.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)) {
						return c;
					}
				}
				return null;
			}
		}
		
		protected override ConfigurationElement CreateNewElement() {
			return new CacheElement();
		}
		protected override object GetElementKey(ConfigurationElement element) {
			return ((CacheElement)element).Name;
		}
	}
}
