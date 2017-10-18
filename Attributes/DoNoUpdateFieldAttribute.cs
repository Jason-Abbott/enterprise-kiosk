using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Attributes {
	/// <summary>
	/// Mark fields that should not be updated through reflection
	/// when using Object.Import()
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Event)]
	public class NoUpdate : System.Attribute {

	}
}
