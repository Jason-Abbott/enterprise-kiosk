using Idaho.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Web;
using config = System.Configuration.ConfigurationManager;

namespace Idaho.Web {
	public abstract class Application : HttpApplication {

		protected bool SettingExists(string key) {
			return !(string.IsNullOrEmpty(config.AppSettings[key]));
		}
		protected string[] ArrayFromSetting(string key, string delimiter) {
			string value = config.AppSettings[key];
			if (!string.IsNullOrEmpty(value)) {
				return (value.Contains(delimiter))
					? value.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)
					: new string[] { value };
			} else {
				return null;
			}
		}
		protected string[] ArrayFromSetting(string key) {
			return this.ArrayFromSetting(key, ",");
		}
		protected MailAddress AddressFromSetting(string key) {
			string address = config.AppSettings[key];
			return (string.IsNullOrEmpty(address)) ? null : new MailAddress(address);
		}
		protected DirectoryInfo DirectoryFromSetting(string key) {
			string path = config.AppSettings[key].Replace('/', '\\');
			if (string.IsNullOrEmpty(path)) { return null; }
			return new DirectoryInfo(string.Format("{0}{1}", HttpRuntime.AppDomainAppPath, path));
		}
		protected string PathFromSetting(string key) {
			string path = config.AppSettings[key];
			if (string.IsNullOrEmpty(path)) { return string.Empty; }
			return HttpContext.Current.Server.MapPath(path);
		}
		protected TimeSpan DurationFromSetting(string key) {
			string duration = config.AppSettings[key];
			if (!string.IsNullOrEmpty(duration)) {
				double count = double.Parse(duration);
				key = key.ToLower();
				if (key.Contains("second")) { return TimeSpan.FromSeconds(count); } else if (key.Contains("minute")) { return TimeSpan.FromMinutes(count); } else if (key.Contains("hour")) { return TimeSpan.FromHours(count); }
			}
			return TimeSpan.MaxValue;
		}
		protected Color ColorFromSetting(string key) {
			string color = config.AppSettings[key];
			if (string.IsNullOrEmpty(color)) { return Color.Empty; }
			return ColorTranslator.FromHtml(color);
		}
	}
}
