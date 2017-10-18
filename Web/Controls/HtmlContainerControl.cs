using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class HtmlContainerControl : HtmlControl {
		private string _tagName;
		private string _content = string.Empty;
		private bool _allowSelfClose = true;

		/// <summary>
		/// Allow the tag to render as self-closing if no content
		/// </summary>
		/// <remarks>
		/// This is to accomodate a Mozilla bug that prevents correct DOM parsing
		/// when self-closed tags, especially DIV, are involved. If passed through
		/// xmlHttp, a tag with no content will be treated as self-closed XML. If
		/// false, this will force at least a space character to be rendered as
		/// content.
		/// Known bug 117516:
		/// https://bugs.launchpad.net/ubuntu/+source/firefox/+bug/117516
		/// </remarks>
		public bool AllowSelfClose { set { _allowSelfClose = value; } }
		public new string TagName { get { return _tagName; } protected set { _tagName = value; } }
		public string InnerHtml { set { _content += value; } get { return _content; } }
		public string InnerText { set { _content += value; } get { return _content; } }

		public HtmlContainerControl(string tagName) {
			_tagName = tagName;
		}

		public void Clear() { _content = string.Empty; }

		#region Events

		protected override void OnInit(EventArgs e) {
			base.OnInit(e);
		}

		protected override void Render(HtmlTextWriter writer) {
			this.RenderBeginTag(writer);
			if (!string.IsNullOrEmpty(_content)) {
				writer.Write(_content);
			} else if (!_allowSelfClose && this.Controls.Count == 0) {
				writer.Write(" ");
			}
			base.RenderChildren(writer);
			this.RenderEndTag(writer);
		}

		#endregion

		protected override ControlCollection CreateControlCollection() {
			return new ControlCollection(this);
		}

		protected override void RenderBeginTag(HtmlTextWriter writer) {
			writer.Write("<");
			writer.Write(_tagName);
			base.RenderAttributes(writer);
			writer.Write(">");
		}
		protected virtual void RenderEndTag(HtmlTextWriter writer) {
			writer.Write("</");
			writer.Write(_tagName);
			writer.Write(">");
		}
	}
}
