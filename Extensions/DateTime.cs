using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idaho {
	public static class DateTimeExtension {

		private static DateTime _minimum = DateTime.Parse("1/1/1753 12:00:00 AM");
		private static DateTime _maximum = DateTime.Parse("12/31/9999 11:59:59 PM");

		[Flags()]
		public enum Format {
			Capitalize=0x1,
			ShowWeekDay=0x2,
			ShowTime=0x4,
			ShowYear=0x8
		}

		public static DateTime Parse(this DateTime dt, object raw) {
			if (raw == null) { return DateTime.MinValue; }

			if (raw is DateTime) {
				return (DateTime)raw;
			} else if (raw is string) {
				return DateTime.Parse(raw as string);
			} else {
				return DateTime.MinValue;
			}
		}

		#region Formatting
		// http://blog.stevex.net/index.php/string-formatting-in-csharp/

		//public override string ToString() { return _dt.ToString(); }
		public static string ToString(this DateTime dt, TimeSpan offset) {
			DateTime adjusted = dt.Add(offset);
			return adjusted.ToString();
		}
		//public string ToString(string format) { return _dt.ToString(format); }
		public static string ToString(this DateTime dt, string format, TimeSpan offset) {
			DateTime adjusted = dt.Add(offset);
			return adjusted.ToString(format);
		}
		/// <summary>
		/// Describe time relative to the current time (yesterday, tomorrow)
		/// </summary>
		public static string Relative(this DateTime dt, Format options) {
			string day;
			bool relative = false;

			if (dt.Date == DateTime.Now.Date) {
				relative = true;
				day = options.Contains(Format.Capitalize) ? "Today" : "today";
			} else if (dt.Date == DateTime.Now.AddDays(-1).Date) {
				relative = true;
				day = options.Contains(Format.Capitalize) ? "Yesterday" : "yesterday";
			} else if (dt.Date == DateTime.Now.AddDays(1).Date) {
				relative = true;
				day = options.Contains(Format.Capitalize) ? "Tomorrow" : "tomorrow";
			} else if (dt.Year == DateTime.Now.Year && !options.Contains(Format.ShowYear)) {
				day = string.Format("{0:MMMM d}", dt);
			} else {
				day = string.Format("{0:MMMM d, yyyy}", dt);
			}

			if (options.Contains(Format.ShowTime)) {
				day = string.Format("{0}, {1:h:mm tt}", day, dt);
			}
			if ((!relative) && options.Contains(Format.ShowWeekDay)) {
				day = string.Format("{0:dddd}, {1}", dt, day);
			}
			return day;
		}
		public static string Relative(this DateTime dt) {
			return Relative(dt, Format.ShowTime);
		}

		#endregion

		#region Rounding

		public static DateTime EndOfDay(this DateTime dt) {
			return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);
		}
		public static DateTime StartOfDay(this DateTime dt) {
			return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
		}
		public static DateTime EndOfHour(this DateTime dt) {
			return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 59, 59);
		}
		public static DateTime StartOfHour(this DateTime dt) {
			return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
		}

		#endregion

		#region Equality

		public static bool SameYear(this DateTime dt, DateTime t) {
			return dt.Year.Equals(t.Year);
		}
		public static bool SameMonth(this DateTime dt, DateTime t) {
			return dt.SameYear(t) && dt.Month.Equals(t.Month);
		}
		public static bool SameDay(this DateTime dt, DateTime t) {
			return dt.SameMonth(t) && dt.Day.Equals(t.Day);
		}
		public static bool SameHour(this DateTime dt, DateTime t) {
			return dt.SameDay(t) && dt.Hour.Equals(t.Hour);
		}
		public static bool SameMinute(this DateTime dt, DateTime t) {
			return dt.SameHour(t) && dt.Minute.Equals(t.Minute);
		}
		public static bool SameSecond(this DateTime dt, DateTime t) {
			return dt.SameMinute(t) && dt.Second.Equals(t.Second);
		}

		#endregion

		public static DateTime Yesterday(this DateTime dt) {
			return DateTime.Now.AddDays(-1);
		}
		public static int UnixStamp(this DateTime dt) {
			TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
			return (int)t.TotalSeconds;
		}
	}
}
