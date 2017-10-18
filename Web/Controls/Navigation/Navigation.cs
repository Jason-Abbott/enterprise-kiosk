using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.Caching;
using System.Xml;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Base class for tabs and menus that use sitemap XML source
	/// </summary>
	public abstract class Navigation : Idaho.Web.Controls.HtmlControl {

		private string _fileName = string.Empty;
		private string _defaultFileName = string.Empty;
		private string _html = string.Empty;
		private int _maximumDepth = 0;
		private string _cacheKey = string.Empty;

#region Properties

		protected Int32 MaximumDepth {
			get { return _maximumDepth; }
			set { _maximumDepth = value; }
		}
		protected string DefaultFile { set { _defaultFileName = value; } }
		public string File { set { _fileName = value; } }

#endregion

		public Navigation() { }
		protected Navigation(HttpContext context) : base(context) { }

		protected override void Render(HtmlTextWriter writer) {
			if (_fileName == string.Empty) { _fileName = _defaultFileName; }
			if (_fileName == string.Empty) {
				throw new System.Exception("No file specified for " + this.ID);
			}
			//_cacheKey = String.Format("{0}{1}", Me.User.Permissions, _fileName)
			_cacheKey = _fileName;
			_html = (string)this.Context.Cache[_cacheKey];

			if (_html == string.Empty) {
				XmlNodeList nodes;
				Idaho.Data.Xml xml = new Idaho.Data.Xml(this.Context);
				StringBuilder sb = new StringBuilder();
				StringWriter sw = new StringWriter(sb);
				HtmlTextWriter tw = new HtmlTextWriter(sw);

				xml.UseCache = false;
				nodes = xml.GetNodes(_fileName, "siteMap/siteMapNode");
				this.RenderNodes(new NavigationNodeCollection(nodes), tw);
				_html = sb.ToString();

				this.Context.Cache.Insert(_cacheKey, _html,
					new CacheDependency(this.Context.Server.MapPath(_fileName)));
			}
			writer.Write(_html);
		}

		protected abstract void RenderNodes(NavigationNodeCollection nodes, HtmlTextWriter writer);

	}
}