using System;
using System.Threading;
using System.Configuration;
using System.Collections.Generic;

namespace Idaho {
	/// <summary>
	/// Configure tasks to run on a schedule in a web environment
	/// </summary>
	public class Schedule : List<Scheduler.Task> {
		private bool _executeAgain = true;
		private System.Threading.Thread _thread = null;
		private TimeSpan _sleepTime = TimeSpan.FromMinutes(30);

		#region Properties

		/// <summary>
		/// A reference to the thread constructed for this scheduler
		/// </summary>
		public System.Threading.Thread Thread { set { _thread = value; } }

		/// <summary>
		/// Amount of time the scheduling thread will sleep before it checks
		/// scheduled tasks for execution needs
		/// </summary>
		public TimeSpan SleepTime { set { _sleepTime = value; } }

		#endregion

		/// <summary>
		/// Create a scheduler that will check member tasks for execution needs
		/// at the given time interval
		/// </summary>
		public Schedule(TimeSpan sleepTime) { _sleepTime = sleepTime; }

		/// <summary>
		/// Create a task to execute the given delegate at the given frequency
		/// </summary>
		/// <param name="frequency">How often this taks should run</param>
		/// <param name="delay">How long to wait before first run</param>
		public void Add(Scheduler.Task.TaskDelegate t, TimeSpan frequency, TimeSpan delay) {
			this.Add(new Scheduler.Task(t, frequency, delay));
		}
		public void Add(Scheduler.Task.TaskDelegate t, TimeSpan frequency) {
			this.Add(t, frequency, TimeSpan.Zero);
		}
		/// <summary>
		/// Create a delegate based on the ITask object and from that create
		/// a task that will execute the delegate at the given frequency
		/// </summary>
		/// <param name="frequency">How often this taks should run</param>
		/// <param name="delay">How long to wait before first run</param>
 		/// <example>
		/// _schedule.Add(new Tasks.UpdateUsers(), TimeSpan.FromHours(1), TimeSpan.FromMinutes(30));
		/// </example>
		public void Add(Scheduler.ITask t, TimeSpan frequency, TimeSpan delay) {
			this.Add(new Scheduler.Task.TaskDelegate(t.Execute), frequency, delay);
		}
		public void Add(Scheduler.ITask t, TimeSpan frequency) {
			this.Add(t, frequency, TimeSpan.Zero);
		}

		/// <summary>
		/// Execute scheduled jobs
		/// </summary>
		public void Start() {
			while (_executeAgain) {
				this.ForEach(t => { if (t.NeedsToRun) { t.Execute(); } });
				Thread.Sleep(_sleepTime);
			}
		}
	}
}