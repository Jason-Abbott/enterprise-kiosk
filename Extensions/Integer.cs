using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	public static class IntegerExtension {

		/// <summary>
		/// Convert a string to an enumeration of the given type
		/// </summary>
		/// <typeparam name="T">Type of enumeration</typeparam>
		/// <param name="x">Enumeration name</param>
		public static T ToEnum<T>(this int x) {
			return (T)(object)x;
		}

		/// <summary>
		/// Are the test bits contained by the flag
		/// </summary>
		/// <param name="source">The containing bits</param>
		/// <param name="test">The bits possibly present in the containing bits</param>
		public static bool ContainsBits(this int source, int test) {
			return (source & test) > 0;
		}
		public static bool ContainsBits(this int source, System.Enum test) {
			return source.ContainsBits(Convert.ToInt32(test));
		}
		public static bool IsFlag(this int i) {
			double exponent = Math.Round(Math.Log(i) / Math.Log(2), 4);
			return (exponent - Math.Floor(exponent) == 0);
		}
		public static int RemoveBits(this int source, int remove) {
			return source & ~remove;
		}
		public static int RemoveBits(this int source, System.Enum remove) {
			return source & ~Convert.ToInt32(remove);
		}
		public static int AddBits(this int source, int add) {
			return source | add;
		}
		public static int AddBits(this int source, System.Enum add) {
			return source | Convert.ToInt32(add);
		}

		/// <summary>
		/// Display "Odd" or "Even" for given number
		/// </summary>
		/// <remarks>Appended to class names in style sheets</remarks>
		public static string SayOddEven(this int number) {
			return (number % 2 == 0) ? "Even" : "Odd";
		}

		/// <summary>
		/// The word for a number
		/// </summary>
		public static string ToName(this int number) {
			switch (number) {
				case 0: return Resource.Say("Number_Zero");
				case 1: return Resource.Say("Number_One");
				case 2: return Resource.Say("Number_Two");
				case 3: return Resource.Say("Number_Three");
				case 4: return Resource.Say("Number_Four");
				case 5: return Resource.Say("Number_Five");
				case 6: return Resource.Say("Number_Six");
				case 7: return Resource.Say("Number_Seven");
				case 8: return Resource.Say("Number_Eight");
				case 9: return Resource.Say("Number_Nine");
				case 10: return Resource.Say("Number_Ten");
				case 11: return Resource.Say("Number_Eleven");
				case 12: return Resource.Say("Number_Twelve");
				case 13: return Resource.Say("Number_Thirteen");
				case 14: return Resource.Say("Number_Fourteen");
				case 15: return Resource.Say("Number_Fifteen");
				case 16: return Resource.Say("Number_Sixteen");
				case 17: return Resource.Say("Number_Seventeen");
				case 18: return Resource.Say("Number_Eighteen");
				case 19: return Resource.Say("Number_Nineteen");
				case 20: return Resource.Say("Number_Twenty");
				default: return number.ToString();
			}
		}
	}
}
