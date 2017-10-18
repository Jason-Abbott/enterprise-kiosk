using Idaho.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Idaho {
	/// <summary>
	/// Indicate and describe a level such as priority or status
	/// </summary>
	public class Indicator : IComparable<Indicator> {
		private int _level = 0;
		private string _name = string.Empty;
		private string _description = string.Empty;
		private string _tag = string.Empty;

		#region Properties

		public string Tag { get { return _tag; } set { _tag = value; } }

		public int Level { get { return _level; } set { _level = value; } }

		public string Name { get { return _name; } set { _name = value; } }

		public string Description { get { return _description; } set { _description = value; } }

		#endregion

		public new string ToString() { return _name; }

		#region Operators

		public static bool operator <(Indicator i1, Indicator i2) {
			return (i1.Level < i2.Level);
		}
		public static bool operator >(Indicator i1, Indicator i2) {
			return (i1.Level > i2.Level);
		}

		public static bool operator ==(Indicator i1, Indicator i2) {
			if (object.ReferenceEquals(i1, null)) { return object.ReferenceEquals(i2, null); }
			if (object.ReferenceEquals(i2, null)) { return object.ReferenceEquals(i1, null); }
			return i1.Equals(i2);
		}
		public static bool operator !=(Indicator i1, Indicator i2) {
			if (object.ReferenceEquals(i1, null)) { return !object.ReferenceEquals(i2, null); }
			if (object.ReferenceEquals(i2, null)) { return !object.ReferenceEquals(i1, null); }
			return !i1.Equals(i2);
		}

		public static bool operator <=(Indicator i1, Indicator i2) {
			return i1.Equals(i2) || i1 < i2;
		}
		public static bool operator >=(Indicator i1, Indicator i2) {
			return i1.Equals(i2) || i1 > i2;
		}

		#endregion

		#region IComparable

		public int CompareTo(Indicator other) { return _level.CompareTo(other.Level); }

		#endregion

	}
}
