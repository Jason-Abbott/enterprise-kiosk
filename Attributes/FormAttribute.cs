using System;

namespace Idaho.Attributes {
	/// <summary>
	/// Attribute to build form fields from object members
	/// </summary>
	/// <example><c>[AMP.Attributes.Form("Label", True, 10)]</c></example>
	/// <remarks>
	/// Used to automatically generate an HTML form from an object. The
	/// promotions form is a current example.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class Form : System.Attribute {

		private string _prefix;
		private string _suffix = string.Empty;
		private Int16 _order = 0;

		public Int16 Order { get { return _order; } }
		public string Prefix { get { return _prefix; } }
		public string Suffix { get { return _suffix; } }

		public Form(string prefix, string suffix, Int16 order) {
			_prefix = prefix;
			_suffix = suffix;
			_order = order;
		}
		public Form(string prefix, Int16 order) { _prefix = prefix; _order = order; }
		public Form(string prefix, string suffix) { _prefix = prefix; _suffix = suffix; }
		public Form(string prefix) { _prefix = prefix; }
	}
}