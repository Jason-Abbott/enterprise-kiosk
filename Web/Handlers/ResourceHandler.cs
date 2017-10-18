using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Web;

namespace Idaho.Web.Handlers {
	/// <summary>
	/// Custom handler since built-in handler can't infer MIME and assembly
	/// </summary>
	public class Resource : IHttpHandler {
	   
		// assembly age in ticks
		private static Int64 _age;
		private static Assembly _assembly;
		private static string _nameSpace = string.Empty;
		private static string _path = "Web.Resources";
		private static bool _allowMinify = true;
		private static bool _allowZip = true;
		private bool _canMinify = false;
		private bool _canZip = true;

		/// <summary>
		/// Indicate whether EcmaScript minification should be applied
		/// </summary>
		public static bool AllowMinify {
			set { _allowMinify = value; }
			internal get { return _allowMinify; }
		}

		/// <summary>
		/// Indicate whether to GZIP output stream
		/// </summary>
		public static bool AllowHttpCompression {
			set { _allowZip = value; }
			internal get { return _allowZip; }
		}

		static Resource() {
			_assembly = Assembly.GetExecutingAssembly();
			_nameSpace = _assembly.FullName.Substring(0, _assembly.FullName.IndexOf('.'));
			FileInfo file = new FileInfo(_assembly.Location);
			_age = file.LastWriteTime.Ticks;
			//_age = 3;
		}

		/// <summary>
		/// Get the URL for a given resource
		/// </summary>
		public static string GetURL(string name) {
			return string.Format("r.axd?f={0}&t={1}", name.Replace('/', '.'), _age);
		}
	   
		private string InferMimeType(string name) {
			string extension = name.Substring(name.LastIndexOf(".") + 1).ToLower();
			switch (extension) {
				case "gif": _canZip = false; return "image/gif";
				case "jpg":
				case "jpeg":
				case "jpe":
					_canZip = false;
					return "image/jpg";
				case "png": _canZip = false; return "image/png";
				case "js":
					if (_allowMinify) { _canMinify = true; }
					return "application/x-javascript";
				case "txt": return "text/plain";
				case "css": return "text/css";
				case "htm":
				case "html":
					return "text/html";
				default: return string.Empty;
			}
		}
	   
		#region IHttpHandler
	   
		public bool IsReusable { get { return true; } }
	   
		public void ProcessRequest(HttpContext context) {
			string name = context.Request.QueryString["f"];
			System.IO.Stream resource;
			HttpCachePolicy cache = context.Response.Cache;

			// since the same instance is reused, reset these defaults
			_canZip = true; _canMinify = false;

			if (name == null) { return; }
		   
			resource = _assembly.GetManifestResourceStream(
				string.Format("{0}.{1}.{2}", _nameSpace, _path, name));

			if (resource != null) {
				cache.SetCacheability(HttpCacheability.Public);
				cache.VaryByParams["f"] = true;
				cache.VaryByParams["t"] = true;
				cache.SetExpires(DateTime.Now.AddYears(1));
				context.Response.ContentType = this.InferMimeType(name);
			   
				if (_canMinify) {
					EcmaScriptMinify em = new EcmaScriptMinify();
					em.WriteMinified(resource, context.Response.Output);
				} else {
					byte[] buffer = new byte[1025];
					int bytesToWrite = 1;
					Stream output;

					if (_canZip && _allowZip) {
						context.Response.AddHeader("Content-encoding", "gzip");
						output = new GZipStream(context.Response.OutputStream, CompressionMode.Compress);
					} else {
						output = context.Response.OutputStream;
					}
					while (bytesToWrite > 0) {
						bytesToWrite = resource.Read(buffer, 0, 1024);
						output.Write(buffer, 0, bytesToWrite);
					}
					resource.Dispose();
					output.Dispose();
				}
			}
		}
	   
		#endregion
	   
	}
}