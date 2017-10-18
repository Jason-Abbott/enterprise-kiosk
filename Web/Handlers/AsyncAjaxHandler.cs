using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace Idaho.Web.Handlers {
	/// <summary>
	/// This matches the functionality of the regular AJAX handler but
	/// spawns a new thread to handle the processing
	/// </summary>
	/// <see cref="http://msdn.microsoft.com/msdnmag/issues/03/06/Threading/default.aspx"/>
	public class AsyncAjaxHandler : Handlers.AjaxHandler, IHttpAsyncHandler {

		#region IHttpAsyncHandler

		/// <summary>
		/// Triage for AJAX patterns
		/// </summary>
		public System.IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback callback, object data) {
			Ajax.AsyncResult result = new Ajax.AsyncResult(context, callback);
			Ajax.Request request = this.HandlerFactory(result, context);

			if (request != null) {
				Thread worker = new Thread(request.Process);
				worker.Start();
			}
			return result;
		}

		public void EndProcessRequest(IAsyncResult result) {
			//If TypeOf result Is IDCL.Asynchronous.Result Then
			//   'could do any needed cleanup here
			//   Me.Profile.Synchronize()
			//End If
		}

		#endregion

	}
}
