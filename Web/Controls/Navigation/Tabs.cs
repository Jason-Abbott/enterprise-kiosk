using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Xml;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class Tabs : Idaho.Web.Controls.Navigation {

		private string _activeStyle = "background: #eee; border-bottom: 1px solid #eee; color: #47a;";
		public string ActiveStyle { set { _activeStyle = value; } }

		/// <summary>
		/// Write style rules to highlight active tab
		/// </summary>
		private void Tabs_Load(object sender, System.EventArgs e) {
			this.Page.StyleBlock = string.Format(
				"ul.tabs li#{0} a {{ {1} }}", this.Page.FileName, _activeStyle);
		}
		private void Tabs_PreRender(object sender, System.EventArgs e) {
			//Me.DefaultFileName = AppSettings("MenuStore")
			this.MaximumDepth = 2;
		}

		protected override void RenderNodes(NavigationNodeCollection nodes, HtmlTextWriter writer) {
			writer.Write("<ul class=\"tabs");
			writer.Write((nodes.Depth == 1 ? " top" : " child"));
			writer.Write("\">");
			foreach (NavigationNode node in nodes) {
				writer.Write("<li");
				if (node.Url != null) {
					writer.Write(" id=\"");
					writer.Write(Utility.FileName(node.Url));
					writer.Write("\"><a href=\"");
					writer.Write(HttpUtility.HtmlEncode(node.Url));
				} else {
					writer.Write("><a href=\"");
				}
				writer.Write("\">");
				writer.Write(node.Title);
				writer.Write("</a>");
				if (node.HasChildNodes()) { this.RenderNodes(node.ChildNodes, writer); }
				writer.Write("</li>");
			}
			writer.Write("</ul>");
		}
	}
}