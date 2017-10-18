using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Configuration {
	[ConfigurationCollection(typeof(ButtonElement), AddItemName = "add",
	CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public class ButtonElementCollection : ConfigurationElementCollection {

		/// <summary>
		/// Cache value with given name
		/// </summary>
		public new ButtonElement this[string key] {
			get {
				ButtonElement b = null;

				if (string.IsNullOrEmpty(key)) {
					return this.Default;
				} else {
					foreach (ConfigurationElement e in this) {
						b = (ButtonElement)e;
						if (b.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)) {
							return b;
						}
					}
					// revert to default if key match not found
					b = this.Default;
				}
				return b;
			}
		}

		/// <summary>
		/// The default button element
		/// </summary>
		public ButtonElement Default {
			get {
				foreach (ConfigurationElement e in this) {
					ButtonElement b = (ButtonElement)e;
					if (b.IsDefault) { return b; }
				}
				return null;
			}
		}

		protected override ConfigurationElement CreateNewElement() {
			return new ButtonElement();
		}
		protected override object GetElementKey(ConfigurationElement element) {
			return ((ButtonElement)element).Name;
		}
	}
}
