using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Idaho {
	/// <summary>
	/// Common regular expression patterns
	/// </summary>
	public static class Pattern {

		#region Properties

		public static string Guid { get { return Load("GUID"); } }
		public static string SSN { get { return Load("SSN"); } }
		public static string SafeHtml { get { return Load("SafeHtml"); } }
		public static string NetworkPath { get { return Load("NetworkPath"); } }
		//<a href=\"gentax-request-search.aspx?developer=cd8ab5a8-44d0-46ee-8cb3-cf1e297c3853\">Matthew Groves</a>
		public static string TagContent { get { return Load("HtmlTagContent"); } }

		#endregion

		/// <summary>
		/// Load regular expression pattern from named resource
		/// </summary>
		public static string Load(string name) { return Resource.Say("RegEx_" + name); }

		/// <summary>
		/// Create regular expression object from named resource
		/// </summary>
		public static Regex Load(string resource, RegexOptions options) {
			return new Regex(Load(resource), options);
		}

		/// <summary>
		/// Does string match pattern
		/// </summary>
		public static bool IsMatch(string text, string resource, RegexOptions options) {
			return Regex.IsMatch(text, Load(resource), options);
		}
		public static bool IsMatch(string text, string resource) {
			return IsMatch(text, resource, RegexOptions.Multiline | RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Perform regular expression replacement using named resource for pattern
		/// </summary>
		public static void Replace(ref string text, string resource, string replacement, RegexOptions options) {
			text = Regex.Replace(text, Load(resource), replacement, options);
		}
		public static void Replace(ref string text, string resource, string replacement) {
			Replace(ref text, resource, replacement, RegexOptions.Multiline | RegexOptions.IgnoreCase);
		}
	}
}
