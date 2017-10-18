using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Idaho.Data {
	/// <summary>
	/// Select an alternate type to deserialize (bind) to
	/// </summary>
	/// <remarks>
	/// http://geekswithblogs.net/gavin/archive/2004/11/12/14907.aspx
	/// </remarks>
	public class Binder : SerializationBinder {
	   
		public override System.Type BindToType(string assemblyName, string typeName) {
			System.Type t;
			string oldName = string.Format("{0}, {1}", typeName, assemblyName);
			t = System.Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
			if (t == null) {
				Debug.BugOut("tried {0}, {1} (for {2})", typeName, assemblyName, oldName);
			}
			return t;
		}
	}
}