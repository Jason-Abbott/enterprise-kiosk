using Idaho.Web;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace Idaho.Web.Handlers {
	/// <summary>
	/// Handle asynchronous (AJAX) calls
	/// </summary>
	/// <example>@ WebHandler Language="cs" class="Idaho.Web.Handlers.Ajax"</example>
	/// http://msdn.microsoft.com/msdnmag/issues/07/02/CuttingEdge/default.aspx
	/// http://microsoft.apress.com/asptodayarchive/73323/create-a-custom-ihttphandler-to-allow-aspnet-pages-to-communicate-with-each-other
	public class AjaxHandler : IHttpHandler, IRequiresSessionState {

		private static Dictionary<string, Type> _knownTypes = new Dictionary<string,Type>();
		internal static Dictionary<string, Type> KnownType { get {	return _knownTypes;	} }

		/// <summary>
		/// Register a type with an abbreviated name
		/// </summary>
		/// <remarks>
		/// To avoid the need to pass full type names through script, types
		/// may be registered with an abbreviated name
		/// </remarks>
		public static void RegisterType(string name, Type type) { _knownTypes.Add(name, type); }

		/// <summary>The type of object to instantiate</summary>
		/// <remarks>
		/// This framework can handle three basic AJAX patterns: one to
		/// render a control and return the resultant HTML out-of-band,
		/// another to invoke a method and return its result in
		/// JSON format and one to invoke a method on a page instance.
		/// </remarks>
		protected enum Patterns { RenderControl, InvokeMethod, PageMethod }

		/// <summary>
		/// Determine the type (pattern) of request based on query string
		/// </summary>
		private Patterns InferPattern(HttpContext context) {
			NameValueCollection qs = context.Request.QueryString;
			if (qs["page"] != null) {
				return Patterns.PageMethod;
			} else if (qs["method"] != null) {
				return Patterns.InvokeMethod;
			} else {
				return Patterns.RenderControl;
			}
		}

		/// <summary>
		/// Get the request object to handle this pattern
		/// </summary>
		internal Ajax.Request HandlerFactory(Ajax.Response result, HttpContext context) {
			Patterns pattern = this.InferPattern(context);
			switch (pattern) {
				case Patterns.InvokeMethod:
					return new Ajax.MethodRequest(result);
				case Patterns.RenderControl:
					return new Ajax.ControlRequest(result);
				case Patterns.PageMethod:
					return new Ajax.PageRequest(result);
				default:
					// cannot proceed
					return null;
			}
		}

		#region IHttpHandler

		/// <summary>
		/// Triage for AJAX patterns
		/// </summary>
		public void ProcessRequest(HttpContext context) {

			Ajax.Request request = this.HandlerFactory(new Ajax.Response(context), context);

			if (request != null) {
				// do not cache the response since underlying data could change at anytime
				context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
				request.Process();
			}
		}

		public bool IsReusable { get { return false; } }

		#endregion

	}
}
