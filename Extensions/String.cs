using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;

namespace Idaho {
	public static class StringExtension {

		public enum HashType { sha1, md5 }

		/// <summary>
		/// Convert a string to an enumeration of the given type
		/// </summary>
		/// <typeparam name="T">Type of enumeration</typeparam>
		/// <param name="x">Enumeration name</param>
		public static T ToEnum<T>(this string x) {
			return (T)Enum.Parse(typeof(T), x);
		}

		/// <summary>
		/// Convert string to integer
		/// </summary>
		public static int ToInteger(this string x, int emptyValue) {
			string numbers = x.NumbersOnly();
			if (!string.IsNullOrEmpty(numbers)) {
				return int.Parse(numbers);
			} else {
				return emptyValue;
			}
		}
		public static int ToInteger(this string x) {
			return ToInteger(x, 0);
		}

		/// <summary>
		/// Convert delimited name-value pairs to JSON array
		/// </summary>
		/// <example>A string of "one|two|three" can be converted to [one,two,three]</example>
		public static string ToScriptArray(this string list, char delim, bool quoted) {
			if (string.IsNullOrEmpty(list)) {
				return "[]";
			} else {
				string pattern = (quoted) ? "[\"{0}\"]" : "[{0}]";
				string replace = (quoted) ? "\",\"" : ",";

				return string.Format(pattern,
					list.Trim(new char[] { delim, ' ' }).Replace(delim.ToString(), replace));
			}
		}
		public static string ToScriptArray(this string list) {
			return ToScriptArray(list, true);
		}
		public static string ToScriptArray(this string list, bool quoted) {
			return ToScriptArray(list, ';', quoted);
		}

		#region Formatting

		/// <summary>
		/// Escape EcmaScript special characters
		/// </summary>
		public static string EscapeForScript(this string raw) {
			if (string.IsNullOrEmpty(raw)) { return string.Empty; }
			string newText = System.Web.HttpUtility.UrlPathEncode(raw);
			newText = newText.Replace("\"", "%22");
			newText = newText.Replace(Environment.NewLine, "%0A");
			return newText;
		}

		/// <summary>
		/// Add spacing in camel-cased runons, such as enumeration names
		/// </summary>
		/// <example>JasonAbbott will be converted to Jason Abbott</example>
		public static string FixSpacing(this string text) {
			string spaced = Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
			spaced = Regex.Replace(spaced, "^(3[dD])(\\w)", "$1 $2");
			return spaced.Replace("_", " ").Trim();
		}

		/// <summary>
		/// Convert words to first character uppercase
		/// </summary>
		/// <remarks>Attempts to handle short acronyms and abbreviations</remarks>
		public static string ProperCase(this string text) {
			if (string.IsNullOrEmpty(text)) { return string.Empty; }
			StringBuilder formatted = new StringBuilder();
			string word;
			//Regex re = new Regex("[a-zA-Z]+");
			Regex re = new Regex("\\w+");
			MatchCollection matches = re.Matches(text.Trim());

			for (int x = 0; x < matches.Count; x++) {
				word = matches[x].Value;
				if (matches[x].Index > 0) { formatted.Append(text.Substring(matches[x].Index - 1, 1)); }
				if (word.Length == 3) {
					switch (word.ToLower()) {
						case "llc": formatted.Append("LLC"); break;
						case "inc": formatted.Append("Inc"); break;
						case "ste": formatted.Append("Suite"); break;
						case "san": formatted.Append("SAN"); break;
						case "cpu": formatted.Append("CPU"); break;
						default: formatted.Append(word.ToLower()); break;
					}
				} else if (word.Length > 2) {
					// words containing numbers (such as model numbers) should be all upper
					if (Regex.IsMatch(word, "\\d", RegexOptions.Multiline)) {
						formatted.Append(word.ToUpper());
					} else {
						formatted.Append(word.Substring(0, 1).ToUpper());
						formatted.Append(word.Substring(1).ToLower());
					}
				} else {
					switch (word.ToLower()) {
						case "jr": formatted.Append("Jr."); break;
						case "dr": formatted.Append("Dr."); break;
						case "of": formatted.Append("of"); break;
						case "on": formatted.Append("on"); break;
						default: formatted.Append(word); break;
					}
				}
			}
			return formatted.ToString().Trim();
		}
		/// <summary>
		/// Remove all characters other than numbers
		/// </summary>
		public static string NumbersOnly(this string number) {
			Regex re = new Regex("\\D", RegexOptions.Multiline);
			string clean = re.Replace(number, "");
			if (!string.IsNullOrEmpty(clean)) { clean = clean.Trim(); }
			return clean;
		}
		/// <summary>
		/// Remove characters unsafe for web display and trim to given length
		/// </summary>
		public static string SafeForWeb(this string text, int length) {
			if (!string.IsNullOrEmpty(text) && text.Length > length) {
				text = text.Substring(0, length);
			}
			return SafeForWeb(text);
		}
		public static string SafeForWeb(this string text) {
			if (!string.IsNullOrEmpty(string.Empty)) {
				Regex re = new Regex(Idaho.Pattern.SafeHtml);
				text = re.Replace(text, "");
			}
			return text;
		}

