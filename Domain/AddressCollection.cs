using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	[Serializable]
	public class AddressCollection : EntityCollection<Address>,
		IEquatable<AddressCollection> {

		/// <summary>
		/// Address of given type
		/// </summary>
		public Address this[Address.Types type] {
			get { return this.Find(a => a.Type == type); }
		}

		/// <summary>
		/// Only add unique addresses
		/// </summary>
		public new bool Add(Address address) {
			foreach (Address a in this) {
				if (a.Street == address.Street) { return false; }
			}
			base.Add(address);
			return true;
		}

		public bool Equals(AddressCollection other) {
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
