using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Web;

namespace Idaho.Web.Modules {
	class Compression : IHttpModule {
		
		void BeginRequest(object sender, EventArgs e) {
			HttpApplication app = (HttpApplication)sender;
			string encodings = app.Request.Headers.Get("Accept-Encoding");

			if (encodings == null) { return; }

			Stream stream = app.Response.Filter;

			encodings = encodings.ToLower();

			if (encodings.Contains("gzip")) {
				app.Response.Filter = new GZipStream(stream, CompressionMode.Compress);
				app.Response.AppendHeader("Content-Encoding", "gzip");
			} else if (encodings.Contains("deflate")) {
				app.Response.Filter = new DeflateStream(stream, CompressionMode.Compress);
				app.Response.AppendHeader("Content-Encoding", "deflate");
			}
		}

		#region IHttpModule

		void IHttpModule.Init(HttpApplication context) {
			context.BeginRequest += new EventHandler(BeginRequest);
		}

		public void Dispose() {	}

		#endregion

	}
}
