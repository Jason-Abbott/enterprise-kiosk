using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Controls {
	/// <summary>
	/// A dialog box
	/// </summary>
	/// <typeparam name="T">Type of Entity ID</typeparam>
	public class Dialog<T> : AjaxUserControl {

		private string _cssClass = "dialog";
		private string _title = string.Empty;
		private bool _modal = true;
		private bool _showTitle = false;
		private string _extraCss = string.Empty;
		private Image _closeButton = null;
		private T _entityID;
		private const string _entityField = "EntityID";

		#region Properties

		/// <summary>
		/// Title to display at the top of the dialog
		/// </summary>
		[WebBindable]
		public string Title { set { _title = value; } get { return _title; } }

		[WebBindable]
		public bool Modal { set { _modal = value; } get { return _modal; } }

		/// <summary>
		/// ID for entity this dialog displays
		/// </summary>
		[WebBindable]
		public T EntityID { set { _entityID = value; } get { return _entityID; } }

		/// <summary>
		/// ID that will be used for bound hidden field, if any
		/// </summary>
		public string HiddenFieldID { get { return this.ID + _entityField; } }

		#endregion

		#region Events

		/// <summary>
		/// Create a hidden field and bind to it for the entity ID auto-update
		/// </summary>
		protected override void OnInit(EventArgs e) {
			base.OnInit(e);
			// render state is set by base initialization so it must precede
			// this condition
			if (this.Ajax.IsRenderState(AjaxBase.RenderStates.InPage)) {
				this.Ajax.HiddenField.Add(_entityField, string.Empty);
				this.Ajax.Bindings.Add(_entityField, this.HiddenFieldID);
				this.Ajax.ShowWaitCursor = true;
				this.Ajax.RenderMode = AjaxBase.RenderModes.Asynchronous;
				this.Ajax.EnableDrag = true;
			}
			// these values are needed both in page and isolation
			this.Ajax.TitleBarTagID = this.ID + "TitleBar";
			this.Ajax.ContainerTagID = this.ID + "Container";
		}
		protected override void OnLoad(EventArgs e) {
			if (this.Ajax.IsRenderState(AjaxBase.RenderStates.InPage) &&
				this.Ajax.IsRenderMode(AjaxBase.RenderModes.Asynchronous)) {

				this.Ajax.AfterRender = string.Format("Page.Modal({0})", _modal.ToJSON());
				this.Ajax.AfterRender = Resource.SayFormat("Script_KeyPressFunction",
					this.ID + ".Fade", (int)Idaho.KeyCode.Escape);
			}
			base.OnLoad(e);
		}
		protected override void OnPreRender(EventArgs e) {
			_showTitle = !string.IsNullOrEmpty(_title);
			if (_showTitle) {
				_closeButton = new Image();
				_closeButton.Src = "/images/icons/close-button.png";
				_closeButton.Transparency = true;
				_closeButton.ID = "close" + this.ID;
				_closeButton.CssClass = "closeButton";
				_closeButton.IsMouseOver = true;
				_closeButton.OnClick = string.Format("{0}.Hide()", this.ID);
				_closeButton.AlternateText = "Close (Esc)";
			}
			base.OnPreRender(e);
		}

		#endregion

		#region Rendering

		protected override void RenderContent(HtmlTextWriter writer) {
			this.RenderBeginTag(writer);
			this.RenderBase(writer);
			this.RenderEndTag(writer);
		}

		protected void RenderBeginTag(HtmlTextWriter writer) {
			if (_showTitle) { _extraCss = "Title"; }

			writer.Write("<table class=\"{0}\" ", _cssClass);
			//base.RenderAttributes(writer);
			writer.Write("id=\"{0}\" ", this.Ajax.ContainerTagID);
			writer.Write("cellspacing=\"0\" cellpadding=\"0\" border=\"0\">");
			// pre-row (to establish cell widths)
			writer.Write("<tr><td class=\"shadow\"></td><td class=\"edge\"></td>");
			writer.Write("<td></td><td class=\"edge\"></td><td class=\"shadow\"></td>");
			// first row
			writer.Write("</tr><tr>");
			writer.Write("<td colspan=\"2\" rowspan=\"2\" class=\"{0}TL\"></td>", _cssClass + _extraCss);
			writer.Write("<td class=\"shadowTop\"></td>");
			writer.Write("<td colspan=\"2\" rowspan=\"2\" class=\"{0}TR\"></td>", _cssClass + _extraCss);
			// second row
			writer.Write("</tr><tr>");
			if (_showTitle) {
				// give title an ID to simplify script events
				writer.Write("<td id=\"{0}\" ", this.Ajax.TitleBarTagID);
				writer.Write("class=\"{0}Top\">{1}", _cssClass + _extraCss, _title);
				_closeButton.RenderControl(writer);
				writer.Write("</td>");
			} else {
				writer.Write("<td class=\"{0}Top\"></td>", _cssClass);
			}
			// third row (content)
			writer.Write("</tr><tr>");
			writer.Write("<td class=\"shadowLeft\"></td>");
			writer.Write("<td colspan=\"3\" class=\"{0}Content\">", _cssClass);

		}
		protected void RenderEndTag(HtmlTextWriter writer) {
			// complete third row
			writer.Write("</td><td class=\"shadowRight\"></td>");
			// fourth row
			writer.Write("</tr><tr>");
			writer.Write("<td colspan=\"2\" rowspan=\"2\" class=\"{0}BL\"></td>", _cssClass);
			writer.Write("<td class=\"{0}Bottom\"></td>", _cssClass);
			writer.Write("<td colspan=\"2\" rowspan=\"2\" class=\"{0}BR\"></td>", _cssClass);
			// fifth row
			writer.Write("</tr><tr>");
			writer.Write("<td class=\"shadowBottom\"></td>");
			// end
			writer.Write("</tr></table>");
		}

		#endregion
	}
}
