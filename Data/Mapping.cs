using System;

namespace Idaho.Data {
	/// <summary>
	/// Attribute to define deserialization mapping
	/// </summary>
	/// <example><code>[AMP.Data.Mapping(Mapping.ChangeType.Rename, "_name")]</code></example>
	[AttributeUsage(AttributeTargets.Field)]
	public class Mapping : System.Attribute {
	   
		private Mapping.ChangeType _type;
		// for renamed fields
		private string _oldName;
	   
		#region Properties
	   
		public Mapping.ChangeType Type { get { return _type; } }
		public string OldName { get { return _oldName; } }
	   
		#endregion
	   
		public enum ChangeType { Rename, NewField }
	   
		public Mapping(Mapping.ChangeType type, string oldName) : this(type) {
			_oldName = oldName;
		}
		public Mapping(Mapping.ChangeType type) { _type = type; }
	}
}