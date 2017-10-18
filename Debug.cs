using System;
using System.Diagnostics;

namespace Idaho {

	/// <summary>
	/// A simpler interface to debug methods
	/// </summary>
	/// <remarks>Uses Conditional() attribute to hide methods from release code</remarks>
	public class Debug {

#region BugCheck()

		[Conditional("DEBUG")]
		public static void BugCheck(bool condition, string text) {
			if (condition) { BugOut(text); }
		}

		[Conditional("DEBUG")]
		public static void BugCheck(bool condition, string text, object arg0) {
			if (condition) { BugOut(text, arg0); }
		}

		[Conditional("DEBUG")]
		public static void BugCheck(bool condition, string text, object arg0, object arg1) {
			if (condition) { BugOut(text, arg0, arg1); }
		}

#endregion

		[Conditional("DEBUG")]
		public static void BugTab() { System.Diagnostics.Debug.Indent(); }

		[Conditional("DEBUG")]
		public static void BugUntab() { System.Diagnostics.Debug.Unindent(); }

#region BugOut()

		[Conditional("DEBUG")]
		public static void BugOut(string text) { System.Diagnostics.Debug.WriteLine(text); }

		[Conditional("DEBUG")]
		public static void BugOut(string text, object arg0) {
			System.Diagnostics.Debug.WriteLine(string.Format(text, arg0));
		}

		[Conditional("DEBUG")]
		public static void BugOut(string text, object arg0, object arg1) {
			System.Diagnostics.Debug.WriteLine(string.Format(text, arg0, arg1));
		}

		[Conditional("DEBUG")]
		public static void BugOut(string text, object arg0, object arg1, object arg2) {
			System.Diagnostics.Debug.WriteLine(string.Format(text, arg0, arg1, arg2));
		}

		[Conditional("DEBUG")]
		public static void BugOut(string text, object arg0, object arg1, object arg2, object arg3) {
			System.Diagnostics.Debug.WriteLine(string.Format(text, arg0, arg1, arg2, arg3));
		}

#endregion

	}
}