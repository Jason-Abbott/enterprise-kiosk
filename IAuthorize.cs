using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace Idaho {
	/// <summary>
	/// Determine if a user is allowed to view a specific page
	/// </summary>
	public interface IAuthorize {
		bool IsAllowed(IIdentity identity, string pageName);
	}
}
