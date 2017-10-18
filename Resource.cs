using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace Idaho {
	public class Resource {
		private static List<ResourceManager> _managers = new List<ResourceManager>();
		private static System.Globalization.CultureInfo _culture;
		private static Dictionary<string, string> _cache = new Dictionary<string, string>();

		/// <summary>
		/// Add resource reference from given assembly, assuming default namespaces
		/// </summary>
		public static void Add(Assembly assembly) {
			string[] resources = assembly.GetManifestResourceNames();
			if (resources != null && resources.Length > 0) {
				foreach (string name in resources) {
					if (name.EndsWith(".resources")) {
						string ns = name.Replace(".resources", "");
						// managers are considered LIFO so add new managers to start
						// of list so that they are considered first
						_managers.Insert(0, new ResourceManager(ns, assembly));
					}
				}
			} else {
				throw new NullReferenceException("No resources found in " + assembly.FullName);
			}
		}

		private static string GetString(string key) {
			if (!_cache.ContainsKey(key)) {
				if (_managers.Count == 0) {
					throw new NullReferenceException("No resource managers have been defined");
				}
				string value = string.Empty;
				bool found = false;

				foreach (ResourceManager m in _managers) {
					value = m.GetString(key);
					if (!string.IsNullOrEmpty(value)) {
						_cache.Add(key, value);
						found = true;
						break;
					}
				}
				// simplify subsequent searches even if there's no match
				if (!found) { _cache.Add(key, string.Empty); }
			}
			return _cache[key];
		}

		static System.Globalization.CultureInfo Culture {
			get { return _culture; }
			set { _culture = value; }
		}
		/// <summary>
		/// Retrieve the resource with the given key
		/// </summary>
		public static string Say(string key) { return GetString(key); }

		/// <summary>
		/// Format the key to retrieve a resource
		/// </summary>
		public static string Say(string key, params string[] value) {
			return GetString(string.Format(key, value));
		}

		/// <summary>
		/// Retrieve a resource then apply formatting to it
		/// </summary>
		public static string SayFormat(string key, params object[] value) {
			return string.Format(GetString(key), value);
		}
	}
}
