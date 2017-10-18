using Idaho.Data;
using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.Caching;

namespace Idaho.Web.Controls {
	public class Content : Idaho.Web.Controls.HtmlControl {

		private string _fileName = string.Empty;
		private string _content = string.Empty;
		private bool _needsFormatted = false;
		private int _maxFiles = 0;
		private bool _useParagraphs = false;

		public bool UseParagraphs { set { _useParagraphs = value; } }
		public int MaxFiles { set { _maxFiles = value; } }
		public string File { set { _fileName = value; } }

		#region Static Fields

		private static DirectoryInfo _folder ;
		public static DirectoryInfo Folder { set { _folder = value; } }

		#endregion

		public Content() { this.CssClass = "content"; }

		/// <summary>
		/// retrieve content
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			this.Fill();
			base.OnLoad(e);
		}

		/// <summary>
		/// Fill control with indicated content
		/// </summary>
		public void Fill() {
			if (string.IsNullOrEmpty(_fileName)) { return; }
			Assert.NoNull(_folder, "NullFolder");
			string path = string.Format("{0}/{1}", _folder.Name, _fileName);

			_fileName = string.Format("{0}{1}\\{2}", 
				HttpRuntime.AppDomainAppPath, _folder, _fileName);

			// retrieve content
			_needsFormatted = _fileName.EndsWith("txt");
			_content = Data.File.Content(_fileName);
			_content = (string)this.Context.Cache[_fileName];
		}

		/// <summary>
		/// Render file content within control
		/// </summary>
		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			if (string.IsNullOrEmpty(_content)) {
				if (_needsFormatted) { _content = Format.ToHtml(_content, _useParagraphs); }
				writer.Write("<div");
				this.RenderAttributes(writer);
				writer.Write(">");
				writer.Write(_content);
				writer.Write("</div>");
			}
		}
	}
}