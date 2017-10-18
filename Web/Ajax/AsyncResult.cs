using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace Idaho.Web.Ajax {
	/// <remarks>
	/// Note the two senses of asynchronous: first, in virtue of this being
	/// called by AJAX methods, then in virtue of the response being built
	/// within an IAsync Page.
	/// </remarks>
	internal class AsyncResult : Ajax.Response, IAsyncResult {

		private AsyncCallback _callback;
		private ManualResetEvent _completeEvent;
		private object _lockOn = new object();
		private bool _complete = false;

		internal AsyncResult(HttpContext context, AsyncCallback callback)
			: base(context) {
			_callback = callback;
		}

		internal void Complete(string html) {
			_complete = true;

			// complete any manually registered events
			//SyncLock _lockOn
			//    If Not _completeEvent Is Nothing Then _completeEvent.Set()
			//End SyncLock

			// call any registered callback handers
			if (_callback != null) { _callback(this); }
			base.Complete(html, true);
		}

		#region IAsyncResult

		// the object on which one could perform a lock
		public object AsyncState { get { return this.Parameters; } }

		// handle that a monitor could lock on
		public System.Threading.WaitHandle AsyncWaitHandle {
			get {
				lock (_lockOn) {
					if (_completeEvent != null) {
						_completeEvent = new ManualResetEvent(false);
					}
					return _completeEvent;
				}
			}
		}
		// always false for this implementation
		public bool CompletedSynchronously { get { return false; } }

		// status
		public bool IsCompleted { get { return _complete; } }

		#endregion

	}
}
