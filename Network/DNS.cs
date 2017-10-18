using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Management;
using System.Text;
using System.Threading;

namespace Idaho.Network {
	public sealed class DNS {
		private static string _nameServer = string.Empty;
		private static Dictionary<string, MxRecord> _mxRecords = new Dictionary<string, MxRecord>();
		private const string _networkWMI = "Win32_NetworkAdapterConfiguration";
		private const string _dnsWmiProperty = "DNSServerSearchOrder";

		private static string NameServer {
			get {
				if (_nameServer == string.Empty) {
					List<string> server = DNS.GetNameServers();
					// return only the first choice
					_nameServer = server[0];
				}
				return _nameServer;
			}
		}

		public static List<MxRecord> MxQuery(string hostName) {
			List<MxRecord> records;
			if (!_mxRecords.ContainsKey(hostName)) {
				records = DNS.MxQuery(hostName, DNS.NameServer);
				_mxRecords.Add(hostName, records[0]);
			}
			records = new List<MxRecord>();
			records.Add(_mxRecords[hostName]);
			return records;
		}

		public static List<MxRecord> MxQuery(string hostName, string nameServer) {
			UdpClient client = new UdpClient(nameServer, (int)Port.Dns);
			byte[] data = DNS.CreateQuery(hostName);
			IPEndPoint ip = null;

			client.Send(data, data.Length);
			data = client.Receive(ref ip);
			return GetResponse(data, data.Length);
		}

		private static List<string> GetNameServers() {
			List<string> servers = new List<string>();
			ManagementObjectCollection.ManagementObjectEnumerator enumerator = 
				new ManagementClass(_networkWMI).GetInstances().GetEnumerator();

			while (enumerator.MoveNext()) {
				ManagementObject manage = (ManagementObject)enumerator.Current;
				string[] dns = (string[])manage[_dnsWmiProperty];
				if (dns != null) {
					for (int x = 0; x <= dns.Length - 1; x++) {	servers.Add(dns[x]); }
				}
			}
			return servers;
		}

		#region Packet parsing

		// from comments at
		// http://www.codeproject.com/aspnet/emailvalidator.asp?df=100&forumid=10083&fr=26
		protected static byte[] CreateQuery(string hostName) {
			byte[] data = new byte[512];
			int pos = 12;
			// data position
			int id = DateTime.Now.Millisecond * 60;

			data[0] = (byte)(id >> 8);
			data[1] = (byte)(id & 0xFF);
			data[2] = (byte)1;
			data[3] = (byte)0;
			data[4] = (byte)0;
			data[5] = (byte)1;
			data[6] = (byte)0;
			data[7] = (byte)0;
			data[8] = (byte)0;
			data[9] = (byte)0;
			data[10] = (byte)0;
			data[11] = (byte)0;

			// part of message with host name
			string[] tokens = hostName.Split('.');
			string token = string.Empty;
			for (int x = 0; x < tokens.Length; x++) {
				token = tokens[x];
				data[pos++] = (byte)(token.Length & 0xFF);
				byte[] buffer = Encoding.ASCII.GetBytes(token);
				buffer.CopyTo(data, pos);
				pos += token.Length;
			}
			data[pos++] = (byte)0;
			data[pos++] = (byte)0;
			data[pos++] = (byte)15;
			data[pos++] = (byte)0;
			data[pos++] = (byte)1;

			// pad remainder of packet
			for (int x = pos; x < data.Length; x++) { data[x] = 0; }
			return data;
		}

		protected static List<MxRecord> GetResponse(byte[] data, int length) {
			List<MxRecord> records = new List<MxRecord>();
			string name = string.Empty;
			int pos = 12;
			int qCount = ((data[4] & 0xFF) << 8) | (data[5] & 0xFF);
			int aCount = ((data[6] & 0xFF) << 8) | (data[7] & 0xFF);

			if (qCount < 0) { throw new InvalidOperationException("Invalid question count"); }
			if (aCount < 0) { throw new InvalidOperationException("Invalid answer count"); }

			for (int x = 0; x < qCount; ++x) {
				name = string.Empty;
				pos = Process(pos, length, ref data, ref name);
				pos += 4;
			}

			for (int x = 0; x < aCount; ++x) {
				name = string.Empty;
				pos = Process(pos, length, ref data, ref name);
				pos += 10;
				int pref = (data[pos++] << 8) | (data[pos++] & 0xFF);
				name = string.Empty;
				pos = Process(pos, length, ref data, ref name);
				records.Add(new MxRecord(pref, name));
			}
			return records;
		}

		protected static int Process(int pos, int length, ref byte[] data, ref string name) {
			int len = (data[pos++] & 0xFF);
			int offset;

			if (len == 0) { return pos; }

			do {
				if ((len & 0xC0) == 0xC0) {
					if (pos >= length) { return -1; }
					offset = ((len & 0x3F) << 8) | (data[pos] & 0xFF);
					pos++;
					Process(offset, length, ref data, ref name);
					return pos;
				} else {
					if ((pos + len) > length) { return -1; }
					name += Encoding.ASCII.GetString(data, pos, len);
					pos += len;
				}
				if (pos > length) { return -1; }
				len = data[pos++] & 0xFF;
				if (len != 0) { name += "."; }
			}
			while (len != 0);
			return pos;
		}

		#endregion

	}
}