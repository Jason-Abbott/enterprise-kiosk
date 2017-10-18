using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Idaho.Web.Ajax {
	/// <summary>
	/// Invoke a method on an instance of the supplied page name
	/// </summary>
	/// <remarks>
	/// This is kind of a hybrid between a method and control request. A method
	/// request could be used for the same purpose, passing the type of the
	/// page, but this simplifies the effort.
	/// </remarks>
	public class PageRequest : MethodRequest {

		private static Assembly _assembly;
		public static Assembly Assembly { set { _assembly = value; } }

		internal PageRequest(Response r) : base(r) { }

		/// <summary>
		/// Use reflection to invoke method on page
		/// </summary>
		/// <remarks>
		/// This will not be a fully constituted page since we are not supplying the context
		/// or raising page lifecycle events.
		/// </remarks>
		internal override void Process() {
			string pageName = base["page"];
			string result = string.Empty;

			if (_assembly == null) {
				this.Response.AddError("No web page assembly specified");
				return;
			}
			Type pageType = Utility.GetType(_assembly, pageName);

			if (pageType == null) {
				this.Response.AddError("The page \"{0}\" could not be found in assembly \"{1}\"", pageName, _assembly.FullName);
				return;
			}
			base.InvokeMethod(pageType);
		}
	}
}
