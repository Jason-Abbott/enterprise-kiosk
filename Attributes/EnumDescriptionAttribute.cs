using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Attributes {
	/// <summary>
	/// Add a description and category to an enumeration value
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class EnumDescription : System.Attribute {
		private string _category = string.Empty;
		private string _description = string.Empty;

		public string Category { get { return _category; } }
		public string Description { get { return _description; } }

		public EnumDescription(string category, string description) {
			_description = description;
			_category = category;
		}
		public EnumDescription(string category) {
			_category = category;
		}
	}
}