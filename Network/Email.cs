using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Idaho.Network {
	public abstract class Email {
		//private const string _schema = "http://schemas.microsoft.com/cdo/configuration/";

		#region Static Fields

		private static MailAddress _from;
		private static MailAddress _redirectTo;
		private static MailAddress _bounceTo;
		private static string _userName = string.Empty;
		private static string _password = string.Empty;
		private static bool _useServer = false;
		private static string _serverName = string.Empty;

		public static MailAddress From { set { _from = value; } }
		public static MailAddress RedirectTo { set { _redirectTo = value; } }
		public static MailAddress BounceTo { set { _bounceTo = value; } }

		/// <summary>
		/// E-mail server user name
		/// </summary>
		public static string UserName { set { _userName = value; } }

		/// <summary>
		/// E-mail server password
		/// </summary>
		public static string ServerPassword { set { _password = value; } }

		/// <summary>
		/// E-mail server name
		/// </summary>
		public static string ServerName {
			set { _serverName = value; }
			internal get { return _serverName; }
		}
		public static bool UseServer { set { _useServer = value; } }

		#endregion

		#region Enumerations

		protected enum SendUsing { Pickup = 1, Port = 2	}
		protected enum Authentication { Anonymous, Basic, Ntlm }

		#endregion

		/// <summary>
		/// Send message through mail server indicated in AppSettings
		/// </summary>
		/// <remarks>
		/// Will redirect to alternate address if AppSettings property is
		/// configured
		/// </remarks>
		protected bool Send(MailMessage mail) {
			SmtpClient smtpClient = new SmtpClient();

			if (_userName != null) {
				// server must require authentication
				smtpClient.Credentials = new System.Net.NetworkCredential(_userName, _password);
			}
			if (_useServer && !string.IsNullOrEmpty(_serverName)) { smtpClient.Host = _serverName; }
		   
			// send only to debug address if one specified
			if (_redirectTo != null) {
				string oldRecipient = mail.To.ToString();
				mail.To.Clear(); mail.CC.Clear(); mail.Bcc.Clear();
				mail.To.Add(_redirectTo);
				string newLine = (mail.IsBodyHtml) ? "<br>" : Environment.NewLine;
				mail.Body += string.Format("{0}{0}(originally sent to {1})", newLine, oldRecipient);
			}
			if (_bounceTo != null) {
				mail.Headers.Add("Return-Path", _bounceTo.ToString());
				mail.Headers.Add("From", _from.ToString());
			} else {
				mail.From = _from;
			}
			try { smtpClient.Send(mail); }
			catch { return false; }
			return true;
		}
	}
}