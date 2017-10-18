using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Configuration;

namespace Idaho.Web.Controls {
	public class AddressList : Idaho.Web.Controls.SelectList {

		#region Controls

		private Field _street = new Field();
		private Field _street2 = new Field();
		private Field _city = new Field();
		private EnumList _state = new EnumList();
		private Field _province = new Field();
		private EnumList _country = new EnumList();
		private Field _zipCode = new Field();
		private Field _postalCode = new Field();

		#endregion

		private const Int16 _maxListNameLength = 20;
		private const string _newField = "fldNew";
		private const string _editField = "fldEdit";
		private bool _domestic = true;
		private Idaho.AddressCollection _addresses;
		private Idaho.Address _selected;
		// bitmask of address types (billing, shipping)
		private int _type = 0;
		// displaying new address
		private bool _new;
		private bool _forCurrentUser = true;
		private Show _show = Show.All;
		private Allow _allow = Allow.All;

		#region Enumerations

		[Flags()]
		private enum Allow {
			All = InPageSave | Delete | MultipleAddresses,
			None = 0x0,
			InPageSave = 0x1,
			Delete = 0x2,
			MultipleAddresses = 0x4
		}

		[Flags()]
		private enum Show {	All = Country, None = 0x0, Country = 0x4 }

		#endregion

		#region Properties

		public bool ForCurrentUser { set { _forCurrentUser = value; } }
		public bool SingleAddress {
			get { return !_allow.Contains(Allow.MultipleAddresses); }
			set {
				if (value) {
					this.AllowDelete = false;
					this.AllowInPageSave = false;
					this.ShowCountry = false;
					this.ShowLabel = false;
				}
				_allow = _allow.Combine<Allow>(Allow.MultipleAddresses, !value);
			}
		}
		public bool AllowDelete {
			get { return _allow.Contains(Allow.Delete); }
			set { _allow = _allow.Combine<Allow>(Allow.Delete, value); }
		}
		public bool AllowInPageSave {
			get { return _allow.Contains(Allow.InPageSave); }
			set { _allow = _allow.Combine<Allow>(Allow.InPageSave, value); }
		}
		public Idaho.Address Address {
			set {
				if (value != null) {
					_addresses = new Idaho.AddressCollection();
					_addresses.Add(value);
				}
			}
		}
		public Idaho.AddressCollection Addresses { set { _addresses = value; } }

		public bool ShowCountry {
			get { return _show.Contains(Show.Country); }
			set { _show = _show.Combine<Show>(Show.Country, value); }
		}
		public new Idaho.Address Selected { get { return _selected; } set { _selected = value; } }

		#endregion

		#region Load Post Data

		public override bool LoadPostData(string key, NameValueCollection posted) {
			bool newAddress = this.BooleanField(_newField, posted);
			bool edit = newAddress || this.BooleanField(_editField, posted);

			/*
			if (_addresses == null && _forCurrentUser) {
				_addresses = this.Page.User.Addresses;
			}
			*/
			if (_addresses != null) {
				if (this.SingleAddress) {
					_selected = _addresses[0];
				} else if (!newAddress) {
					_selected = _addresses[int.Parse(posted[key])];
				}
			}

			if (edit && _selected == null) {
				// new address
				newAddress = true;
			} else if (!(_selected == null || edit)) {
				// just needed to select address--done here
				return false;
			} else if (_selected == null && !edit) {
				// no selection and no edit--nothing to do
				return false;
			}
			// else editing an existing address

			if (newAddress)
				_selected = new Idaho.Address();
			if (this.Label == null)
				this.Label = "Address";

			// populate address object
			{
				_selected.Street = posted[_street.UniqueID];
				_selected.StreetLine2 = posted[_street2.UniqueID];
				_selected.City = posted[_city.UniqueID];
				if (this.ShowCountry) {
					_selected.Country = posted[_country.UniqueID].ToEnum<Address.Countries>();
				}
				if ((!this.ShowCountry) || _selected.IsDomestic) {
					// domestic
					if (posted[_state.UniqueID] != string.Empty) {
						_selected.State = posted[_state.UniqueID].ToEnum<Address.States>();
					}
					if (posted[_zipCode.UniqueID] != string.Empty) {
						_selected.ZipCode = int.Parse(posted[_zipCode.UniqueID]);
					}
				} else {
					// foreign
					_selected.Province = posted[_province.UniqueID];
					_selected.PostalCode = posted[_postalCode.UniqueID];
				}
			}

			if (newAddress && (_addresses != null)) {
				if (_addresses.Add(_selected)) {
					this.Page.Profile.Message = Resource.Say("Error_DuplicateAddress");
				}
			}

			return false;
		}

