using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Attributes {
	/// <summary>
	/// Ensure that property matches the given regular expression pattern
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class PatternValidator : ConfigurationValidatorAttribute {

		private string _resource = string.Empty;
		
		/// <summary>
		/// Validate property setter value against pattern identified in resource file
		/// </summary>
		/// <param name="resource">Assembly resource value name</param>
		public PatternValidator(string resource) { _resource = resource; }

		public override ConfigurationValidatorBase ValidatorInstance {
			get { return new Configuration.Validation.Pattern(_resource); }
		}
	}
}