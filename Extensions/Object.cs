using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Idaho {
	public static class ObjectExtension {

		/// <summary>
		/// Copy all field values from one entity to another
		/// </summary>
		/// <returns>Whether the source changes the target</returns>
		/// <remarks>Uses reflection to recurse through base types.</remarks>
		public static bool Import(this object target, object source, BindingFlags binding) {
			bool changed = false;
			if (source == null || target == null) { return false; }
			System.Type type = target.GetType();
			if (type != source.GetType()) { return false; }
			System.Reflection.FieldInfo[] fields;
			System.Type exclude = typeof(Idaho.Attributes.NoUpdate);
			//object sourceValue = null;
			//object targetValue = null;

			// recurse until base object is encountered
			while (type != typeof(object)) {
				fields = type.GetFields(binding);
				foreach (System.Reflection.FieldInfo f in fields) {
					if (!f.HasAttribute(exclude)) {
						if (f.CopyValue(source, target)) { changed = true; }
					}
				}
				type = type.BaseType;
			}
			return changed;
		}
		public static bool Import(this object target, object source) {
			return Import(target, source,
				BindingFlags.Instance | BindingFlags.NonPublic);
		}

		/// <summary>
		/// Are given objects equal when using type specific equality checks
		/// </summary>
		internal static bool IsEqual(object x, object y, System.Type t) {
			// if only one object is null then they are not equal
			if ((x == null && y != null) || (x != null && y == null)) { return false; }
			// if both are null then consider equal
			if (x == null && y == null) { return true; }
			// if both objects are empty arrays then consider equal
			if (t.IsSubclassOf(typeof(Array))) {
				if (EqualCount(t, x, y, 0, "Length")) { return true; }
			} else if (t.ImplementsInterface(typeof(System.Collections.ICollection))) {
				if (EqualCount(t, x, y, 0, "Count")) { return true; }
			}
			MethodInfo equals = t.GetMethod("Equals", new System.Type[] { t });

			if (equals != null) {
				// call type-specific equality check
				return (bool)equals.Invoke(x, new object[] { y });
			} else {
				// call generic object equality check
				return x.Equals(y);
			}
		}
		public static bool IsEqual(object x, object y) {
			return IsEqual(x, y, x.GetType());
		}

		/// <summary>
		/// Do the two collections or arrays both have a specific member count
		/// </summary>
		public static bool EqualCount(System.Type t, object x, object y, int equalTo, string method) {
			PropertyInfo p = t.GetProperty(method, new System.Type[] { });
			if (p != null) {
				if ((int)p.GetValue(x, null) == equalTo &&
					(int)p.GetValue(y, null) == equalTo) {

					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Does object implement the given interface
		/// </summary>
		public static bool ImplementsInterface(this object o, Type i) {
			if (o != null) {
				Type t = o.GetType();
				return t.ImplementsInterface(i);
			}
			return false;
		}

		/// <summary>
		/// Convert basic .NET types to JavaScript Object Notation
		/// </summary>
		public static string ToJSON(this object value) {
			StringBuilder script;

			if (value == null) {
				return Web.EcmaScript.Null;
			} else if (value is string) {
				string text = value.ToString();
				if (string.IsNullOrEmpty(text)) {
					return Web.EcmaScript.Null;
				} else {
					return string.Format("\"{0}\"", text.EscapeForScript());
				}
			} else if (value is bool) {
				return value.ToString().ToLower();
			} else if (value is int) {
				return value.ToString();
			} else if (value is string[]) {
				script = new StringBuilder();
				string[] list = (string[])value;
				script.Append("[");
				for (int x = 0; x < list.Length; x++) {
					if (x > 0) { script.Append(","); }
					if (string.IsNullOrEmpty(list[x])) {
						script.Append(Web.EcmaScript.Null);
					} else {
						script.AppendFormat("\"{0}\"", list[x].EscapeForScript());
					}
				}
				script.Append("]");
				return script.ToString();

			} else if (value is int[]) {
				script = new StringBuilder();
				int[] list = (int[])value;
				script.Append("[");
				for (int x = 0; x < list.Length; x++) {
					if (x > 0) { script.Append(","); }
					script.Append(list[x]);
				}
				script.Append("]");
				return script.ToString();

			} else if (value is Enum) {
				return ((int)value).ToString();
			} else if (value is Type) {
				return ((Type)value).FullName;
			} else if (value is DateTime) {
				return string.Format("'{0}'", value.ToString());
			} else if (value.ImplementsInterface(typeof(Web.IScriptable))) {
				return ((Web.IScriptable)value).ToJSON();
			}
			return string.Empty;
		}
	}
}
