using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	/// <summary>
	/// Assertions
	/// </summary>
	public class Assert {

		#region Value in range

		/// <summary>
		/// Assert that a value must fall within a range
		/// </summary>
		/// <param name="value">Value to evaluate</param>
		/// <param name="min">Minimum value</param>
		/// <param name="max">Maximum value</param>
		/// <param name="resx">Resource key to display as error</param>
		public static void Range(int value, int min, int max, string resx) {
			if (value < min || value > max) { ThrowRangeError(resx, value, min, max); }
		}
		public static void Range(byte[] value, int length, string resx) {
			if (value.Length > length) { ThrowRangeError(resx, value, length); }
		}
		public static void Range(string[] value, int length, string resx) {
			if (value.Length > length) { ThrowRangeError(resx, value, length); }
		}

		#endregion

		/// <summary>
		/// Date must be in the future
		/// </summary>
		public static void Future(DateTime t, string resx) {
			if (t < DateTime.Now) {
				throw new SystemException(Resource.Say("Error_{0}", resx));
			}
		}
		
		/// <summary>
		/// Item must be a member of the collection
		/// </summary>
		public static void MemberOf<T>(T item, List<T> list, string resx) {
			if (!list.Contains(item)) {
				throw new System.Exception(Resource.Say("Error_{0}", resx));
			}
		}

		/// <summary>
		/// Item must be a key of the given dictionary
		/// </summary>
		public static void ContainsKey<K, V>(K key, Dictionary<K, V> hash, string resx) {
			if (!hash.ContainsKey(key)) {
				throw new System.IndexOutOfRangeException(string.Format(
					Resource.Say("Error_{0}", resx), key.ToString()));
			}
		}

		/// <summary>
		/// Value must match regex pattern named by resource
		/// </summary>
		public static void MatchesPattern(string text, string resource) {
			if (!Pattern.IsMatch(text, resource)) {
				throw new System.ArgumentException("\"" + text + "\" does not match \"" + Pattern.Load(resource) + "\"");
			}
		}

		/// <summary>
		/// Type must be a subtype of the generic type
		/// </summary>
		public static void SubTypeOf<T>(Type t, string resx) where T : class {
			Assert.NoNull(t, resx);
			if (!t.IsSubclassOf(typeof(T))) {
				throw new System.Exception(Resource.Say("Error_{0}", resx));
			}
		}

		public static void ValidEntity(Entity e, string resx) {
			if (!e.IsValid) { throw new System.Exception(Resource.Say("Error_{0}", resx)); }
		}

		#region Not null

		/// <summary>
		/// Assert that a value cannot be null
		/// </summary>
		/// <param name="value">The value being evaluated</param>
		/// <param name="resx">Resource key to display as error</param>
		public static void NoNull(object value, string resx) {
			if (value == null) { ThrowNullError(resx); }
		}
		/// <param name="value">The value being evaluated</param>
		/// <param name="resx">Resource key to display as error</param>
		/// <param name="value1">String.Format substitution</param>
		public static void NoNull(object value, string resx, object value1) {
			if (value == null) { ThrowNullError(resx, value1); }
		}
		/// <param name="value">The value being evaluated</param>
		/// <param name="resx">Resource key to display as error</param>
		/// <param name="value1">String.Format substitution</param>
		/// <param name="value2">String.Format substitution</param>
		public static void NoNull(object value, string resx, object value1, object value2) {
			if (value == null) { ThrowNullError(resx, value1, value2); }
		}
		public static void NoNull(int value, string resx) {
			if (value == 0) { ThrowNullError(resx); }
		}
		public static void NoNull(string value, string resx) {
			if (string.IsNullOrEmpty(value)) { ThrowNullError(resx); }
		}
		public static void NoNull(string value, string resx, object value1) {
			if (string.IsNullOrEmpty(value)) { ThrowNullError(resx, value1); }
		}
		public static void NoNull(string value, string resx, object value1, object value2) {
			if (string.IsNullOrEmpty(value)) { ThrowNullError(resx, value1, value2); }
		}

		#endregion

		public static void ThrowRangeError(string resx) {
			throw new ArgumentOutOfRangeException(Resource.Say("Error_{0}", resx));
		}
		/// <param name="value">String.Format substitution</param>
		public static void ThrowRangeError(string resx, object value) {
			throw new ArgumentOutOfRangeException(
				string.Format(Resource.Say("Error_{0}", resx), value));
		}
		/// <param name="value1">String.Format substitution</param>
		/// <param name="value2">String.Format substitution</param>
		public static void ThrowRangeError(string resx, object value1, object value2) {
			throw new ArgumentOutOfRangeException(
				string.Format(Resource.Say("Error_{0}", resx), value1, value2));
		}
		/// <param name="value1">String.Format substitution</param>
		/// <param name="value2">String.Format substitution</param>
		/// <param name="value3">String.Format substitution</param>
		public static void ThrowRangeError(string resx, object value1, object value2, object value3) {
			throw new ArgumentOutOfRangeException(
				string.Format(Resource.Say("Error_{0}", resx), value1, value2, value3));
		}

		private static void ThrowNullError(string resx) {
			throw new NullReferenceException(Resource.Say("Error_{0}", resx));
		}
		/// <param name="value">String.Format substitution</param>
		private static void ThrowNullError(string resx, object value) {
			throw new NullReferenceException(
				string.Format(Resource.Say("Error_{0}", resx), value));
		}
		/// <param name="value1">String.Format substitution</param>
		/// <param name="value2">String.Format substitution</param>
		private static void ThrowNullError(string resx, object value1, object value2) {
			throw new NullReferenceException(
				string.Format(Resource.Say("Error_{0}", resx), value1, value2));
		}
	}
}
