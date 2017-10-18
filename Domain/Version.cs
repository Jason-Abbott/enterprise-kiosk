using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	/// <summary>
	/// Represent octet version such as 2.14.123.12432
	/// </summary>
	public class Version : IEquatable<Version> {
		private int[] _version = new int[4];
		private const int _major = 0;
		private const int _minor = 1;
		private const int _build = 2;
		private const int _subBuild = 3;

		#region Properties

		public int Major { get { return _version[_major]; } set { _version[_major] = value; } }
		public int Minor { get { return _version[_minor]; } set { _version[_minor] = value; } }
		public int Build { get { return _version[_build]; } set { _version[_build] = value; } }
		public int SubBuild { get { return _version[_subBuild]; } set { _version[_subBuild] = value; } }
		private int[] Part { get { return _version; } }
		
		/// <summary>
		/// Convenience function for databinding
		/// </summary>
		public string Text { get { return this.ToString(); } }

		#endregion

		private Version() { }

		public static Version Parse(string text) {
			if (string.IsNullOrEmpty(text) || !text.Contains(".")) { return null; }
			Version version = new Version();
			string[] parts = text.Split(new char[] { '.' });
			for (int x = 0; x < parts.Length; x++) {
				version.Part[x] = int.Parse(parts[x]);
			}
			return version;
		}

		public override string ToString() {
			return string.Format("{0}.{1}.{2}.{3}", this.Major, this.Minor, this.Build, this.SubBuild);
		}

		public bool Equals(Version other) {
			return this.Major == other.Major && this.Minor == other.Minor
				&& this.Build == other.Build && this.SubBuild == other.SubBuild;
		}
	}
}
