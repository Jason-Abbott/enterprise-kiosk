using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idaho {
	public static class StringBuilderExtension {

		/// <summary>
		/// Remove last character
		/// </summary>
		public static void TrimEnd(this StringBuilder sb) {
			if (sb.Length > 1) { sb.Length = sb.Length - 1; }
		}
	}
}
