using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idaho {
	public static class DoubleExtension {

		public static string ToName(this double number) {
			if ((int)number == number) {
				return IntegerExtension.ToName((int)number);
			} else {
				return number.ToString();
			}
		}
	}
}
