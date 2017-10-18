using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idaho {
	public static class TypeExtension {

		/// <summary>
		/// Name in format [full type name].[assembly name]
		/// </summary>
		/// <returns>String name</returns>
		/// <example>IDCL.BPC.Controls.CaseGrid, IDCL.BPC.Web, Version=1.0.2062.16484, Culture=neutral, PublicKeyToken=null</example>
		public static string QualifiedName(this Type t) {
			string tooLong = t.AssemblyQualifiedName;
			return tooLong.Substring(0, tooLong.IndexOf("Version") - 2);
		}

		/// <summary>
		/// See if the type implements the interface
		/// </summary>
		/// <param name="t">Type being checked</param>
		/// <param name="i">Interface being checked for</param>
		public static bool ImplementsInterface(this Type t, Type i) {
			Type[] interfaces = t.GetInterfaces();
			foreach (Type x in interfaces) {
				if (x.Equals(i)) { return true; }
			}
			return false;
		}

	}
}
