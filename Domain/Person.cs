using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	[Serializable]
	public abstract class Person : Entity {
		private string _firstName = string.Empty;
		private string _lastName = string.Empty;
		private string _originalName = string.Empty;
		private string _email = string.Empty;
		private bool _requireEmail = true;
		[NoUpdate] private bool _hideEmail = false;
		[NoUpdate] private string _description = string.Empty;
		private string _jobTitle = string.Empty;
		private PhoneNumberCollection _phones;
		private AddressCollection _addresses;
		public enum NameFormat { FirstLast, LastFirst }

		#region Properties

		/// <summary>
		/// Require an e-mail address to be a valid (IsValid) user
		/// </summary>
		public bool RequireEmail {
			set { _requireEmail = value; } get { return _requireEmail; }
		}

		public AddressCollection Addresses {
			get {
				if (_addresses == null) { _addresses = new AddressCollection(); }
				return _addresses;
			}
		}
		public Address HomeAddress {
			get { return this.Addresses[Address.Types.Home]; }
		}
		public Address OfficeAddress {
			get { return this.Addresses[Address.Types.Office]; }
		}

		public PhoneNumberCollection PhoneNumbers {
			get {
				if (_phones == null) { _phones = new PhoneNumberCollection(); }
				return _phones; 
			}
		}
		public PhoneNumber OfficePhone {
			get { return this.PhoneNumbers[PhoneNumber.Types.Office]; }
		}
		public PhoneNumber FaxPhone {
			get { return this.PhoneNumbers[PhoneNumber.Types.FAX]; }
		}
		public PhoneNumber HomePhone {
			get { return this.PhoneNumbers[PhoneNumber.Types.Home]; }
		}
		public PhoneNumber MobilePhone {
			get { return this.PhoneNumbers[PhoneNumber.Types.Mobile]; }
		}

		public string FirstName { get { return _firstName; } set { _firstName = value; } }
		public string LastName { get { return _lastName; } set { _lastName = value; } }

		/// <summary>
		/// If FirstName is the "goes by" name then this will be their original name,
		/// such as "Christopher" for "Chris"
		/// </summary>
		public string OriginalName { get { return _originalName; } set { _originalName = value; } }
		
		public string Email {
			get { return _email; }
			set { if (!string.IsNullOrEmpty(value)) { _email = value.ToLower(); } }
		}
		public string EmailLink {
			get {
				if (!string.IsNullOrEmpty(_email)) {
					return string.Format("<a href=\"mailto:{0}\">{0}</a>", _email);
				}
				return string.Empty;
			}
		}
		public string Description { get { return _description; } set { _description = value; } }
		public string JobTitle { get { return _jobTitle; } set { _jobTitle = value; } }

		/// <summary>
		/// Should e-mail address be hidden from public view
		/// </summary>
		public bool HideEmail { get { return _hideEmail; } set { _hideEmail = value; } }

		/// <summary>
		/// Does user have correctly populated required fields
		/// </summary>
		public override bool IsValid {
			get {
				return (!string.IsNullOrEmpty(_firstName)) &&
					(!_requireEmail || !string.IsNullOrEmpty(_email));
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Person is incomplete when initially constructed
		/// </summary>
		public Person() { base.Status = Status.Incomplete; }
		protected Person(TimeSpan cacheDuration) : base(cacheDuration) {
			base.Status = Status.Incomplete;
		}
		/// <summary>
		/// Create a new user with an optional new ID
		/// </summary>
		protected Person(bool createID) : base(createID) { }

		/// <summary>
		/// Construct an object for an existing user with given ID
		/// </summary>
		protected Person(Guid id) : base(id) { }

		protected Person(Guid id, TimeSpan cacheDuration) : base(id, cacheDuration) {
			base.Status = Status.Incomplete;
		}

		#endregion

		#region Name

        /// <summary>
        /// Return name formatted as First Last
        /// </summary>
		public string Name {
			get { return this.FormatName(NameFormat.FirstLast); }
		}

		/// <summary>
		/// Return name formatted as specified
		/// </summary>
		public string FormatName(NameFormat format) {
			string value = string.Empty;
			switch (format) {
				case NameFormat.FirstLast:
					value = string.Format("{0} {1}", _firstName, _lastName); break;
				case NameFormat.LastFirst:
					value = string.Format("{0}, {1}", _lastName, _firstName); break;
			}
			return value.Trim();
		}

		/// <summary>
		/// Infer first and last name
		/// </summary>
		/// <remarks>If middle name given it is combined with first name</remarks>
		public static string[] ParseName(string name) {
			string[] split = name.Split(' ');
			string last = split[split.Length - 1].Trim();
			string first = name.Replace(last, "").Trim();
	   
			return new string[] {first, last};
		}

		#endregion

		public override string ToString() { return this.Name; }
	}
}
