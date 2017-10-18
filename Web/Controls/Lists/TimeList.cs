using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Idaho.Web.Controls {
	public class TimeList : Idaho.Web.Controls.SelectList {

		private string _nameFormat = "h:mm tt";
		private string _valueFormat = "s";
		private Idaho.DateRange _range = new Idaho.DateRange(
			DateTime.MinValue, DateTime.MaxValue, Idaho.DateRange.PreciseTo.QuarterHour);

		#region Properties

		/// <summary>
		/// List of date/time objects to display as options
		/// </summary>
		public List<DateTime> Values {
			set {
				var l = new SortedDictionary<string, string>();
				string v = string.Empty;

				foreach (DateTime d in value) {
					v = d.ToString(_valueFormat);
					if (!l.Contains(v)) { l.Add(v, d.ToString(_nameFormat)); }
				}
				base.KeysAndValues = l;
			}
		}

		/// <summary>
		/// Selected date/time values
		/// </summary>
		public new List<DateTime> Selections {
			get {
				var l = new List<DateTime>();
				var selected = base.Selections;
				foreach (string s in selected) {
					l.Add(DateTime.Parse(s));
				}
				return l;
			}
		}

		/// <summary>
		/// Selected time or minimum date if no selection
		/// </summary>
		public new DateTime Selection {
			get {
				string s = base.Selection;
				if (!string.IsNullOrEmpty(s)) {
					return DateTime.Parse(s);
				} else {
					return DateTime.MinValue;
				}
			}
		}

		/// <summary>
		/// Range of times to display in list (date is ignored)
		/// </summary>
		public Idaho.DateRange Range { get { return _range; } set { _range = value; } }

		/// <summary>
		/// Format string for the time
		/// </summary>
		public string Format { set { _nameFormat = value; } }

		#endregion

		protected override void OnLoad(EventArgs e) {
			base.StyleCondition = new KeyValuePair<Regex, string>(
				new Regex(":00 ", RegexOptions.Singleline), "hour");

			if (_range.IsValid) {
				int i = 0;
				var d = new SortedDictionary<string, string>();
				DateTime t = _range.Start;
				while (t < _range.End && i < 250) {
					d.Add(t.ToString(_valueFormat), t.ToString(_nameFormat));
					t = t.Add(_range.PrecisionSpan);
					i++;
				}
				base.KeysAndValues = d;
			}
			base.OnLoad(e);
		}
	}
}
