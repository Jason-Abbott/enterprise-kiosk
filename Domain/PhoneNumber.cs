using System;

namespace Idaho {
	[Serializable]
	public class PhoneNumber : Entity, ILinkable, IEquatable<PhoneNumber>, IComparable<PhoneNumber> {

		private short _countryCode = 1;
		private short _areaCode;
		private static short _defaultAreaCode = 0;
		private int _number;
		private short _extension = -1;
		private string _description = string.Empty;
		private Types _type;

		[Flags()]
		public enum Types { Home = 0x1, Mobile = 0x2, Office = 0x4, FAX = 0x8 }

		#region Properties

		public Types Type { get { return _type; } set { _type = value; } }
		public static short DefaultAreaCode { set { _defaultAreaCode = value; } }
		public short CountryCode { get { return _countryCode; } set { _countryCode = value; } }
		public short AreaCode { get { return _areaCode; } set { _areaCode = value; } }
		public int Number { get { return _number; } set { _number = value; } }
		public short Extension { get { return _extension; } set { _extension = value; } }
		public string Description { get { return _description; } set { _description = value; } }

		/// <summary>
		/// Number expressed as an Int64
		/// </summary>
		public long FullNumber {
			get {
				string number = string.Format("{0:000}{1:0000000}", _areaCode, _number);
				if (_extension > 0) { number = string.Format("{0}{1:0000}", number, _extension); }
				return long.Parse(number);
			}
		}

		#endregion

		public PhoneNumber(long number, Types type) : this(number) { _type = type; }
		public PhoneNumber(long number) {
			this.Import(PhoneNumber.Parse(number.ToString()));
		}
		private PhoneNumber() { }

		#region ILinkable

		/// <summary>
		/// URL for reverse lookup in the U.S.
		/// </summary>
		/// <returns></returns>
		public string DetailUrl {
			get { return Resource.SayFormat("URL_LookupPhoneNumber", _areaCode, _number); }
		}
		public string DetailLink {
			get {
				if (_countryCode == 1) {
					return string.Format("<a href=\"{0}\" title=\"Trace Phone Number\" target=\"_blank\">{1}</a>",
						this.DetailUrl, this.ToString());
				} else {
					return this.ToString(false);
				}
			}
		}

		#endregion
		
		public string ToString(bool includeCountry) {
			string result = string.Empty;

			if (includeCountry) {
				result = String.Format("{0} ({1:000}) {2:000-0000}", _countryCode, _areaCode, _number);
			} else {
				result = String.Format("({0:000}) {1:000-0000}", _areaCode, _number);
			}
			if (_extension > 0) {
				result = string.Format("{0}x{1:0000}", result, _extension);
			}
			return result;
		}
		public override string ToString() { return this.ToString(false); }

		public static PhoneNumber Parse(string raw) {
			if (string.IsNullOrEmpty(raw)) { return null; }
			PhoneNumber phone = new PhoneNumber();
			raw = raw.NumbersOnly();

			if (raw.Length > 11) {
				// assume ###-###-####x###
				phone.AreaCode = short.Parse(raw.Substring(0, 3));
				phone.Number = int.Parse(raw.Substring(3, 4));
				phone.Extension = short.Parse(raw.Substring(7, raw.Length - 10));
			} else if (raw.Length == 11) {
				// assume #-###-###-####
				phone.CountryCode = short.Parse(raw.Substring(0, raw.Length - 10));
                phone.AreaCode = short.Parse(raw.Substring(raw.Length - 10, 3));
                phone.Number = int.Parse(raw.Substring(raw.Length - 7, 7));
			} else if (raw.Length >= 8) {
				// assume ###-#### with junk to ignore
				int index = raw.Length - 7;
				phone.AreaCode = short.Parse(raw.Substring(0, index));
                phone.Number = int.Parse(raw.Substring(index, 7));
			} else if (raw.Length == 7) {
				// assume ###-####
				phone.AreaCode = _defaultAreaCode;
				phone.Number = int.Parse(raw);
			} else {
				return null;
			}
			return phone;
		}
		public static PhoneNumber Parse(string raw, Types type) {
			PhoneNumber phone = Parse(raw);
			if (phone != null) { phone.Type = type; }
			return phone;
		}

		public bool Equals(PhoneNumber other) {
			return (_number == other.Number &&
				_areaCode == other.AreaCode &&
				_extension == other.Extension);
		}

		#region IComparable

		public int CompareTo(PhoneNumber other) {
			return this.FullNumber.CompareTo(other.FullNumber);
		}

		#endregion
	}
}
