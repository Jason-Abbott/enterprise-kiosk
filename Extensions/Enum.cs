using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Idaho {
	public static class EnumExtension {

		/// <summary>
		/// Does the bitmask enumeration contain the given bitmask
		/// </summary>
		public static bool Contains(this Enum source, Enum test) {
			return Convert.ToInt64(source).ContainsBits(test);
		}
		public static bool Contains(this Enum source, Enum[] test) {
			foreach (Enum e in test) {
				if (!Convert.ToInt32(source).ContainsBits(e)) { return false; }
			}
			return true;
		}
		// TODO: paramarray
		public static bool Contains(this Enum source, Enum test1, Enum test2) {
			return Contains(source, new Enum[] { test1, test2 });
		}
		public static T Add<T>(this Enum source, Enum add) {
			int sum = Convert.ToInt32(source).AddBits(add);
			return (T)Convert.ChangeType(sum, typeof(T));
		}

		public static T Remove<T>(this Enum source, Enum remove) {
			int diff = Convert.ToInt32(source).RemoveBits(remove);
			return (T)Convert.ChangeType(diff, typeof(T));
		}
		public static T Combine<T>(this Enum source, Enum other, bool add) {
			return (add) ? source.Add<T>(other) : source.Remove<T>(other);
		}
		
		/// <summary>
		/// Identify unique bitmask values
		/// </summary>
		/// <remarks>Bitmask values are even powers of two</remarks>
		public static bool IsFlag(this Enum e) {
			return Convert.ToInt32(e).IsFlag();
		}
		/// <summary>
		/// Enumeration name
		/// </summary>
		/// <typeparam name="T">Type of enumeration</typeparam>
		public static string Name<T>(this Enum e) {
			return System.Enum.GetName(typeof(T), e);
		}
		public static List<string> Names<T>(this Enum e) {
			var list = new List<string>();
			var names = System.Enum.GetNames(typeof(T));
			foreach (string n in names) { list.Add(n); }
			return list;
		}

		/// <summary>
		/// Convert flags contained in mask to list of separate flags
		/// </summary>
		public static List<T> Split<T>(this Enum flag) where T : struct {
			var list = new List<T>();
			int match = Convert.ToInt32(flag);
			int[] flags = (int[])System.Enum.GetValues(typeof(T));

			foreach (int f in flags) {
				if (match.ContainsBits(f)) { list.Add((T)(object)f); }
			}
			return list;
		}

		/// <summary>
		/// Convert camel case or underscores to spaced words
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static string ToWords(this Enum e) {
			return e.ToString().FixSpacing();
		}
	}
}
