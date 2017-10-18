using System;
using System.Collections.Generic;
using System.Reflection;

namespace Idaho.Attributes {
	/// <summary>
	/// Indicates that a method should be exposed to asynchronous
	/// invokation through reflection
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class WebInvokable : System.Attribute {

		private static Dictionary<Type, Dictionary<string, MethodInfo>> _methodCache
			= new Dictionary<Type, Dictionary<string, MethodInfo>>();

		/// <summary>
		/// Get invokable method of given name on type
		/// </summary>
		public static MethodInfo GetMethod(Type type, string name) {
			if (_methodCache.ContainsKey(type)
				&& _methodCache[type].ContainsKey(name)) {

				return _methodCache[type][name];
			} else {
				MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
				Type attributeType = typeof(WebInvokable);

				foreach (MethodInfo m in methods) {
					if (m.Name == name && m.GetCustomAttributes(attributeType, false).Length > 0) {
						if (!_methodCache.ContainsKey(type)) {
							_methodCache.Add(type, new Dictionary<string, MethodInfo>());
						}
						_methodCache[type].Add(name, m);
						return m;
					}
				}
				return null;
			}
		}
	
	}
}