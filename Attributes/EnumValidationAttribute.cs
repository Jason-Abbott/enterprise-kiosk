using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Attributes {
	/// <summary>
	/// Ensure that property is of the given enumeration type
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class EnumValidator : ConfigurationValidatorAttribute {
		private Type _enumType;

		public EnumValidator(Type enumType) { _enumType = enumType; }

		public override ConfigurationValidatorBase ValidatorInstance {
			get { return new Configuration.Validation.Enumeration(_enumType); }
		}
	}
}