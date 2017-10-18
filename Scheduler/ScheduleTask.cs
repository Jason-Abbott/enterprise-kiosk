using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Scheduler {
	/// <summary>
	/// A task that runs on a schedule
	/// </summary>
	public class Task {
		private TimeSpan _frequency = TimeSpan.MaxValue;
		private DateTime _lastRun = DateTime.MinValue;
		private DateTime _startedOn = DateTime.Now;
		private TimeSpan _delayRun = TimeSpan.Zero;
		private int _maximumRuns = -1;
		private int _runCount = 0;
		private TaskDelegate _delegate = null;

		/// <summary>
		/// Delegate the method that should run
		/// </summary>
		public delegate bool TaskDelegate();

		/// <summary>
		/// Create a task that will execute the given delegate at the given frequency
		/// </summary>
		/// <param name="frequency">How often this task should run</param>
		/// <param name="delay">How long to wait before running the first time</param>
		internal Task(TaskDelegate t, TimeSpan frequency, TimeSpan delay) {
			_delegate = t;
			_frequency = frequency;
			_delayRun = delay;
		}
		internal Task(TaskDelegate t, TimeSpan frequency) : this(t, frequency, TimeSpan.Zero) { }

		/// <summary>
		/// Does this task need to be run
		/// </summary>
		public bool NeedsToRun {
			get {
				// has it run the maximum number of times
				if (_maximumRuns > 0 && _runCount >= _maximumRuns) { return false; }
				// has the start delay passed
				if ((_startedOn + _delayRun) > DateTime.Now) { return false; }
				// has enough time passed since last run
				return ((_lastRun + _frequency) < DateTime.Now);
			}
		}

		public bool Execute() {
			bool success = false;
			if (_delegate != null) { success = _delegate(); }
			_lastRun = DateTime.Now;
			_runCount++;
			return success;
		}
	}
}
