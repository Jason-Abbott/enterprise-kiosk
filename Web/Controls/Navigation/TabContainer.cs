using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Container for multi-step forms
	/// </summary>
	public class TabContainer : HtmlContainerControl {

		private string _label = string.Empty;
		private List<Tab> _tabs = new List<Tab>();

		#region Properties

		public string Label { set { _label = value; } get { return _label; } }

		#endregion

		public TabContainer() : base("fieldset") {
			base.AllowSelfClose = true;
		}

		#region Events

		protected override void OnInit(EventArgs e) {
			base.OnInit(e);
		}
		protected override void OnLoad(EventArgs e) {
			// find tabs
			foreach (Control c in this.Controls) {
				if (c is Tab) { _tabs.Add((Tab)c); }
			}
			this.Class = "tabs";
			//this.EnableScriptSupport();
			if (string.IsNullOrEmpty(_label)) { base.TagName = "div"; }
			base.OnLoad(e);
		}

		/// <summary>
		/// Add final formatting to controls
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			HtmlContainerControl list = new HtmlContainerControl("ul");
			HtmlContainerControl fieldset = new HtmlContainerControl("fieldset");
			HtmlContainerControl item;
			string url = this.Context.Request.Url.PathAndQuery;
			bool select = true;

			list.CssClass = "tabClick";
			list.ID = this.ID + "_list";

			this.Controls.AddAt(0, fieldset);
			this.Controls.AddAt(0, list);

			// legend for container
			if (!string.IsNullOrEmpty(_label)) {
				HtmlContainerControl legend = new HtmlContainerControl("legend");
				legend.InnerHtml = _label;
				this.Controls.AddAt(0, legend);
			}

			// tab selector
			foreach (Tab t in _tabs) {
				if (t.Visible) {
					item = new HtmlContainerControl("li");
					item.InnerHtml = t.Label;
					item.ID = t.ID + "_click";

					if (select) {
						item.CssClass = "selected";
						t.CssClass += " selected";
						select = false;
					}
					list.Controls.Add(item);
					fieldset.Controls.Add(t);
				}
			}
			base.OnPreRender(e);
		}

		#endregion

		/// <summary>
		/// Render script elements to support tab switching
		/// </summary>
		/// <returns>Script string for final script object initialization</returns>
		public string EnableScriptSupport(Idaho.Web.Page page) {
			page.ScriptFile.Add("navigation");
			page.ScriptVariable(this.ID, null);
			return string.Format(
				"{0} = new TabsObject('{0}'); {0}.OnInit();", this.ID);
		}
	}
}
