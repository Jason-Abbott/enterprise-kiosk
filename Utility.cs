using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Idaho {
	public enum KeyCode {
		Backspace = 8,
		Tab = 9,
		Enter = 13,
		Shift = 16,
		Ctrl = 17,
		Alt = 18,
		Pause = 19,
		CapsLock = 20,
		Escape = 27,
		PageUp = 33,
		Space = 32,
		PageDown = 34,
		End = 35,
		Home = 36,
		ArrowLeft = 37,
		ArrowUp = 38,
		ArrowRight = 39,
		ArrowDown = 40,
		PrintScreen = 44,
		Insert = 45,
		Delete = 46,
		Number0 = 48,
		Number1 = 49,
		Number2 = 50,
		Number3 = 51,
		Number4 = 52,
		Number5 = 53,
		Number6 = 54,
		Number7 = 55,
		Number8 = 56,
		Number9 = 57,
		LetterA = 65,
		LetterB = 66,
		LetterC = 67,
		LetterD = 68,
		LetterE = 69,
		LetterF = 70,
		LetterG = 71,
		LetterH = 72,
		LetterI = 73,
		LetterJ = 74,
		LetterK = 75,
		LetterL = 76,
		LetterM = 77,
		LetterN = 78,
		LetterO = 79,
		LetterP = 80,
		LetterQ = 81,
		LetterR = 82,
		LetterS = 83,
		LetterT = 84,
		LetterU = 85,
		LetterV = 86,
		LetterW = 87,
		LetterX = 88,
		LetterY = 89,
		LetterZ = 90,
		WindowKeyLeft = 91,
		WindowKeyRight = 92,
		SelectKey = 93,
		NumberPad0 = 96,
		NumberPad1 = 97,
		NumberPad2 = 98,
		NumberPad3 = 99,
		NumberPad4 = 100,
		NumberPad5 = 101,
		NumberPad6 = 102,
		NumberPad7 = 103,
		NumberPad8 = 104,
		NumberPad9 = 105,
		Multiply = 106,
		Add = 107,
		Subtract = 109,
		DecimalPoint = 110,
		Divide = 111,
		F1 = 112,
		F2 = 113,
		F3 = 114,
		F4 = 115,
		F5 = 116,
		F6 = 117,
		F7 = 118,
		F8 = 119,
		F9 = 120,
		F10 = 121,
		F11 = 122,
		F12 = 123,
		NumberLock = 144,
		ScrollLock = 154,
		SemiColon = 186,
		Equals = 187,
		Comma = 188,
		Hyphen = 189,
		Period = 190,
		SlashForward = 191,
		SlashBackward = 220,
		BracketOpen = 219,
		BracketClose = 221,
		SingleQuote = 222
	}

	public static class Utility {

		private static string _basePath = string.Empty;

		#region NullSafe

		public static T NoNull<T>(T firstChoice, T secondChoice) {
			if (typeof(T).Equals(typeof(string))) {
				return (string.IsNullOrEmpty(Convert.ToString(firstChoice)))
					? secondChoice : firstChoice;
			} else {
				return (firstChoice.Equals(default(T))) ? secondChoice : firstChoice;
			}
		}
		public static T NoNull<T>(T firstChoice, T secondChoice, T thirdChoice) {
			if (firstChoice.Equals(default(T))) {
				return (secondChoice.Equals(default(T))) ? thirdChoice : secondChoice;
			} else {
				return firstChoice;
			}
		}
		/// <summary>
		/// Generic conversion to specific type, returning default value for null
		/// </summary>
		public static T NullSafe<T>(object o) {
			return (o != null && !o.Equals(DBNull.Value)) ? (T)o : default(T);
		}
		public static T NullSafe<T>(object o, T secondChoice) {
			return (o != null && !o.Equals(DBNull.Value)) ? (T)o : secondChoice;
		}

		#endregion

		#region Constraints

		/// <summary>
		/// Return the minimum value if less than minimum,
		/// maximum if more than maximum, or the value itself
		/// </summary>
		public static int Constrain(int value, int min, int max) {
			if (value < min) { return min; }
			if (value > max) { return max; }
			return value;
		}

		public static byte Constrain(float value, float min, float max) {
			if (value < min) { return (byte)min; }
			if (value > max) { return (byte)max; }
			return (byte)value;
		}

		#endregion

		#region Reflection

		/// <summary>
		/// Case insensitive search in assembly for abbreviated type name
		/// </summary>
		public static Type GetType(Assembly host, string typeName) {
			typeName = typeName.ToLower();
			foreach (Type t in host.GetTypes()) {
				if (t.Name.Equals(typeName, Format.IgnoreCase)) { return t; }
			}
			return null;
		}
		public static Type GetType(string typeName) {
			return GetType(Assembly.GetExecutingAssembly(), typeName);
		}

		#endregion

		/// <summary>
		/// Does the given browser support PNG transparency
		/// </summary>
		public static bool HandlesTransparency(HttpBrowserCapabilities bc) {
			return !(bc.Browser == "IE" && bc.MajorVersion == 6 && bc.Platform.Contains("Win"));
		}

		/// <summary>
		/// Normalized web path
		/// </summary>
		/// <remarks>Removes redundant slash</remarks>
		public static string BasePath {
			get {
				if (_basePath == string.Empty) {
					_basePath = HttpRuntime.AppDomainAppVirtualPath;
					if (_basePath == "/") { _basePath = ""; }
				}
				return _basePath;
			}
		}

		/// <summary>
		/// Return page name of url
		/// </summary>
		public static string FileName(string url) {
			//TODO: see of .NET has function for this
			if (url == string.Empty) { return url; }
			int start = 0;
			int finish = url.LastIndexOf('.');
			if (url.IndexOf('/') > -1) { start = url.IndexOf('/'); }
			return url.Substring(start, finish);
		}

		/// <summary>
		/// Generate a simple, random set of letters and numbers
		/// </summary>
		public static string RandomText(int length) {
			StringBuilder result = new StringBuilder();
			Random random = new Random();
			double r;

			for (int x = 1; x <= length; x++) {
				// alternate random numbers and letters
				//ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
				r = (x % 2 == 0) ? (25 * random.NextDouble() + 66) : (10 * random.NextDouble() + 48);
				//randomNumber = (int)((x % 2 == 0) ? (int)(25 * Math.Rnd()) + 1 + 65 : (int)(10 * VBMath.Rnd()) + 1 + 47);
				result.Append(Convert.ToChar((int)r));
			}
			return result.ToString().ToLower();
		}

		#region Compression

		/// <summary>
		/// Apply GZIP compression to the given string
		/// </summary>
		public static byte[] Compress(byte[] value) {
			byte[] output;
			MemoryStream ms = new MemoryStream();

			using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true)) {
				zip.Write(value, 0, value.Length);
			}
			output = new byte[ms.Length];
			ms.Position = 0;
			ms.Read(output, 0, output.Length);
			ms.Dispose();
			return output;
		}
		public static byte[] Compress(string value) {
			return Compress(Encoding.UTF8.GetBytes(value));
		}

		public static byte[] Decompress(byte[] value) {
			MemoryStream ms = new MemoryStream(value);
			MemoryStream output = new MemoryStream();
			GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
			byte[] buffer = new byte[1024];
			int length = zip.Read(buffer, 0, buffer.Length);

			while (length != 0) { output.Write(buffer, 0, length); }

			return output.ToArray();
		}

		#endregion

	}
}
