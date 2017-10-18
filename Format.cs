using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Idaho {
	public enum Compare {
		LessThan,
		GreaterThan,
		EqualTo,
		NotEqualTo
	}

	// http://blog.stevex.net/index.php/string-formatting-in-csharp/
	public static class Format {
		private static string _anyRe = "[^~]";
		public static StringComparison IgnoreCase = StringComparison.InvariantCultureIgnoreCase;
		
		/// <summary>
		/// Wrap text to the given length
		/// </summary>
		/// <param name="lineCount">Number of lines needed to fit text to length</param>
		public static string Wrap(string text, int lineLength, out int lineCount) {
			StringBuilder formatted = new StringBuilder();
			bool content = false;
			lineCount = 0;

			if (!string.IsNullOrEmpty(text)) {
				int index = 0;
				lineCount = (int)(text.Length / lineLength);
				if (text.Length % lineLength > 0) { lineCount++; }
				int remainingLines = lineCount;
				string remainingText = text;
				string line = string.Empty;
				string consider = string.Empty;

				while (remainingLines > 0 && remainingText.Length > 0) {
					if (content) { formatted.Append(Environment.NewLine); }

					if (remainingText.Length <= lineLength) {
						formatted.Append(remainingText);
						remainingText = string.Empty;
					} else {
						consider = remainingText.Substring(0, lineLength);
						index = consider.LastIndexOf(' ');
						if (index <= 0) { index = consider.LastIndexOf('/'); }
						if (index <= 0) { index = consider.LastIndexOf('-'); }
						if (index <= 0) { index = consider.LastIndexOf('.'); }
						// if no logical break point then force a break
						if (index <= 0) { index = lineLength; }
						line = consider.Substring(0, index);
						formatted.Append(line.Trim());
						remainingText = remainingText.Replace(line, "");
					}
					content = true;
					remainingLines--;
				}
			}
			return formatted.ToString();
		}

		/// <summary>
		/// Convert text to abbreviated form
		/// </summary>
		/// <example>Jason Abbott will be converted to JA</example>
		public static string Abbreviate(string text) {
			if (string.IsNullOrEmpty(text)) { return string.Empty; }
			text = text.ToLower();

			// known exceptions
			if (text.EndsWith("administration")) { return "ADM"; }
			switch (text) {
				case "legal": return "LE";
				case "policy": return "POL";
				case "commissioners": return "COM";
				case "bankruptcy": return "BKR";
				case "ftrf audit": return "FTRF";
				case "taxpayer services": return "TPS";
				case "unclaimed property support": return "UP";
			}
			StringBuilder formatted = new StringBuilder();
			string word;
			Regex re = new Regex("[a-zA-Z]+");
			MatchCollection matches = re.Matches(text.Trim());
			for (int x = 0; x < matches.Count; x++) {
				word = matches[x].Value;
				formatted.Append(word.Substring(0, 1).ToUpper());
			}
			return formatted.ToString().Trim();
		}

		public static string CurlyQuote(string text) {
			if (!string.IsNullOrEmpty(text)) {
				text = Regex.Replace(text, "(^|\\W)\"(\\w)", "$1" + Web.Symbol.LeftQuote + "$2");
				text = Regex.Replace(text, "(\\w|\\.|,)\"(\\W|$)", "$1" + Web.Symbol.RightQuote + "$2");
				text = Regex.Replace(text, "(I|\\w{2})'(\\w{1,2})\\s", "$1" + Web.Symbol.Apostrophe + "$2 ");
				text = Regex.Replace(text, "(^|\\W)'(\\w)", "$1" + Web.Symbol.LeftSingleQuote + "$2");
				text = Regex.Replace(text, "(\\w|\\.)'(\\W|$)", "$1" + Web.Symbol.RightSingleQuote + "$2");
			}
			return text;
		}

		/// <summary>
		/// Format special characters
		/// </summary>
		public static string SpecialCharacters(string text) {
			if (!string.IsNullOrEmpty(text)) {
				text = text.Replace("->", Web.Symbol.RightArrow);
				text = text.Replace("<-", Web.Symbol.LeftArrow);
				text = text.Replace("--", Web.Symbol.EmDash);
				text = Regex.Replace(text, "\\.\\s*\\.\\s*\\.", Web.Symbol.Ellipses);
				text = Regex.Replace(text, "(\\d+)-(\\d+)", "$1" + Web.Symbol.EnDash + "$2");
			}
			return text;
		}

		/// <summary>
		/// Convert plain text list to HTML list
		/// </summary>
		/// <param name="text">Text to be converted</param>
		/// <param name="bullet">Bullet character to replace</param>
		/// <param name="fromHtml">Whether the overall text is already HTML</param>
		public static string AsHtmlList(string text, char bullet, bool fromHtml) {
			string newLine = fromHtml ? "<br/>" : Environment.NewLine;
			string find = newLine + bullet;
			if (text.Contains(find)) {
				string output = string.Empty;
				string list = string.Empty;
				int start = text.StartsWith(bullet.ToString()) ? 0 : text.IndexOf(find);
				int last = text.LastIndexOf(find);

				last = text.IndexOf(newLine, last + find.Length);
				if (last < 0) { last = text.Length; }

				list = text.Substring(start, last - start);
				list = list.Trim(new char[] { bullet });
				list = list.Replace(find, "</li><li>");
				list = "<ul><li>" + list + "</li></ul>";
				list = list.Replace("<li></li><li>", "<li>");
				list = list.Replace("<li><li>", "<li>");
				list = list.Replace("<ul></li>", "<ul>");

				output = text.Substring(0, start);
				output += list;

				if (last < text.Length) {
					output += text.Substring(last, text.Length - last);
				}
				return output;
			}
			return text;
		}
		public static string AsHtmlList(string text) {
			return AsHtmlList(text, '-', true);
		}
		public static string AsHtmlList(string text, bool fromHtml) {
			return AsHtmlList(text, '-', fromHtml);
		}
		public static string AsHtmlList(string text, char bullet) {
			return AsHtmlList(text, bullet, true);
		}

		/// <summary>
		/// Format plain text as HTML
		/// </summary>
		public static string ToHtml(string text, bool paragraph) {
			if (!string.IsNullOrEmpty(text)) {
				if (paragraph) {
					text = text.Replace(Environment.NewLine + Environment.NewLine, "</p><p>");
				}
				text = text.Replace(Environment.NewLine, "<br/>");
				text = SpecialCharacters(text);
				text = CurlyQuote(text);
			}
			return (paragraph) ? "<p class=\"first\">" + text + "</p>" : text;
		}
		public static string AsHtml(string text) { return ToHtml(text, false); }
	}
}
