using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Attributes {
	/// <summary>
	/// Mark fields that can be updated
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class Updatable : System.Attribute  {

	}
}
