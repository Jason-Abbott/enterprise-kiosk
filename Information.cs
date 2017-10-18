using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Idaho {
	/// <summary>
	/// Display assembly information
	/// </summary>
	public class Information {
		public static string Copyright {
			get { return AttributeValue<AssemblyCopyrightAttribute>().Copyright; }
		}
		public static string CompanyName {
			get { return AttributeValue<AssemblyCompanyAttribute>().Company; }
		}
		public static string ProductName {
			get { return AttributeValue<AssemblyProductAttribute>().Product; }
		}
		private static T AttributeValue<T>() {
			Assembly assembly = System.Reflection.Assembly.GetEntryAssembly();
			object[] values = assembly.GetCustomAttributes(typeof(T), false);
			return (values != null) ? (T)values[0] : default(T);
		}
	}
}
