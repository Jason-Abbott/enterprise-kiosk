using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Idaho {
	public static class FieldInfoExtension {

		/// <summary>
		/// Does the field have the given attribute
		/// </summary>
		public static bool HasAttribute(this FieldInfo f, Type a) {
			return (a != null && f != null &&
				f.GetCustomAttributes(a, false).Length > 0);
		}

		public static T GetAttribute<T>(this FieldInfo f) where T:System.Attribute {
			Type a = typeof(T);
			object[] matches = f.GetCustomAttributes(a, false);
			if (matches.Length > 0) {
				return (T)matches[0];
			} else {
				return default(T);
			}
		}

		/// <summary>
		/// Copy value from one field to another
		/// </summary>
		public static bool CopyValue(this FieldInfo f, object source, object target) {
			object s = f.GetValue(source);
			object t = f.GetValue(target);

			if (ObjectExtension.IsEqual(s, t, f.FieldType)) {
				// no need to copy values
				return false;
			} else {
				f.SetValue(target, s);
				return true;
			}
		}
	}
}