		/// <summary>
		/// Process boolean fields
		/// </summary>
		private bool BooleanField(string name, NameValueCollection posted) {
			return bool.Parse(posted[string.Format("{0}:{1}", this.UniqueID, name)]);
		}

		#endregion

		protected override void OnInit(EventArgs e) {
			this.Page.RegisterRequiresPostBack(this);
			// doesn't seem to be working
			this.SetupFields();
			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e) {
			//if (_addresses == null && _forCurrentUser)
			//	_addresses = this.Page.User.Addresses;
			if (_addresses == null)
				_addresses = new Idaho.AddressCollection();
			_new = (_addresses.Count == 0);
			if (_new)
				this.AllowInPageSave = false;
			if (this.Visible)
				this.SetupValidation();
			
			base.OnLoad(e);
		}

		#region Rendering

		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			if (_selected != null) {
				_street.Value = _selected.Street;
				_street2.Value = _selected.StreetLine2;
				_city.Value = _selected.City;
				_state.Selected = (int)_selected.State;
				if (_selected.ZipCode != 0)
					_zipCode.Value = _selected.ZipCode.ToString();
				if (this.ShowCountry) {
					_postalCode.Value = _selected.PostalCode;
					_province.Value = _selected.Province;
					_country.Selected = (int)_selected.Country;
				}
				_domestic = true;

			} else {
				_domestic = true;
				if (this.ShowCountry) {
					_country.Selected = (int)Idaho.Address.Countries.UnitedStatesOfAmerica;
				}
				// If _new Then _country.Selected = AMP.Address.Countries.UnitedStatesOfAmerica
			}
			if (!this.SingleAddress) {
				// label
				writer.Write("<div class=\"addressSelect\" id=\"");
				writer.Write(this.ClientID);
				writer.Write("_select\"");
				if (_new)
					writer.Write(" style=\"display: none;\"");
				writer.Write("><label");
				if (this.Required)
					writer.Write(" class=\"required\"");
				writer.Write(" for=\"");
				writer.Write(this.ClientID);
				writer.Write("\">");
				writer.Write(this.Label);
				writer.Write("</label>");
				// list
				this.RenderList(writer);
				// buttons
				if (!_new)
					this.RenderButton("Edit", "Edit", writer);
				if (this.AllowDelete)
					this.RenderButton("Delete", "Delete", writer);
				this.RenderButton("AddAddress", "Add", writer);
				// preview
				writer.Write("<div id=\"preview\"></div></div>");
			} else {
				this.RenderPostbackTrigger(writer);
			}
			// form
			this.RenderForm(writer);
		}

		private void RenderList(HtmlTextWriter writer) {
			if (_new) {
				this.HeadItem = SelectList.HeadItems.New;
			} else if ((_selected != null) || _addresses.Count == 1) {
				this.HeadItem = SelectList.HeadItems.DoNotDisplay;
			}

			Guid selectedID = Guid.Empty;
			if (_selected != null) { selectedID = _selected.ID; }

			this.RenderBeginTag(writer);
			if (!_new) {
				_addresses.Sort();
				foreach (Idaho.Address a in _addresses) {
					writer.Write("<option value=\"");
					writer.Write(a.ID);
					writer.Write("\"");
					if (a.ID.Equals(selectedID))
						writer.Write(" selected=\"selected\"");
					writer.Write(">");
					if (a.Street.Length > _maxListNameLength) {
						writer.Write(a.Street.Substring(0, _maxListNameLength - 3));
						writer.Write("...");
					} else {
						writer.Write(a.Street);
					}
					writer.Write("</option>");
				}
			}
			this.RenderEndTag(writer);
		}

