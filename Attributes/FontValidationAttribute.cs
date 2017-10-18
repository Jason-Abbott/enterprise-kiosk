using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Attributes {
	/// <summary>
	/// Ensure that property is the name of a system font
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class FontValidator : ConfigurationValidatorAttribute {
		public override ConfigurationValidatorBase ValidatorInstance {
			get { return new Configuration.Validation.Font(); }
		}
	}
}