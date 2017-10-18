using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Configuration {
	/// <summary>
	/// Configuration items that can load their own values and set relevant properties
	/// in the assembly
	/// </summary>
	interface ISelfLoad<T> {
		// load default settings
		void ApplySettings();
		// load settings for a specific instance
		bool ApplySettings(string key, T entity);
	}
}
