using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Configuration.Validation {
	/// <summary>
	/// Validate a configuration value based on pattern loaded from assembly resource
	/// </summary>
	public class Pattern : ConfigurationValidatorBase {
		private string _resource = string.Empty;

		/// <summary>
		/// Construct a validator using the pattern with the given resource name
		/// </summary>
		/// <param name="resource">Name of resource in assembly resource file</param>
		public Pattern(string resource) { _resource = resource; }
		protected Pattern() { }

		public override void Validate(object value) { this.Validate((string)value); }
		public void Validate(string value) { Assert.MatchesPattern(value, _resource); }
		public override bool CanValidate(Type type) { return (type == typeof(string)); }
	}
}
