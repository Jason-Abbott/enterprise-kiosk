using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Idaho.Network {
	public enum Port { Dns = 53, Smtp = 25 }
	public class SMTP {

		private Stack<string> _stack = new Stack<string>();
		private System.Net.Sockets.NetworkStream _stream;
		private System.IO.StreamReader _reader;
		private bool _testDomain = false;
		private const string _random = "hvf8zrx02qssa";
		private const string _pattern = "^[a-zA-Z][\\w\\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\\w\\.-]*[a-zA-Z0-9]\\.[a-zA-Z][a-zA-Z\\.]*[a-zA-Z]$";

		#region Static Fields

		private static bool _port25blocked = false;
		/// <remarks>
		/// If port 25 is blocked at the application host then SMTP
		/// communications are prevented. One alternative is to communicate
		/// via webservice to a different host with an open port 25.
		/// </remarks>
		public static bool Port25Blocked { set { _port25blocked = value; } }

		#endregion

		#region Documentation

		// SMTP commands
		// http://www.ietf.org/rfc/rfc0821.txt
		// http://www.gwolf.cx/seguridad/wrap/fpaper/node23.html

		// SMTP validation
		// http://www.codeproject.com/aspnet/emailvalidator.asp
		// http://www.componentspace.com/emailchecker.net.aspx
		// http://www.codeproject.com/aspnet/Valid_Email_Addresses.asp
		// http://www.eggheadcafe.com/articles/20030316.asp

		// Compare
		// http://www.patnorsoft.com/OnlineSamples/EmailValNet/Index.aspx $375
		// http://www.aspnetmx.com/demo.aspx

		// Proxy issues
		// http://support.microsoft.com/default.aspx?scid=kb;EN-US;307220
		// http://weblogs.asp.net/jan/archive/2004/01/28/63771.aspx
		// http://blogs.msdn.com/gzunino/archive/2004/09/05/225881.aspx
		// http://support.microsoft.com/kb/318140/EN-US/

		#endregion

		#region Enumerations

		protected enum Response {
			ConnectSuccess = 220,
			GenericSuccess = 250,
			DataSuccess = 354,
			QuitSuccess = 221
		}
		[Flags()]
		public enum Result {
			MxComplete = AddressNotFound | ValidatedBadAddress | Succeeded,
			Failed = DomainNotResolved | AddressNotFound | BadAddressFormat,
			Unknown = NoMxRecords | ValidatedBadAddress | NoServerGreeting | RefusedMailFrom | ProgramError,
			Succeeded = 0x1,
			NoMxRecords = 0x2,
			DomainNotResolved = 0x4,
			ValidatedBadAddress = 0x8,
			AddressNotFound = 0x10,
			NoServerGreeting = 0x20,
			RefusedMailFrom = 0x40,
			BadAddressFormat = 0x80,
			ProgramError = 0x100
		}

		#endregion

		public Stack<string> Stack { get { return _stack; } set { _stack = value; } }

		/// <summary>
		/// Validate an e-mail address by checking the mail server
		/// </summary>
		/// <param name="from"></param>
		/// <param name="recipient"></param>
		/// <returns></returns>
		public Stack<string> Ping(string from, string recipient) {
			if (!Regex.IsMatch(recipient, _pattern)) {
				_stack.Push(Result.BadAddressFormat.ToString());
				return _stack;
			}

			if (_port25blocked) {
				// execute through web service
				/*com.vasst.SMTP smtp = new com.vasst.SMTP();
				//Dim smtp As New com.webott.Utility
				object[] response = smtp.Ping(recipient);
				_stack.Clear();
				for (int x = response.Length - 1; x >= 0; x += -1) {
					_stack.Push(response(x));
				}
				 */
			} else {
				// execute locally
				Result status = Result.Failed;
				string domain = this.HostPart(recipient);

				if (_testDomain) {
					try {
						IPHostEntry host = System.Net.Dns.GetHostEntry(domain);
					} catch {
						_stack.Push(Result.DomainNotResolved.ToString());
						return _stack;
					}
				}

				List<MxRecord> mxRecords = DNS.MxQuery(domain);

				if (mxRecords.Count == 0) {
					// if no records found then try common mail server name
					status = Result.NoMxRecords;
					mxRecords.Add(MxRecord.Infer(domain));
				} else {
					mxRecords.Sort();
				}

				for (int x = 0; x <= mxRecords.Count - 1; x++) {
					status = this.WillAccept(mxRecords[x].HostName, from, recipient);
					if (status.Contains(Result.MxComplete)) { break; } 
				}
				_stack.Push(status.ToString());
			}
			return _stack;
		}

		/// <summary>
		/// Communicate with SMTP server
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="from"></param>
		/// <param name="recipient"></param>
		/// <returns></returns>
		private Result WillAccept(string domain, string from, string recipient) {
			if (Email.ServerName == string.Empty) {
				throw new NullReferenceException("No Email.ServerName has been specified");
			}
			System.Net.Sockets.TcpClient client = 
				new System.Net.Sockets.TcpClient(domain, (int)Port.Smtp);
			_stream = client.GetStream();
			_reader = new System.IO.StreamReader(client.GetStream());
			Result status = Result.Succeeded;
			string mailServer = Email.ServerName;
			string crlf = Environment.NewLine;

			if (mailServer == null) { mailServer = MxRecord.Infer(from).HostName; }

			// some servers acknowledge connection
			if (_reader.Peek() != -1) { this.ValidResponse(Response.ConnectSuccess); }

			// HELO
			if (!this.SmtpCommand(string.Format("HELO {0}{1}", mailServer, crlf))) {
				status = Result.NoServerGreeting;
			}
			// MAIL FROM
			if (status.Contains(Result.Succeeded) &&
				!this.SmtpCommand(string.Format("MAIL FROM:<{0}>{1}", from, crlf))) {
				status = Result.RefusedMailFrom;
			}
			// RCPT TO
			if (status.Contains(Result.Succeeded) &&
				!this.SmtpCommand(string.Format("RCPT TO:<{0}>{1}", recipient, crlf))) {
				status = Result.AddressNotFound;
			}
			// RCPT TO random
			// if random address is successful then server is not really validating
			if (status.Contains(Result.Succeeded) &&
				this.SmtpCommand(string.Format("RCPT TO:<{0}@{1}>{2}", _random,
					this.HostPart(recipient), crlf))) {

				status = Result.ValidatedBadAddress;
			}
			// QUIT
			this.SmtpCommand(string.Format("QUIT{0}", crlf), Response.QuitSuccess);

			_stream.Close();
			_reader.Close();
			client.Close();

			return status;
		}

		/// <summary>
		/// Return host portion of address
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		private string HostPart(string address) {
			return address.Substring(address.IndexOf("@") + 1);
		}

		/// <summary>
		/// Send an SMTP command
		/// </summary>
		/// <param name="command"></param>
		/// <param name="expected"></param>
		/// <returns></returns>
		private bool SmtpCommand(string command, Response expected) {
			_stack.Push(command);
			byte[] buffer = Encoding.ASCII.GetBytes(_stack.Peek().ToString());
			_stream.Write(buffer, 0, buffer.Length);
			return this.ValidResponse(expected);
		}
		private bool SmtpCommand(string command) {
			return this.SmtpCommand(command, Response.GenericSuccess);
		}

		/// <summary>
		/// Determine if SMTP response is valid (expected)
		/// </summary>
		/// <param name="expected"></param>
		/// <returns></returns>
		private bool ValidResponse(Response expected) {
			string line = _reader.ReadLine();
			if (expected == Response.QuitSuccess && line == string.Empty) {
				return false;
			}
			_stack.Push(line);
			int code = int.Parse(line.Substring(0, 3));
			return (code == (int)expected);
		}

	}
}