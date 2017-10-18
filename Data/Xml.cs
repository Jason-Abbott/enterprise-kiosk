using System;
using System.IO;
using System.Xml;
using System.Web;
using System.Web.Caching;

namespace Idaho.Data {
	public class Xml {
		private bool _useCache = true;
		private System.Web.HttpContext _context;
	   
		/// <summary>
		/// Construct object to manage XML repositories
		/// </summary>
		/// <param name="context">HttpContext</param>
		/// <remarks>
		/// Context is passed to provide access to request, response, server,
		/// cookie, etc., objects. Normally it's accessible as a static method
		/// but since the Xml class can be created by a spawned thread, the
		/// static member may be null, so pass context explicitly.
		/// </remarks>
		public Xml(System.Web.HttpContext context) { _context = context; }

		/// <summary>
		/// Should nodes loaded from file be cached
		/// </summary>
		public bool UseCache { set { _useCache = value; } }

		/// <summary>
		/// Load node list from cache or file system
		/// </summary>
		public XmlNodeList GetNodes(FileInfo file, string xPath) {
			XmlNodeList nodes = null;
			string cacheKey = file.FullName;

			if (_useCache) { nodes = (XmlNodeList)_context.Cache.Get(cacheKey); }

			if (nodes == null && file.Exists) {
				XmlDocument xml = new XmlDocument();

				xml.Load(file.FullName);
				nodes = xml.SelectNodes(xPath);

				if (_useCache) {
					// put content in cache
					CacheDependency dependsOn = new CacheDependency(file.FullName);
					_context.Cache.Insert(cacheKey, nodes, dependsOn);
				}
			}
			return nodes;
		}
		public XmlNodeList GetNodes(string fullPath, string xPath) {
			return this.GetNodes(new FileInfo(fullPath), xPath);
		}

	}
}
