using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Indicate controls that can load their own configuration information
	/// </summary>
	interface IConfigurable<T> where T:System.Configuration.ConfigurationSection {
		void LoadConfiguration();
	}
}
