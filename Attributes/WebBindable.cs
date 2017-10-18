using System;
using System.Collections.Generic;
using System.Reflection;

namespace Idaho.Attributes {
	/// <summary>
	/// Indicate which properties of IAsynchronous controls should
	/// be exposed for EcmaScript assignment via an asynchronous call
	/// and reflection. These properties must be both readable and
	/// writable to fully support reflection.
	/// </summary>
	/// <example>
	/// see Idaho.Web.Controls.Grid for example
	/// see Idaho.Web.Controls.Asynchronous where attribute is read
	/// </example>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public class WebBindable : System.Attribute {

		/// <summary>
		/// Should property or method be treated as always bindable
		/// </summary>
		/// <remarks>
		/// By default, no binding script is rendered for properties that have
		/// a null value. Set this to true to force the property to be rendered
		/// even if null.
		/// </remarks>
		private bool _alwaysBind = false;

		public WebBindable() { }
		public WebBindable(bool alwaysBind) { _alwaysBind = alwaysBind; }

		/// <summary>
		/// Get all the web bindable properties of a given type
		/// </summary>
		public static PropertyInfo[] Properties(System.Type t) {
			BindingFlags binding = BindingFlags.Public | BindingFlags.Instance;
			PropertyInfo[] candidates = t.GetProperties(binding);
			Type attribute = typeof(WebBindable);
			List<PropertyInfo> matches = new List<PropertyInfo>();

			foreach (PropertyInfo p in candidates) {
				if (p.GetCustomAttributes(attribute, false).Length > 0 && p.CanRead && p.CanWrite)
					matches.Add(p);
			}
			return matches.ToArray();
		}

		/// <summary>
		/// Deterine if given property has AlwaysBind property set
		/// </summary>
		public static bool AlwaysBind(PropertyInfo p) {
			object[] attributes = p.GetCustomAttributes(typeof(WebBindable), false);
			if (attributes.Length > 0) {
				WebBindable bindable = (WebBindable)attributes[0];
				return bindable._alwaysBind;
			} else {
				return false;
			}
		}
	}
}