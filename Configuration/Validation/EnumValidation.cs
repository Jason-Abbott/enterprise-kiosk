using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Configuration.Validation {
	/// <summary>
	/// Validate a configuration value against an enumeration type
	/// </summary>
	public class Enumeration : ConfigurationValidatorBase {

		private Type _enumType = null;

		/// <summary>
		/// Construct a validator for an enumeration type
		/// </summary>
		/// <param name="enumType">Type of enumeration</param>
		public Enumeration(Type enumType) { _enumType = enumType; }
		protected Enumeration() { }

		public override void Validate(object value) {
			string name = (string)value;
			if (string.IsNullOrEmpty(name)) {
				throw new ConfigurationErrorsException(
					"Empty string is not a member of the \""
					+ _enumType.ToString() + "\" enumeration");
			} else {
				object match = Enum.Parse(_enumType, name, true);
				if (match == null) {
					throw new ConfigurationErrorsException(
						"\"" + name + "\" is not a member of the \""
						+ _enumType.ToString() + "\" enumeration");
				}
			}
		}

		public override bool CanValidate(Type type) { return (type == typeof(string)); }
	}
}