		private void RenderForm(HtmlTextWriter writer) {
			// name
			writer.Write("<fieldset class=\"addressForm\" id=\"");
			writer.Write(this.ClientID);
			writer.Write("_fields\"");
			if (!(_new || this.SingleAddress)) { writer.Write(" style=\"display: none;\""); }
			writer.Write("><legend id=\"addressTitle\">");
			if (!this.SingleAddress) { writer.Write("New "); }
			writer.Write("Address</legend>");
			// country
			if (this.ShowCountry) {
				writer.Write("<label ");
				if (this.Required) { writer.Write("class=\"required\" "); }
				writer.Write("for=\"");
				writer.Write(_country.UniqueID);
				writer.Write("\">");
				writer.Write(Resource.Say("Label.Country"));
				writer.Write("</label>");
				_country.RenderControl(writer);
				writer.Write("<br/>");
			}
			// street
			_street.RenderControl(writer);
			writer.Write("<br/><label>&nbsp</label>");
			_street2.RenderControl(writer);
			writer.Write("<br/>");
			// city
			_city.RenderControl(writer);
			writer.Write("<br/>");

			// buttons
			writer.Write("<div id=\"addressActions\" class=\"actions\"");
			if (!this.AllowInPageSave) { writer.Write(" style=\"display: none\""); }
			writer.Write(">");
			this.RenderButton("Cancel", "Cancel", writer);
			this.RenderButton("Save", "Save", writer);
			writer.Write("</div>");

			// domestic state/zip
			writer.Write("<div id=\"");
			writer.Write(this.ClientID);
			writer.Write("_domestic\"");
			if (!_domestic) { writer.Write(" style=\"display: none;\""); }
			writer.Write("><label");
			if (this.Required) { writer.Write(" class=\"required\""); }
			writer.Write(" for=\"");
			writer.Write(_state.UniqueID);
			writer.Write("\">");
			writer.Write(Resource.Say("Label.StateZip"));
			writer.Write("</label>");
			_state.RenderControl(writer);
			_zipCode.RenderControl(writer);
			writer.Write("</div>");

			// foreign province/postal code
			if (this.ShowCountry) {
				writer.Write("<div id=\"");
				writer.Write(this.ClientID);
				writer.Write("_foreign\"");
				if (_domestic) { writer.Write(" style=\"display: none;\""); }
				writer.Write(">");
				_province.RenderControl(writer);
				writer.Write("<br/>");
				_postalCode.RenderControl(writer);
				writer.Write("</div>");
			}

			// modes
			this.RenderHidden("fldEdit", (_new || this.SingleAddress).ToString(), writer);
			this.RenderHidden("fldNew", (_new).ToString(), writer);

			writer.Write("</fieldset>");
		}

		private void RenderHidden(string suffix, string value, HtmlTextWriter writer) {
			writer.Write("<input type=\"hidden\" name=\"");
			writer.Write(this.UniqueID);
			writer.Write(":");
			writer.Write(suffix);
			writer.Write("\" id=\"");
			writer.Write(this.ClientID);
			writer.Write("_");
			writer.Write(suffix);
			writer.Write("\" value=\"");
			writer.Write(value);
			writer.Write("\" />");
		}

		private void RenderButton(string resx, string method, HtmlTextWriter writer) {
			Idaho.Web.Controls.Button button = new Idaho.Web.Controls.Button();
			button.Page = this.Page;
			button.ResourceKey = resx;
			button.OnClick = string.Format("Address.{0}()", method);
			button.RenderControl(writer);
		}

