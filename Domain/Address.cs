using Idaho.Attributes;
using Idaho.Properties;
using System;
using System.Data;
using System.Data.OleDb;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace Idaho {
	[Serializable]
	public class Address : Entity, ILinkable, IComparable<Address>, IEquatable<Address> {

		private string _street = string.Empty;
		private string _street2 = string.Empty;
		private string _city = string.Empty;
		private Address.States _state = States.Unknown;
		private Address.Countries _country = 0;
		private string _province = string.Empty;
		private string _postalCode = string.Empty;
		private int _zipCode = -1;
		private bool _validated = false;
		private Types _type;

		#region Enumerations

		[Flags()]
		public enum Types { Home = 0x1, Office = 0x2 }
		
		[Flags()]
		public enum For { Any = Delivery | Billing, Delivery = 0x1, Billing = 0x2 }
		public enum States {
			Unknown = 0,
			Alaska = 1,
			Alabama,
			American_Samoa,
			Arkansas,
			Arizona,
			California,
			Colorado,
			Connecticut,
			Deleware,
			Federated_States_of_Micronesia,
			Florida,
			Georgia,
			Guam,
			Hawaii,
			Idaho,
			Iowa,
			Illinois,
			Indiana,
			Kansas,
			Kentucky,
			Louisiana,
			Maine,
			Marshall_Islands,
			Maryland,
			Massachusetts,
			Michigan,
			Minnesota,
			Mississippi,
			Missouri,
			Montana,
			North_Carolina,
			North_Dakota,
			Northern_Mariana_Islands,
			Nebraska,
			Nevada,
			New_Hampshire,
			New_Jersey,
			New_Mexico,
			New_York,
			Ohio,
			Oklahoma,
			Oregon,
			Palau,
			Pennsylvania,
			Puerto_Rico,
			Rhode_Island,
			South_Carolina,
			South_Dakota,
			Tennessee,
			Texas,
			Utah,
			Virginia,
			Vermont,
			Virgin_Islands,
			Washington,
			Wisconsin,
			West_Virginia,
			Wyoming,
			Washington_DC
		}

		public enum Countries {
			Unknown = 0,
			Afghanistan = 1,
			Albania,
			Algeria,
			AmericanSamoa,
			Andorra,
			Angola,
			Anguilla,
			Antigua_and_Barbuda,
			Argentina,
			Armenia,
			Aruba,
			Australia,
			Austria,
			Azerbaijan,
			Azores,
			Bahamas,
			Bahrain,
			Bangladesh,
			Barbados,
			Belarus,
			Belgium,
			Belize,
			Benin,
			Bermuda,
			Bhutan,
			Bolivia,
			Bonaire,
			BosniaHerzegovina,
			Botswana,
			Brazil,
			BritishIndianOceanTerritory,
			Brunei,
			Bulgaria,
			BurkinaFaso,
			Burundi,
			Cambodia,
			Cameroon,
			Canada,
			CanaryIslands,
			CapeVerde,
			CaymanIslands,
			CentralAfricanRepublic,
			Chad,
			ChannelIslands,
			Chile,
			China,
			ChristmasIsland,
			CocosIsland,
			Columbia,
			Comoros,
			Congo,
			CookIslands,
			CostaRica,
			CoteDIvoire,
			Croatia,
			Cuba,
			Curacao,
			Cyprus,
			CzechRepublic,
			Denmark,
			Djibouti,
			Dominica,
			DominicanRepublic,
			EastTimor,
			Ecuador,
			Egypt,
			ElSalvador,
			EquatorialGuinea,
			Eritrea,
			Estonia,
			Ethiopia,
			FalklandIslands,
			FaroeIslands,
			Fiji,
			Finland,
			France,
			FrenchGuiana,
			FrenchPolynesia,
			FrenchSouthernTerritory,
			Gabon,
			Gambia,
			Georgia,
			Germany,
			Ghana,
			Gibraltar,
			GreatBritain,
			Greece,
			Greenland,
			Grenada,
			Guadeloupe,
			Guam,
			Guatemala,
			Guinea,
			Guyana,
			Haiti,
			Hawaii,
			Honduras,
			HongKong,
			Hungary,
			Iceland,
			India,
			Indonesia,
			Iran,
			Iraq,
			Ireland,
			IsleOfMan,
			Israel,
			Italy,
			Jamaica,
			Japan,
			Jordan,
			Kazakhstan,
			Kenya,
			Kiribati,
			KoreaNorth,
			KoreaSouth,
			Kuwait,
			Kyrgyzstan,
			Laos,
			Latvia,
			Lebanon,
			Lesotho,
			Liberia,
			Libya,
			Liechtenstein,
			Lithuania,
			Luxembourg,
			Macau,
			Macedonia,
			Madagascar,
			Malaysia,
			Malawi,
			Maldives,
			Mali,
			Malta,
			MarshallIslands,
			Martinique,
			Mauritania,
			Mauritius,
			Mayotte,
			Mexico,
			MidwayIslands,
			Moldova,
			Monaco,
			Mongolia,
			Montserrat,
			Morocco,
			Mozambique,
			Myanmar,
			Nambia,
			Nauru,
			Nepal,
			NetherlandAntilles,
			Netherlands,
			Nevis,
			NewCaledonia,
			NewZealand,
			Nicaragua,
			Niger,
			Nigeria,
			Niue,
			NorfolkIsland,
			Norway,
			Oman,
			Pakistan,
			PalauIsland,
			Palestine,
			Panama,
			PapuaNewGuinea,
			Paraguay,
			Peru,
			Philippines,
			PitcairnIsland,
			Poland,
			Portugal,
			PuertoRico,
			Qatar,
			Reunion,
			Romania,
			Russia,
			Rwanda,
			SaintBarthelemy,
			SaintEustatius,
			SaintHelena,
			SaintKittsNevis,
			SaintLucia,
			SaintMaarten,
			SaintPierre_and_Miquelon,
			SaintVincent_and_Grenadines,
			Saipan,
			Samoa,
			SamoaAmerican,
			SanMarino,
			SaoTome_and_Principe,
			SaudiArabia,
			Senegal,
			Seychelles,
			Serbia_and_Montenegro,
			SierraLeone,
			Singapore,
			Slovakia,
			Slovenia,
			SolomonIslands,
			Somalia,
			SouthAfrica,
			Spain,
			SriLanka,
			Sudan,
			Suriname,
			Swaziland,
			Sweden,
			Switzerland,
			Syria,
			Tahiti,
			Taiwan,
			Tajikistan,
			Tanzania,
			Thailand,
			Togo,
			Tokelau,
			Tonga,
			Trinidad_and_Tobago,
			Tunisia,
			Turkey,
			Turkmenistan,
			Turks_and_CaicosIsland,
			Tuvalu,
			Uganda,
			Ukraine,
			UnitedArabEmirates,
			UnitedStatesOfAmerica,
			Uruguay,
			Uzbekistan,
			Vanuatu,
			VaticanCityState,
			Venezuela,
			Vietnam,
			VirginIslandsBrittish,
			VirginIslands_USA,
			WakeIsland,
			Wallis_and_FutanaIsland,
			Yemen,
			Zaire,
			Zambia,
			Zimbabwe
		}

		#endregion

		#region Properties

		public Types Type { get { return _type; } set { _type = value; } }

		public string PostalCode {
			get { return _postalCode; }
			set { _postalCode = value.SafeForWeb(15); }
		}
		public string Province {
			get { return _province; }
			set { _province = value.SafeForWeb(50); }
		}
		public bool Validated {
			get { return _validated; }
			set { _validated = value; }
		}
		public Address.States State {
			get { return _state; }
			set { _state = value; }
		}
		public string StateAbbreviation {
			get {
				switch (_state) {
					case States.Alabama: return "AL";
					case States.Alaska: return "AK";
					case States.American_Samoa: return "AS";
					case States.Arizona: return "AZ";
					case States.Arkansas: return "AR";
					case States.California: return "CA";
					case States.Colorado: return "CO";
					case States.Connecticut: return "CT";
					case States.Deleware: return "DE";
					case States.Washington_DC: return "DC";
					case States.Federated_States_of_Micronesia: return "FM";
					case States.Florida: return "FL";
					case States.Georgia: return "GA";
					case States.Guam: return "GU";
					case States.Hawaii: return "HI";
					case States.Idaho: return "ID";
					case States.Illinois: return "IL";
					case States.Indiana: return "IN";
					case States.Iowa: return "IA";
					case States.Kansas: return "KS";
					case States.Kentucky: return "KY";
					case States.Louisiana: return "LA";
					case States.Maine: return "ME";
					case States.Marshall_Islands: return "MH";
					case States.Maryland: return "MD";
					case States.Massachusetts: return "MA";
					case States.Michigan: return "MI";
					case States.Minnesota: return "MN";
					case States.Mississippi: return "MS";
					case States.Missouri: return "MO";
					case States.Montana: return "MT";
					case States.Nebraska: return "NE";
					case States.Nevada: return "NV";
					case States.New_Hampshire: return "NH";
					case States.New_Jersey: return "NJ";
					case States.New_Mexico: return "NM";
					case States.New_York: return "NY";
					case States.North_Carolina: return "NC";
					case States.North_Dakota: return "ND";
					case States.Northern_Mariana_Islands: return "MP";
					case States.Ohio: return "OH";
					case States.Oklahoma: return "OK";
					case States.Oregon: return "OR";
					case States.Palau: return "PW";
					case States.Pennsylvania: return "PA";
					case States.Puerto_Rico: return "PR";
					case States.Rhode_Island: return "RI";
					case States.South_Carolina: return "SC";
					case States.South_Dakota: return "SD";
					case States.Tennessee: return "TN";
					case States.Texas: return "TX";
					case States.Utah: return "UT";
					case States.Vermont: return "VT";
					case States.Virgin_Islands: return "VI";
					case States.Virginia: return "VA";
					case States.Washington: return "WA";
					case States.West_Virginia: return "WV";
					case States.Wisconsin: return "WI";
					case States.Wyoming: return "WY";
					default: return string.Empty;
				}
			}
		}

		public Idaho.Address.Countries Country {
			get { return _country; }
			set { _country = value; }
		}
		public int ZipCode {
			get { return _zipCode; }
			set {
				Assert.Range(value, 0, 999999999, "InvalidZipCode");
				_zipCode = value;
			}
		}
		public string City {
			get { return _city; }
			set { _city = value.SafeForWeb(100); }
		}
		public string Street {
			get { return _street; }
			set { _street = value.SafeForWeb(150); }
		}
		public string StreetLine2 {
			get { return (_street2 == null) ? string.Empty : _street2; }
			set { _street2 = value.SafeForWeb(150); }
		}
		public override bool IsValid {
			get {
				return _state != States.Unknown && !string.IsNullOrEmpty(_street)
					&& !string.IsNullOrEmpty(_city) && _zipCode > -1;
			}
		}

		#endregion

		public Address(Guid id) { base.ID = id; }
		public Address(Types type) { _type = type; }
		public Address() { }

		#region ORM

		public bool Save() {
			Data.Jet db = new Data.Jet(); //ConfigurationManager.AppSettings["UserStore"]);

			//.Command.CommandText = IIf(Me.Persisted, "updateAddress", "createAddress")
			db.ProcedureName = "createAddress";
			db.Parameters.Add("@id", this.ID, OleDbType.Guid);
			db.Parameters.Add("@street1", _street, OleDbType.VarChar);
			db.Parameters.Add("@street2", _street2, OleDbType.VarChar);
			db.Parameters.Add("@stateID", (Int32)_state, OleDbType.Integer);
			db.Parameters.Add("@countryID", (Int32)_country, OleDbType.Integer);
			db.Parameters.Add("@province", _province, OleDbType.VarChar);
			db.Parameters.Add("@postalCode", _postalCode, OleDbType.VarChar);
			db.Parameters.Add("@zipCode", _zipCode, OleDbType.Integer);
			db.Parameters.Add("@validated", _validated, OleDbType.Boolean);

			try { db.ExecuteOnly(); }
			catch { db.Finish(true); return false; }

			return true;
		}

		#endregion

		#region ILinkable

		public string DetailUrl {
			get {
				return Resource.SayFormat("URL_GoogleMap",
					Regex.Replace(_street, "\\s*#\\s*\\d+", ""), _zipCode);
			}
		}
		public string DetailLink {
			get {
				if (this.IsDomestic) {
					return string.Format("<a href=\"{0}\" title=\"Map Address\" target=\"_blank\">{1}</a>",
						this.DetailUrl, this.ToHtml());
				} else {
					return this.ToHtml();
				}
			}
		}

		#endregion

		/// <summary>
		/// Is shipping region domestic
		/// </summary>
		public bool IsDomestic {
			get { return (_country == Countries.UnitedStatesOfAmerica); }
		}
		public bool CanadaOrMexico {
			get { return _country == Countries.Canada || _country == Countries.Mexico; }
		}

		/// <summary>
		/// Build formatted address
		/// </summary>
		public override string ToString() { return this.ToString(Environment.NewLine); }
		public string ToHtml() { return this.ToString("<br/>"); }
		private string ToString(string newLine) {
			StringBuilder text = new StringBuilder();
			text.AppendFormat("{0}{1}", _street, newLine);
			if (!string.IsNullOrEmpty(_street2)) { text.AppendFormat("{0}{1}", _street2, newLine); }
			if (this.IsDomestic) {
				text.AppendFormat("{0}, {1} ", _city,
					_state.ToString().FixSpacing());
				if (_zipCode > 99999) {
					string zip = _zipCode.ToString();
					text.AppendFormat("{0}-{1}", zip.Substring(0, 5), zip.Substring(5, 4));
				} else {
					text.Append(_zipCode);
				}
			} else {
				text.AppendFormat("{0}, {1} {2}{3}{4}", _city, _province, _postalCode,
					newLine, _country.ToString().FixSpacing());
			}
			return text.ToString();
		}
		public bool Empty() {
			return (_street == null && _city == null && _state == 0 && _country == 0 && _zipCode == 0);
		}
		int IComparable<Address>.CompareTo(Address other) {
			return string.Compare(_street, other.Street);
		}
		public bool Equals(Address other) {
			return (_street == other.Street &&
				_street2 == other.StreetLine2 &&
				_city == other.City &&
				_state == other.State);
		}
		
		public static int ParseZipCode(string value) {
			if (string.IsNullOrEmpty(value)) { return -1; }
			value = value.NumbersOnly();
			if (value.Length > 5) { value = value.Substring(0, 4); }
			return int.Parse(value);
		}

		/// <summary>
		/// Find state enumerations for string
		/// </summary>
		/// <remarks>http://www.usps.com/ncsc/lookups/abbr_state.txt</remarks>
		public static States ParseState(string value) {
			if (string.IsNullOrEmpty(value)) { return States.Unknown; }
			value = value.ToLower().Replace("_", string.Empty);
			value = value.Replace(" ", string.Empty);
			// Hawaii sometimes spelled Hawai'i
			value = value.Replace("'", string.Empty);

			switch (value) {
				case "al":
				case "alabama": return States.Alabama;

				case "ak":
				case "alaska": return States.Alaska;

				case "as":
				case "americansamoa": return States.American_Samoa;

				case "az": 
				case "arizona": return States.Arizona;

				case "ar":
				case "arkansas": return States.Arkansas;

				case "ca":
				case "california": return States.California;

				case "co":
				case "colorado": return States.Colorado;

				case "ct":
				case "connecticut": return States.Connecticut;

				case "de":
				case "deleware": return States.Deleware;

				case "dc":
				case "districtofcolumbia": return States.Washington_DC;

				case "fm":
				case "federatedstatesofmicronesia": return States.Federated_States_of_Micronesia;

				case "fl":
				case "florida": return States.Florida;

				case "ga":
				case "georgia": return States.Georgia;

				case "gu":
				case "guam": return States.Guam;

				case "hi":
				case "hawaii": return States.Hawaii;

				case "id":
				case "idaho": return States.Idaho;

				case "il":
				case "illinois": return States.Illinois;

				case "in":
				case "indiana": return States.Indiana;

				case "ia":
				case "iowa": return States.Iowa;

				case "ks":
				case "kansas": return States.Kansas;

				case "ky":
				case "kentucky": return States.Kentucky;

				case "la":
				case "louisiana": return States.Louisiana;

				case "me":
				case "maine": return States.Maine;

				case "mh":
				case "marshalislands":
				case "marshallislands": return States.Marshall_Islands;

				case "md":
				case "maryland": return States.Maryland;

				case "ma":
				case "massachusets":
				case "masachusetts":
				case "massachusetts": return States.Massachusetts;

				case "mi":
				case "michigan": return States.Michigan;

				case "mn":
				case "minnesota": return States.Minnesota;

				case "ms":
				case "mississippi": return States.Mississippi;

				case "mo":
				case "missouri": return States.Missouri;

				case "mt":
				case "montana": return States.Montana;

				case "ne":
				case "nebraska": return States.Nebraska;

				case "nv":
				case "nevada": return States.Nevada;

				case "nh":
				case "newhampshire": return States.New_Hampshire;

				case "nj":
				case "newjersey": return States.New_Jersey;

				case "nm":
				case "newmexico": return States.New_Mexico;

				case "ny":
				case "newyork": return States.New_York;

				case "nc":
				case "northcarolina": return States.North_Carolina;

				case "nd":
				case "northdakota": return States.North_Dakota;

				case "mp":
				case "northernmarianaislands": return States.Northern_Mariana_Islands;

				case "oh":
				case "ohio": return States.Ohio;

				case "ok":
				case "oklahoma": return States.Oklahoma;

				case "or":
				case "oregon": return States.Oregon;

				case "pw":
				case "palua":
				case "palau": return States.Palau;

				case "pa":
				case "pennsylvania": return States.Pennsylvania;

				case "pr":
				case "puertorico": return States.Puerto_Rico;

				case "ri":
				case "rhodeisland": return States.Rhode_Island;

				case "sc":
				case "southcarolina": return States.South_Carolina;

				case "sd":
				case "southdakota": return States.South_Dakota;

				case "tn":
				case "tennessee": return States.Tennessee;

				case "tx":
				case "texas": return States.Texas;

				case "ut":
				case "utah": return States.Utah;

				case "vt":
				case "vermont": return States.Vermont;

				case "vi":
				case "virginislands": return States.Virgin_Islands;

				case "va":
				case "virginia": return States.Virginia;

				case "wa":
				case "washington": return States.Washington;

				case "wv":
				case "westvirginia": return States.West_Virginia;

				case "wi":
				case "wisconsin": return States.Wisconsin;

				case "wy":
				case "wyoming": return States.Wyoming;
			}
			return States.Unknown;
		}

		public static Countries ParseCountry(string value) {
			if (string.IsNullOrEmpty(value)) { return Countries.Unknown; }
			value = value.ToLower().Replace("_", string.Empty);
			value = value.Replace(" ", string.Empty);
			value = value.Replace(".", string.Empty);

			switch (value) {
				case "us":
				case "unitedstates":
				case "unitedstatesofamerica":
					return Countries.UnitedStatesOfAmerica;
				default:
					return (Countries)Enum.Parse(typeof(Countries), value);
			}
		}
	}
}