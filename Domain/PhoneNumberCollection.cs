using System;
using System.Collections.Generic;

namespace Idaho {
	[Serializable()]
	public class PhoneNumberCollection : EntityCollection<PhoneNumber>,
		IEquatable<PhoneNumberCollection> {

		/// <summary>
		/// The first instance of a type of number
		/// </summary>
		public PhoneNumber this[PhoneNumber.Types type] {
			get {
				foreach (PhoneNumber p in this) { if (p.Type == type) { return p; } }
				return null;
			}
		}

		public PhoneNumber Primary {
			get {
				if (this.Count == 1) { return this[0]; } else { return null; }
			}
		}

		/// <summary>
		/// Add phone number only if good value
		/// </summary>
		public void Add(long number, PhoneNumber.Types type) {
			// number must at least be 000-1000
			if (number >= 10000) { this.Add(new PhoneNumber(number, type)); }
		}

		public bool Equals(PhoneNumberCollection other) {
			if (other == null) { return false; }
			if (this.Count == 0 && other.Count == 0) { return true; }
			if (this.Count != other.Count) { return false; }

			for (int x = 0; x < this.Count; x++) {
				if (!(this.AtIndex(x)).Equals(other.AtIndex(x))) { return false; }
			}
			return true;
		}
	}
}
