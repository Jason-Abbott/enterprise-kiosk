using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Idaho.Web {
	/// <summary>
	/// Methods supporting EcmaScript (javascript)
	/// </summary>
	/// <remarks>
	/// Override this in your project to support conversion between the
	/// types declared in your project.
	/// </remarks>
	/// <example>
	/// public override static [whatever]() {
	///		[ custom conversion code]
	///		// let base class handle standard conversions
	///		base.[whichever]();
	///	}
	/// </example>
	public static class EcmaScript {

		public const string Null = "null";
		public const string False = "false";
		public const string True = "true";

		/// <summary>
		/// Names of embedded EcmaScript files
		/// </summary>
		public class Resource {
			public const string Page = "page";
			public const string DOM = "dom";
			public const string AJAX = "ajax";
			public const string Cookies = "cookies";
			public const string Validation = "validation";
			public const string Control = "control";
			public const string Diagram = "diagram";
			public const string HierarchicalList = "hierarchy";
			public const string Extensions = "extensions";
		}

		#region Object Creation

		/// <summary>
		/// Create an EcmaScript object using methods in standard site.js
		/// </summary>
		/// <param name="name">Name of the EcmaScript object to be created</param>
		/// <param name="typeName">EcmaScript type to instantiate</param>
		/// <param name="arguments"></param>
		/// <param name="properties"></param>
		/// <returns>
		/// String that when executed as EcmaScript will create the specified object
		/// </returns>
		/// <remarks>
		/// Creation waits for the completion of site.js (an artifical event)
		/// since this object may have dependencies on those constructed globally
		/// </remarks>
		public static string CreateObject(string name, string typeName, object[] arguments,
			Dictionary<string, object> properties) {

			StringBuilder script = new StringBuilder();
			script.AppendFormat("var {0}; ", name);
			script.Append("AfterPageLoad(function() { ");
			script.AppendFormat("{0} = new {1}(", name, typeName);
			if (!(arguments == null || arguments.Length == 0)) {
				foreach (object arg in arguments) {
					script.AppendFormat("{0},", arg.ToJSON());
				}
				script.Length -= 1;
				// trim trailing comma
			}
			script.Append(");");
			if (!(properties == null || properties.Count == 0)) {
				script.AppendFormat(" with ({0}) {{", name);
				foreach (string p in properties.Keys) {
					script.AppendFormat(" {0} = {1};", p, properties[p].ToJSON());
				}
				script.Append(" }");
			}
			script.Append(" } );");
			return script.ToString();
		}
		public static string CreateObject(string name, string typeName, Dictionary<string, object> properties) {
			return EcmaScript.CreateObject(name, typeName, new string[] { }, properties);
		}
		public static string CreateObject(string name, string typeName, string[] arguments) {
			return EcmaScript.CreateObject(name, typeName, arguments, null);
		}
		public static string CreateObject(string name, string typeName) {
			return EcmaScript.CreateObject(name, typeName, new string[] { }, null);
		}

		/// <remarks>
		/// By convention, some script objects are designed to support a particular
		///	type and so are always constructed with that type's name.
		///	Specifically, asynchronous method calls need a type against which
		///	to invoke the method by reflection (see IDCL.Controls.Asynchronous).
		/// <see cref="IDCL.Controls.Asynchronous"/>
		/// </remarks>
		public static string CreateObject(string name, string typeName, Type t, object[] arguments) {
			int length = arguments.Length;
			object[] transfer = new object[length + 1];
			// type name needs to be first argument
			transfer[0] = t.QualifiedName();
			for (int x = 1; x <= length; x++) {
				transfer[x] = arguments[x - 1];
			}
			return EcmaScript.CreateObject(name, typeName, transfer, null);
		}
		public static string CreateObject(string name, string typeName, Type t) {
			return EcmaScript.CreateObject(name, typeName, new string[] {
				t.QualifiedName() }, null);
		}
		// by convention, JS object function names end with "Object"
		public static string CreateObject(string name, Type t, object[] arguments) {
			return EcmaScript.CreateObject(name, name + "Object", t, arguments);
		}
		public static string CreateObject(string name, Type t) {
			return EcmaScript.CreateObject(name, name + "Object", t);
		}
		public static string CreateObject(Type t) {
			return EcmaScript.CreateObject(t.Name, t);
		}
		public static string CreateObject(Type t, object[] arguments) {
			return EcmaScript.CreateObject(t.Name, t, arguments);
		}

		// overloads for script objects that operate on grid controls
		//Public Shared Function CreateObject(ByVal name As String, ByVal t As System.Type, _
		// ByVal grid As IDCL.Controls.Grid) As String

		// Return EcmaScript.CreateObject(name, t, New Object() {grid.ID})
		//End Function

		//Public Shared Function CreateObject(ByVal t As System.Type, ByVal grid As IDCL.Controls.Grid) As String
		// Return EcmaScript.CreateObject(t.Name, t, grid)
		//End Function

		#endregion

		#region Conversions

		/// <summary>
		/// Cast value to type expected by parameter
		/// </summary>
		/// <param name="value">EcmaScript literal</param>
		/// <param name="t">Type to convert to</param>
		public static object ToType(string value, Type t, HttpContext context) {		
			if (string.IsNullOrEmpty(value)) {
				// a caller may opt to leave certain non-scriptable parameter objects
				// null, allowing this method to instantiate them at conversion
				if (t == typeof(HttpContext) || t.IsSubclassOf(typeof(HttpContext))) {
					return context;
				} else if (t.IsSubclassOf(typeof(Idaho.Person))) {
					// attempt conversion to person object stored in profile
					Web.Profile profile = Web.Profile.Load(context);
					return profile.User;
				} else if (t.IsSubclassOf(typeof(Web.Profile))) {
					// pass inherited profile
					MethodInfo load = t.GetMethod("Load", new Type[] { typeof(HttpContext) });
					object profile = load.Invoke(null, new object[] { context });
					return Convert.ChangeType(profile, t);
				} else if (t == typeof(Web.Profile)) {
					return Web.Profile.Load(context);
				} else {
					return null;
				}
			}
			MethodInfo parse = t.GetMethod("Parse", new Type[] { typeof(string) });
			
			if (t.IsEnum) {
				return Enum.ToObject(t, int.Parse(value));
			} else if (t == typeof(Guid)) {
				return new Guid(value);
			} else if (t == typeof(Guid[])) {
				string[] values = value.Split(',');
				Guid[] guids = new Guid[values.Length + 1];
				for (int x = 0; x < values.Length ; x++) {
					guids[x] = new Guid(values[x]);
				}
				return guids;
			} else if (t == typeof(Type)) {
				return Type.GetType(value);
			} else if (t == typeof(string)) {
				return value;
			} else if (t == typeof(string[])) {
				return value.Split(',');
			} else if (t == typeof(int[])) {
				return Array.ConvertAll<string, int>(value.Split(','),
					delegate(string from) {
						int to = 0;
						return int.TryParse(from, out to) ? to : 0;
					});
			} else if (t == typeof(List<Guid>)) {
				string[] values = value.Split(',');
				List<Guid> list = new List<Guid>();
				for (int x = 0; x < values.Length; x++) {
					list.Add(new Guid(values[x]));
				}
				return list;
			} else if (parse != null) {
				// if a parse method exists, try that
				return parse.Invoke(null, new object[] { value });
			} else {
				// otherwise, attempt direct conversion
				return System.Convert.ChangeType(value, t);
			}
		}
		public static object ToType(Type t, HttpContext context) {
			return ToType(string.Empty, t, context);
		}

		#endregion

		/// <summary>
		/// Build EcmaScript string that will redirect to specified page
		/// </summary>
		/// <returns>String of EcmaScript</returns>
		/// <remarks>
		/// Adds EcmaScript AfterPageLoad() event so that redirection happens
		/// after page has completed loading
		/// </remarks>
		public static string Redirect(string url) {
			return string.Format("AfterPageLoad( function() {{ Page.Redirect(\"{0}\"); }} );", url);
		}

		/// <summary>
		/// Compress EcmaScript by removing unneeded characters
		/// </summary>
		public static string Compress(string script) {
			EcmaScriptMinify em = new EcmaScriptMinify();
			em.WriteMinified(script);
			return em.ToString();
		}
	
	}
}