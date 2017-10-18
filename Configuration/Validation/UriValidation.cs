using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Configuration.Validation {
	class URI : ConfigurationValidatorBase {
        private UriKind _kind;

        public URI(UriKind kind) { _kind = kind; }

		public override bool CanValidate(Type type) { return type == typeof(string); }

        public override void Validate(object value) {
			string uri = (string)value;
			if (!Uri.IsWellFormedUriString(uri, _kind)) {
				throw new ConfigurationErrorsException("\"" + uri + "\" is not a valid URL");
			}
		}
	}
}
