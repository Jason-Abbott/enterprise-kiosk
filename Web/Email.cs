using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Idaho.Web {
	public class Email : Idaho.Network.Email {

		public Email() { }

		#region Static Fields

		private static List<MailAddress> _fulfillmentBcc = new List<MailAddress>();
		private static string _hostName = string.Empty;

		public static List<MailAddress> FulfillmentBcc { get { return _fulfillmentBcc; } }
		public static string HostName { set { _hostName = value; } }

		#endregion
	
		#region Security

		/// <summary>
		/// Send password to user
		/// </summary>
		public bool Password(string name, string email, string newPassword) {
			MailMessage mail = new MailMessage();
			MailAddress sendTo = new MailAddress(email, name);
			string body = string.Empty; // this.Template("PasswordTemplate");

			body = body.Replace("<name>", name);
			body = body.Replace("<password>", newPassword);

			mail.IsBodyHtml = false;
			mail.To.Add(sendTo);
			mail.Subject = Resource.SayFormat("Subject_NewPassword", Resource.Say("SiteName"));
			mail.Body = body;

			return base.Send(mail);
		}
		public bool Password(User user, string newPassword) {
			return this.Password(user.Name, user.Email, newPassword);
		}

		#endregion

		#region Debugging

		/// <summary>
		/// Mail the content of a form post
		/// </summary>
		/// <param name="posted">The typical Request.Form collection</param>
		public void Post(NameValueCollection posted) {
			Assert.NoNull(Exception.MailTo, "NullMailTo");
			StringBuilder body = new StringBuilder();
			foreach (string key in posted.Keys) {
				body.Append(key);
				body.Append("=");
				body.Append(posted[key]);
				body.Append(Environment.NewLine);
			}
			MailMessage mail = new MailMessage();
			mail.IsBodyHtml = false;
			mail.Body = body.ToString();
			mail.Subject = "Form post";
			mail.To.Add(Exception.MailTo);
			this.Send(mail);
		}

		#endregion

		/// <summary>
		/// Send e-mail about critical error
		/// </summary>
		public void Error(Exception ex, List<MailAddress> sendTo, int occurrences) {

			Assert.NoNull(sendTo, "NullMailTo");
			MailMessage mail = new MailMessage();
			StringBuilder recipients = new StringBuilder();
			string body = Exception.MailTemplate;
			string occurrenceText = string.Empty;
			string solutionText = string.Empty;
			string folder = string.Empty;

			if (occurrences > 0) {
				occurrenceText = "<p>" + Resource.SayFormat("Message_Occurrences", occurrences) + "</p>";
			}

			if (ex.Message.Contains("GDI")) {
				// an error writing generated images
				folder = Draw.GraphicBase.GeneratedImageFolder.Name;
			} else if (ex.Message.Contains("updateable query")) {
				// an error accessing the database
				folder = string.Format("{0} and {1}", Data.File.DataFolder.Name, "");
			} else if (ex.Message.Contains("Access to the path")) {
				// some other file permissions problem
				Regex re = new Regex(string.Format("{0}(\\w+)[\\\\/]",
					HttpRuntime.AppDomainAppPath.Replace("\\", "\\\\")),
					RegexOptions.IgnoreCase);
				Match m = re.Match(ex.Message);
				folder = m.Groups[1].Value;
			}

			if (!string.IsNullOrEmpty(folder)) {
				solutionText = Idaho.Data.File.PermissionTemplate;
				solutionText = solutionText.Replace("<hostName>", _hostName);
				solutionText = solutionText.Replace("<folder>", folder);
			}
			body = body.Replace("<userName>", (ex.User != null) ? ex.User.Name : Exception.DefaultCustomerName);
			body = body.Replace("<dateTime>", DateTime.Now.ToString());
			body = body.Replace("<userIP>", ex.IpAddress.ToString());
			body = body.Replace("<server>", ex.MachineName);
			body = body.Replace("<process>", ex.MachineName);
			body = body.Replace("<message>", ex.Message);

			if (!string.IsNullOrEmpty(ex.Stack)) {
				body = body.Replace("<stack>", ex.Stack.Replace(Environment.NewLine, "<br/>"));
			}
			body = body.Replace("<occurrences>", occurrenceText);
			body = body.Replace("<solution>", solutionText);

			foreach (MailAddress a in sendTo) { mail.To.Add(a); }
			mail.IsBodyHtml = true;
			mail.Priority = MailPriority.High;
			mail.Subject = Resource.SayFormat("Subject_CriticalError", Resource.Say("SiteName"));
			mail.Body = body;

			try { this.Send(mail); } catch (System.Exception e) {
				// do not log or infinite loop may be created
				Debug.BugOut("Error sending mail through server {0}: {1}",
					Network.Email.ServerName, e.Message);
			}
		}
		public void Error(System.Exception ex, string userName, string clientIP, string machine,
			string process, List<String> sendTo, int occurrences) {

			List<MailAddress> list = new List<MailAddress>();
			foreach (string a in sendTo) { list.Add(new MailAddress(a)); }
			this.Error(ex, userName, clientIP, machine, process, sendTo, occurrences);
		}
	}
}