		#endregion

		#region Control Configuration

		private void SetupValidation() {
			if (!this.ShowCountry) {
				_country = null;
				_province = null;
				_postalCode = null;
			}

			Field[] fields = { _street, _street2, _city, _province, _zipCode, _postalCode };
			Field[] validate = { _street, _city, _province, _zipCode, _postalCode };

			// state, country and selector
			if (this.Required) {
				if ((!this.SingleAddress) && _addresses.Count > 0) {
					this.RegisterValidation(Validation.Types.Select, "Address");
				}
				Validation validState = new Validation();
				validState.Type = Validation.Types.Select;
				validState.Message = "State";
				validState.Required = true;
				validState.Control = _state;
				validState.Register(this.Page);

				if (this.ShowCountry) {
					Validation validCountry = new Validation();
					validCountry.Type = Validation.Types.Select;
					validCountry.Message = "Country";
					validCountry.Required = true;
					validCountry.Control = _country;
					validCountry.Register(this.Page);
				}
			}

			// common setup for fields
			foreach (Field f in fields) {
				if (f != null) {
					if (f.ID.EndsWith("ZipCode")) {
						f.ValidationType = Validation.Types.ZipCode;
					} else {
						f.ValidationType = Validation.Types.PlainText;
						f.ShowLabel = true;
					}
					f.Page = this.Page;
					f.Type = Field.Types.Text;
					f.OnInit();
				}
			}

			// common validation for fields
			foreach (Field f in validate) {
				if (f != null) { f.RegisterValidation(); }
			}
			this.Page.ScriptFile.Add("address");
			this.Page.ScriptFile.Add("validation/address");
			this.Page.StyleSheet.Add("address");
		}

		/// <summary>
		/// Setup controls for address
		/// </summary>
		private void SetupFields() {
			string idFormat = string.Format("{0}_{{0}}", this.ClientID);

			// street
			_street.ID = string.Format(idFormat, "fldStreet");
			_street.Resx = "StreetOrPO";
			_street.Required = this.Required;
			_street.Style.Add("width", "12em");
			_street.MaxLength = 50;
			_street2.Page = this.Page;
			_street2.ID = string.Format(idFormat, "fldStreet2");
			_street2.Type = Field.Types.Text;
			_street2.Required = false;
			_street2.MaxLength = 50;
			_street2.ShowLabel = false;
			_street2.Style.Add("width", "12em");
			_street2.OnInit();

			// city
			_city.ID = string.Format(idFormat, "fldCity");
			_city.Resx = "City";
			_city.Style.Add("width", "6em");
			_city.Required = this.Required;
			_city.MaxLength = 50;

			// state
			_state.ID = string.Format(idFormat, "fldState");
			_state.Page = this.Page;
			_state.Type = typeof(Idaho.Address.States);
			_state.IsBitmask = false;

			// zip code
			_zipCode.ID = string.Format(idFormat, "fldZipCode");
			_zipCode.Resx = "ZipCode";
			_zipCode.ShowLabel = false;
			_zipCode.MaxLength = 10;
			_zipCode.Required = this.Required;
			_zipCode.Style.Add("width", "6em");

			if (this.ShowCountry) {
				// country
				_country.ID = string.Format(idFormat, "fldCountry");
				_country.Page = this.Page;
				_country.Type = typeof(Idaho.Address.Countries);
				_country.IsBitmask = false;

				// province
				_province.ID = string.Format(idFormat, "fldProvince");
				_province.Resx = "Province";
				_province.Required = false;
				_province.Style.Add("width", "10em");
				_province.MaxLength = 50;

				// postal code
				_postalCode.ID = string.Format(idFormat, "fldPostalCode");
				_postalCode.Resx = "PostalCode";
				_postalCode.MaxLength = 20;
				_postalCode.Required = false;
				_postalCode.ValidationType = Validation.Types.PlainText;
				_postalCode.Style.Add("width", "6em");
			}
		}

		#endregion

	}
}