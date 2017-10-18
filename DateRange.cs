using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	/// <summary>
	/// Represent a date range
	/// </summary>
	/// <remarks>
	/// The range is inclusive meaning the complete start and end day
	/// (the default precision) are part of the time span.
	/// </remarks>
	[Serializable()]
	public class DateRange : IEquatable<DateRange>, IComparable<DateRange> {

		private DateTime _start = DateTime.MinValue;
		private DateTime _end = DateTime.MaxValue;
		private bool _estimated = false;
		private event EventHandler<EventArgs<DateTime>> _subscribers = null;
		private PreciseTo _precision = PreciseTo.Default; 
		public enum PreciseTo {
			Second,
			Minute, 
			QuarterHour,
			HalfHour,
			Hour,
			Day,
			Default = Day
		}

		#region Properties

		/// <summary>
		/// Is the start or (typically) end date estimated
		/// </summary>
		public bool Estimated { get { return _estimated; } set { _estimated = value; } }

		public PreciseTo Precision {
			set {
				if (_precision != value) {
					_precision = value;
					// re-apply dates to round to new precision
					this.Start = _start;
					this.End = _end;
				}
			}
			get { return _precision; } }

		/// <summary>
		/// Timespan equal to the precision setting
		/// </summary>
		public TimeSpan PrecisionSpan {
			get {
				switch (_precision) {
					case PreciseTo.Second: return TimeSpan.FromSeconds(1);
					case PreciseTo.Minute: return TimeSpan.FromMinutes(1);
					case PreciseTo.QuarterHour: return TimeSpan.FromMinutes(15);
					case PreciseTo.HalfHour: return TimeSpan.FromMinutes(30);
					case PreciseTo.Hour: return TimeSpan.FromHours(1);
					case PreciseTo.Day: return TimeSpan.FromDays(1);
				}
				return TimeSpan.Zero;
			}
		}

		/// <summary>
		/// Raise an event when boundary dates change
		/// </summary>
		public event EventHandler<EventArgs<DateTime>> ChangeEvent {
			add { _subscribers += value; }
			remove { _subscribers -= value; }
		}
		public int StartHour {
			set {
				DateTime n = DateTime.Now;
				this.Start = new DateTime(n.Year, n.Month, n.Day, value, 0, 0);
			}
		}
		public int EndHour {
			set {
				DateTime n = DateTime.Now;
				this.End = new DateTime(n.Year, n.Month, n.Day, value, 0, 0);
			}
		}

		public DateTime Start {
			get { return _start; }
			set {
				this.AssertEndAfterStart(value, _end);
				switch (_precision) {
					case PreciseTo.Day: _start = value.StartOfDay(); break;
					case PreciseTo.Hour: _start = value.StartOfHour(); break;
					default: _start = value; break;
				}
				this.Changed(_start);
			}
		}
		public DateTime End {
			get { return _end; }
			set {
				this.AssertEndAfterStart(_start, value);
				switch (_precision) {
					case PreciseTo.Day: _end = value.EndOfDay(); break;
					case PreciseTo.Hour: _end = value.EndOfHour(); break;
					default: _end = value; break;
				}
				this.Changed(_end);
			}
		}
		/// <summary>
		/// Get timespan between beginning and end or update end to accomodate given span
		/// </summary>
		public TimeSpan Span {
			get { return _end.Subtract(_start); }
			set { _end = _start.Add(value); }
		}

		/// <summary>
		/// Does the date range cover the current date
		/// </summary>
		public bool Current {
			get { return (!this.IsEmpty) && DateTime.Now >= _start && DateTime.Now <= _end; }
		}
		public static DateRange Empty { get { return new DateRange(); } }
		public bool IsEmpty {
			get { return _start.Equals(DateTime.MinValue) && _end.Equals(DateTime.MaxValue); }
		}
		public bool IsValid {
			get { return !(_start.Equals(DateTime.MinValue) || _end.Equals(DateTime.MaxValue)); }
		}

		#endregion

		#region Constructors

		private DateRange() { }
		public DateRange(DateTime start, DateTime end, PreciseTo precision) {
			this.AssertEndAfterStart(start, end);
			_precision = precision;
			this.Start = start;
			this.End = end;
		}
		public DateRange(DateTime start, DateTime end) : this(start, end, PreciseTo.Default) { }
		public DateRange(DateTime start, TimeSpan duration) : this(start, start.Add(duration)) { }
		public DateRange(DateTime start, TimeSpan duration, PreciseTo precision)
			: this(start, start.Add(duration), precision) { } 

		#endregion

		/// <summary>
		/// Called when a boundary date is changed
		/// </summary>
		/// <remarks>
		/// This raises the change event if there are any subscribers.
		/// </remarks>
		private void Changed(DateTime t) {
			if (_subscribers != null) {
				_subscribers(this, new EventArgs<DateTime>(t, EventType.NewValue));
			}
		}

		/// <summary>
		/// Require start date to precede end date
		/// </summary>
		private void AssertEndAfterStart(DateTime start, DateTime end) {
			if (end < start) { throw new InvalidOperationException(
				string.Format(Resource.Say("Error_InvalidDateRange"), start, end)); }
		}
		public DateRange Intersection(DateRange other) {
			if (other == null) { return null; }
			return new DateRange(this.LatestStart(other.Start), this.EarliestEnd(other.End));
		}
		private DateTime LatestStart(DateTime other) {
			return (_start > other) ? _start : other;
		}
		private DateTime EarliestEnd(DateTime other) {
			return (_end < other) ? _end : other;
		}
		public bool Equals(DateRange other) {
			return _start.Equals(other.Start) && _end.Equals(other.End);
		}
		/// <summary>
		/// Does this range contain the given date
		/// </summary>
		public bool Contains(DateTime t) { return (t >= _start && t <= _end); }

		/// <summary>
		/// Parse a DateRange from string
		/// </summary>
		/// <param name="dates">in the format [StartDate],[EndDate]</param>
		public static DateRange Parse(string dates) {
			return Parse(dates, PreciseTo.Default);
		}
		public static DateRange Parse(string dates, PreciseTo precision) {
			if (string.IsNullOrEmpty(dates)) { return DateRange.Empty; }
			string[] date = dates.Split(',');
			if (date == null || date.Length < 2) { return DateRange.Empty; }
			return Parse(date[0], date[1], precision);
		}
		public static DateRange Parse(string start, string end, PreciseTo precision) {
			if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end)) {
				return DateRange.Empty;
			}
			return new DateRange(DateTime.Parse(start), DateTime.Parse(end), precision);
		}
		public static DateRange Parse(string start, string end) {
			return Parse(start, end, PreciseTo.Default);
		}

		public string ToString(string format) {
			return string.Format("{0}–{1}", _start.ToString(format), _end.ToString(format));
		}
		public override string ToString() {
			return this.ToString(string.Empty);
		}

		public int CompareTo(DateRange other) {
			return _start.CompareTo(other.Start);
		}
	}
}
