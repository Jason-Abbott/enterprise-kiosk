using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idaho {
	public static class LongExtension {
		/// <summary>
		/// Are the test bits contained by the flag
		/// </summary>
		/// <param name="source">The containing bits</param>
		/// <param name="test">The bits possibly present in the containing bits</param>
		public static bool ContainsBits(this long source, long test) {
			return (source & test) > 0;
		}
		public static bool ContainsBits(this long source, System.Enum test) {
			return source.ContainsBits(Convert.ToInt64(test));
		}
		public static bool IsFlag(this long i) {
			double exponent = Math.Round(Math.Log(i) / Math.Log(2), 4);
			return (exponent - Math.Floor(exponent) == 0);
		}
		public static long RemoveBits(this long source, long remove) {
			return source & ~remove;
		}
		public static long RemoveBits(this long source, System.Enum remove) {
			return source & ~Convert.ToInt64(remove);
		}
		public static long AddBits(this long source, long add) {
			return source | add;
		}
		public static long AddBits(this long source, System.Enum add) {
			return source | Convert.ToInt64(add);
		}
	}
}
