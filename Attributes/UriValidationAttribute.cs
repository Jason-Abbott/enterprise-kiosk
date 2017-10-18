using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Attributes {
	/// <summary>
	/// Ensure that property is a URI
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class UriValidator : ConfigurationValidatorAttribute {
		private UriKind _kind;
		
		public UriValidator(UriKind kind) { _kind = kind; }

		public override ConfigurationValidatorBase ValidatorInstance {
			get { return new Configuration.Validation.URI(_kind); }
		}
	}
}