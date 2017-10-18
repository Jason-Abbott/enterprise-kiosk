using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Caching;

namespace Idaho.Web.Handlers {
	public abstract class Graphic : IHttpHandler {
	   
		private string _cacheKey;
		private int _width = 5;
		private int _height = 5;
		private Bitmap _bitmap = null;
		// variables for convenience only
		private Cache _cache = null;
		private HttpRequest _request = null;
		private HttpResponse _response = null;
		private static bool _useCache = true;
	   
		#region Properties

		public static bool UseCache { set { _useCache = value; } }
		protected Bitmap Bitmap { get { return _bitmap; } set { _bitmap = value; } }
		protected int Width { get { return _width; } set { _width = value; } }
		protected int Height { get { return _height; } }
		protected HttpResponse Response { get { return _response; } }
		protected HttpRequest Request { get { return _request; } }
		protected Cache Cache { get { return _cache; } }
		protected System.Collections.Specialized.NameValueCollection Query {
			get { return _request.QueryString; } }
	   
		#endregion

		/// <summary>
		/// Setup values common to sub-classes
		/// </summary>
		protected void Initialize() {
			_request = HttpContext.Current.Request;
			_response = HttpContext.Current.Response;

			if (_useCache) {
				_cache = HttpContext.Current.Cache;
				_cacheKey = _request.Url.Query;
			}
			if (Query["w"] != null) { _width = int.Parse(Query["w"]); }
			if (Query["h"] != null) { _height = int.Parse(Query["h"]); }
		}
	   
		protected bool IsCached {
			get {
				if (_useCache && (_cache[_cacheKey] != null)) {
					_bitmap = (Bitmap)_cache[_cacheKey];
					return true;
				} else {
					_bitmap = new Bitmap(_width, _height);
					return false;
				}
			}
		}
	   
		protected abstract void MakeGraphic();

		/// <summary>
		/// Save the image to output
		/// </summary>
		/// <param name="transparency">Does the graphic have an alpha channel</param>
		/// <remarks>
		/// http://www.aspnetresources.com/blog/cache_control_extensions.aspx
		/// http://support.microsoft.com/default.aspx?scid=kb;en-us;323290
		/// </remarks>
		protected void Save(bool transparency) {
			if (_useCache) {
				_response.Cache.SetExpires(DateTime.Now.AddHours(6));
				_response.Cache.SetCacheability(HttpCacheability.Public);
				_response.Cache.SetSlidingExpiration(true);
				_response.Cache.SetMaxAge(new TimeSpan(24, 0, 0));
			}
			if (transparency) {
				MemoryStream memory = new MemoryStream();
				_response.ContentType = "image/png";
				_bitmap.Save(memory, ImageFormat.Png);
				memory.WriteTo(_response.OutputStream);
			} else {
				// PNG gamma support varies by browser so otherwise use JPEG
				_response.ContentType = "image/jpeg";
				_bitmap.Save(_response.OutputStream, ImageFormat.Jpeg);
			}
		}
		protected void OutputCache() { this.Save(true); }

		/// <summary>
		/// Cache image bytes in server memory
		/// </summary>
		protected void ServerCache() {
			if (_useCache) {
				_cache.Add(_cacheKey, _bitmap, null,
					System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.Zero,
					CacheItemPriority.Normal, null);
			}
		}
	   
		#region IHttpHandler

		/// <remarks>
		/// Instance cannot be re-usable because new threads (requests) could then
		/// over-write values set by existing thread before it completes.
		/// </remarks>
		public bool IsReusable { get { return false; } }
		public abstract void ProcessRequest(HttpContext context);
	   
		#endregion
	   
	}
}
