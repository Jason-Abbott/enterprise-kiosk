using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Text;

namespace Idaho.Configuration.Validation {
	/// <summary>
	/// Validate a configuration value against available system font names
	/// </summary>
	public class Font : ConfigurationValidatorBase {
		private string _name = string.Empty;

		public override void Validate(object value) {
			string name = (string)value;
			if (string.IsNullOrEmpty(name)) {
				throw new ConfigurationErrorsException("Empty string cannot be converted to a font");
			} else {
				System.Drawing.Font f = new System.Drawing.Font(name, 10);
				if (f == null) {
					throw new ConfigurationErrorsException("\"" + name
						+ "\" is not a recognized system font");
				}
			}
		}
		public override bool CanValidate(Type type) { return (type == typeof(string)); }
	}
}