		/// <summary>
		/// Capitalize first letter only
		/// </summary>
		/// <remarks>
		/// select * from [user] where lastname like 'Mc%' or lastname like '%''%'
		/// </remarks>
		public static string Capitalize(this string text) {
			if (!string.IsNullOrEmpty(text)) {
				if (text.Contains("'")) {
					// for names like O'Brian also capitalize after apostrophe
					return InnerCapital(text.IndexOf('\''), text);
				} else if (text.Contains("-")) {
					// for hyphenated names
					return InnerCapital(text.IndexOf('-'), text);
				} else if (text.StartsWith("Mc", Format.IgnoreCase)) {
					// for names like McNiell also capitalize after "Mc"
					return InnerCapital(1, text);
				} else {
					return text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
				}
			}
			return text;
		}
		/// <summary>
		/// For names with an inner capital letter
		/// </summary>
		private static string InnerCapital(int index, string text) {
			string output = text.Substring(0, 1).ToUpper();
			output += text.Substring(1, index).ToLower();
			output += text.Substring(index + 1, 1).ToUpper();
			output += text.Substring(index + 2).ToLower();
			return output;
		}

		#endregion

		#region Pattern matching

		/// <summary>
		/// Does string match pattern
		/// </summary>
		public static bool Matches(this string text, string pattern, RegexOptions options) {
			return Regex.IsMatch(text, pattern, options);
		}
		public static bool Matches(this string text, string pattern) {
			return Matches(text, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
		}
		public static bool Matches(this string text, string pattern, string sub1) {
			return Matches(text, string.Format(pattern, sub1));
		}
		public static bool Matches(this string text, string pattern,
			string sub1, string sub2) {
			return Matches(text, string.Format(pattern, sub1, sub2));
		}
		public static bool Matches(this string text, string pattern,
			string sub1, string sub2, string sub3) {
			return Matches(text, string.Format(pattern, sub1, sub2, sub3));
		}

		#endregion

		#region Encryption

		/// <summary>
		/// Encrypt string with salt
		/// </summary>
		public static string Encrypt(this string text, string salt, HashType type) {
			return FormsAuthentication.HashPasswordForStoringInConfigFile(text + salt, type.ToString());
		}
		public static string Encrypt(this string text, string salt) {
			return Encrypt(text, salt, HashType.md5);
		}
		public static string Encrypt(this string text) {
			return (!string.IsNullOrEmpty(text)) ? Encrypt(text, Resource.Say("Salt")) : text;
		}
		public static string Encyrpt(this string text, HashType type) {
			return Encrypt(text, string.Empty, HashType.md5);
		}

		#endregion

	}
}
