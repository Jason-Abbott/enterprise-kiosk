using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Configuration {
	[ConfigurationCollection(typeof(AssemblyElement), AddItemName = "add",
	CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public class AssemblyElementCollection : ConfigurationElementCollection {

		protected override ConfigurationElement CreateNewElement() {
			return new AssemblyElement();
		}
		protected override object GetElementKey(ConfigurationElement element) {
			return ((AssemblyElement)element).AssemblyName;
		}
	}
}
