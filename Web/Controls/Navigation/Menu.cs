using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Xml;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Render HTML menu from sitemap
	/// </summary>
	/// <remarks>
	/// Functionality depends on the inclusion of a menu script at the page level.
	/// http://www.htmldog.com/articles/suckerfish/dropdowns/example/
	/// </remarks>
	public class Menu : Idaho.Web.Controls.Navigation {

		private Orientations _orientation = Orientations.Horizontal;
		public enum Orientations { Horizontal, Vertical }
		public Orientations Orientation { set { _orientation = value; } }

		public Menu() {	}
		/// <summary>
		/// When used asynchronously, the context must be provided
		/// </summary>
		public Menu(HttpContext context) : base(context) { }

		private void Menu_PreRender(object sender, System.EventArgs e) {
			//this.DefaultFile = ConfigurationManager.AppSettings["MenuStore"];
		}

		/// <summary>
		/// Recursively render child nodes
		/// </summary>
		/// <param name="nodes"></param>
		/// <param name="leftMargin"></param>
		/// <param name="writer"></param>
		protected void RenderNodes(NavigationNodeCollection nodes, string leftMargin, HtmlTextWriter writer) {
			{
				writer.Write("<ul");
				if (nodes.Depth == 1) { writer.Write(" id=\"nav\""); }
				if (leftMargin != null) {
					writer.Write(" style=\"margin-left: ");
					writer.Write(leftMargin);
					writer.Write(";\"");
				}
				writer.Write(">");
				//.Write("<ul class=""menu")
				//If nodes.Depth = 1 Then
				// .Write(" top")
				// If _orientation = Orientations.Horizontal Then .Write(" horizontal")
				//Else
				// .Write(" child")
				//End If
				//.Write(""">")
				foreach (NavigationNode node in nodes) {
					writer.Write("<li");
					if (node.NewSection)
						writer.Write(" class=\"break\"");
					writer.Write("><a href=\"");
					if (node.Url != null) { writer.Write(HttpUtility.HtmlEncode(node.Url)); }
					writer.Write("\"");
					if ((node.Depth > 1 || _orientation == Orientations.Vertical) && node.HasChildNodes())
						writer.Write("class=\"parent\"");
					if (node.Depth > 1 && node.ParentNode.Width != null) {
						writer.Write(" style=\"width: ");
						writer.Write(node.ParentNode.Width);
						writer.Write(";\"");
						leftMargin = node.ParentNode.Width;
					} else {
						leftMargin = null;
					}
					writer.Write(">");
					writer.Write(node.Title);
					writer.Write("</a>");
					if (node.HasChildNodes())
						this.RenderNodes(node.ChildNodes, leftMargin, writer);
					writer.Write("</li>");
				}
				writer.Write("</ul>");
			}
		}

		protected override void RenderNodes(NavigationNodeCollection nodes, HtmlTextWriter writer) {
			this.RenderNodes(nodes, null, writer);
		}
	}
}