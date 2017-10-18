using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace Idaho.Web.Ajax {

	public class Response {
		private HttpContext _context;
		private List<string> _errors = new List<string>();
		private static CacheCollection _cache = new CacheCollection();
		const string _quote = "\"";
		const string _default = "\"\"";
		private bool _cacheable = false;
		private string _cacheKey = string.Empty;
		private List<Type> _cacheDependencies;

		#region Properties

		protected internal HttpContext Context { get { return _context; } }

		/// <summary>
		/// EntityCollection types that the output control depends on
		/// </summary>
		internal List<Type> CacheDependencies { set { _cacheDependencies = value; } }

		internal bool Cacheable { set { _cacheable = value; } }
		internal string CacheKey { set { _cacheKey = value; } }

		/// <remarks>
		/// All items posted as form fields through xmlHttpRequest are treated
		/// as parameters to the target .NET method or type.
		/// </remarks>
		internal System.Collections.Specialized.NameValueCollection Parameters {
			get { return _context.Request.Form; }
		}
		/// <summary>
		/// Cached responses
		/// </summary>
		public static CacheCollection Cache { get { return _cache; } }

		#endregion

		internal Response(HttpContext context) { _context = context; }

		/// <summary>
		/// Set IAsyncResult complete and build JSON response
		/// </summary>
		/// <remarks>
		/// Firefox handles an XML literal but IE does not so it is necessary to escape
		/// the HTML strings rather than treat them as XML.
		/// </remarks>
		/// <param name="isHtml">
		/// Inidicate of value is HTML to be escaped or an EcmaScript literal that won't
		/// be escaped.
		/// </param>
		protected internal void Complete(string value, bool isHtml) {

			StringBuilder result = new StringBuilder();
			bool compressed = false;
			byte[] output;
			int count = 0;

			if (string.IsNullOrEmpty(value)) {
				value = _default;
			} else if (isHtml) {
				value = _quote + value.EscapeForScript() + _quote;
			}
			result.Append("response={Errors:[");
			foreach (string e in _errors) {
				if (count > 0) { result.Append(","); }
				result.AppendFormat("{0}{1}{0}", _quote, e.EscapeForScript());
				count++;
			}
			result.AppendFormat("], IsHtml:{0}, Value:{1}}};", isHtml.ToString().ToLower(), value);

			if (isHtml && Handlers.Resource.AllowHttpCompression) {
				output = Utility.Compress(result.ToString());
				compressed = true;
			} else {
				output = Encoding.UTF8.GetBytes(result.ToString());
			}
			if (_cacheable && !string.IsNullOrEmpty(_cacheKey)) {
				_cache.Add(_cacheKey, output, compressed, _cacheDependencies);
			}
			this.WriteBytes(output, compressed);
		}

		protected internal void Complete(string value) { this.Complete(value, false); }

		/// <summary>
		/// Write response from byte array
		/// </summary>
		protected internal void WriteBytes(byte[] value, bool compressed) {
			if (compressed) { _context.Response.AddHeader("Content-encoding", "gzip"); }
			_context.Response.ContentType = "text/plain";
			_context.Response.OutputStream.Write(value, 0, value.Length);
		}

		/// <summary>
		/// Write the cached value
		/// </summary>
		protected internal bool WriteCache(string key) {
			CacheItem i = _cache[key];

			if (i != null) {
				this.WriteBytes(i.Value, i.Compressed);
				return true;
			} else {
				return false;
			}
		}

		protected internal void Abort() { this.Complete(string.Empty, false); }

		internal void AddError(string message) { _errors.Add(message); }
		internal void AddError(string message, string value1) { _errors.Add(string.Format(message, value1)); }
		internal void AddError(string message, string value1, string value2) {
			_errors.Add(string.Format(message, value1, value2));
		}
		internal void Error(System.Exception ex) {
			_errors.Add(ex.Message);
			while (ex.InnerException != null) {
				ex = ex.InnerException;
				_errors.Add(ex.Message);
			}
		}
	}
}