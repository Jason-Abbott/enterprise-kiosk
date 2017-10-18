using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace Idaho.Network {
	/// <summary>
	/// Represent an IP address
	/// </summary>
	/// <remarks>
	/// The System.Net.IPAddress has Win32 references that prevent it
	/// from being serialized.
	/// </remarks>
	[Serializable()]
	public class IpAddress : IComparable<IpAddress>, ILinkable, IEquatable<IpAddress>  {

		private static IpAddress _host = null;
		private byte[] _octets = new byte[4];
		private const int _end = 3;
	   
		public byte[] Octets {
			get { return _octets; }
			set {
				Assert.Range(value, 4, "InvalidIpBytes");
				_octets = value;
			}
		}
	   
		#region Constructors

		/// <summary>
		/// Construct with int
		/// </summary>
		public IpAddress(int address) {
			Assert.Range(address, 0, 2147483647, "InvalidIpInteger");
			_octets[0] = (byte)(address & 0xFF);
			_octets[1] = (byte)((address >> 8) & 0xFF);
			_octets[2] = (byte)((address >> 16) & 0xFF);
			_octets[3] = (byte)((address >> 24) & 0xFF);
		}

		/// <summary>
		/// Construct with byte array
		/// </summary>
		public IpAddress(byte[] address) {
			Assert.Range(address, 4, "InvalidIpBytes");
			_octets = address;
		}
	   
		public IpAddress() {
			// accessible by reflection only
		}
	   
		#endregion

		#region ILinkable

		public string DetailUrl {
			get { return Resource.SayFormat("URL_LookupIp", this.ToString()); }
		}
		public string DetailLink {
			get {
				return string.Format("<a href=\"{0}\" title=\"Trace Address\" target=\"_blank\">{1}</a>",
					this.DetailUrl, this.ToString());
			}
		}

		#endregion

		/// <summary>
		/// The IP address of the client from HttpContext
		/// </summary>
		public static IpAddress Client {
			get {
				if (HttpContext.Current != null) {
					// no context for background threads
					return IpAddress.Parse(HttpContext.Current.Request.UserHostAddress);
				} else { return null; }
			}
		}

		/// <summary>
		/// The IP address of the host machine
		/// </summary>
		public static IpAddress Host {
			get {
				if (_host == null) {
					string name = System.Net.Dns.GetHostName();
					IPHostEntry e = System.Net.Dns.GetHostEntry(name);
					foreach (System.Net.IPAddress ip in e.AddressList) {
						if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
							_host = IpAddress.Parse(ip.ToString());
							break;
						}
					}
				}
				return _host;
			}
		}

		/// <summary>
		/// Determine address equality
		/// </summary>
		public bool Equals(IpAddress other) {
			for (int x = 0; x <= _end; x++) {
				if (other.Octets[x] != _octets[x]) { return false; }
			}
			return true;
		}

		/// <summary>
		/// Convert string to IP address object
		/// </summary>
		public static IpAddress Parse(string ipString) {
			Assert.NoNull(ipString, "NullIpString");
			if (ipString == "::1") { return Parse("127.0.0.1"); }
			string[] address = ipString.Split('.');
			int octet;
			byte[] octets = new byte[4];

			Assert.Range(address, 4, "InvalidIpBytes");

			for (int x = 0; x <= _end; x++) {
				octet = int.Parse(address[x]);
				if (octet < 0 || octet > 0xFF) {
					throw new ArgumentException("Bad IP format (" + ipString + ")");
				}
				octets[x] = (byte)octet;
			}
			return new IpAddress(octets);
		}

		/// <summary>
		/// Convert address octets to int
		/// </summary>
		public int ToInt32() { return BitConverter.ToInt32(_octets, 0); }
	   
		/// <summary>
		/// Determine if current address is local (127.0.0.1)
		/// </summary>
		public bool IsLocal {
			get {
				byte[] octet = { 127, 0, 0, 1 };
				return this.Equals(new IpAddress(octet));
			}
		}

		/// <summary>
		/// Convert address to string
		/// </summary>
		public override string ToString() {
			string[] address = new string[7];
			address[0] = _octets[0].ToString();
			address[1] = ".";
			address[2] = _octets[1].ToString();
			address[3] = ".";
			address[4] = _octets[2].ToString();
			address[5] = ".";
			address[6] = _octets[_end].ToString();
			return string.Concat(address);
		}

		public int CompareTo(IpAddress other) {
			return this.ToInt32().CompareTo(other.ToInt32());
		}
	}
}
