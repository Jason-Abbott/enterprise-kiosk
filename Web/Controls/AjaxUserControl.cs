using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public abstract class AjaxUserControl : UserControl, IAjaxControl {

		private AjaxBase _ajax = null;
		
		#region Constructors

		/// <summary>
		/// Context needs to be provided if called on a custom thread
		/// </summary>
		public AjaxUserControl(HttpContext context) {
			Assert.NoNull(context, "NullContext");
			_ajax = new AjaxBase(this, context);
		}
		public AjaxUserControl() : this(HttpContext.Current) { }

		#endregion

		#region Events

		protected override void OnInit(EventArgs e) {
			_ajax.Initialize();
			base.OnInit(e);
		}
		protected override void OnLoad(EventArgs e) {
			_ajax.EventsCompleted = true;
			if (this.Visible && _ajax.NeedLoadScript) {
				this.Page.ScriptBlock = _ajax.BuildLoadScript();
			}
			base.OnLoad(e);
		}
		protected new void PreRender() { _ajax.PreRender(); }

		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);
		}
		protected override void RenderChildren(HtmlTextWriter writer) {
			base.RenderChildren(writer);
		}

		#endregion

		#region Rendering

		protected override void Render(HtmlTextWriter writer) {
			if (_ajax.IsRenderingContent) {
				if (!_ajax.EventsCompleted) { _ajax.PreRender(); }
				this.RenderContent(writer);
			} else {
				_ajax.RenderPlaceHolder(writer);
			}
		}
		/// <summary>
		/// Override to customize content
		/// </summary>
		protected virtual void RenderContent(HtmlTextWriter writer) {
			this.RenderBase(writer);
		}
		/// <summary>
		/// Invoke the render method of the base user control
		/// </summary>
		protected void RenderBase(HtmlTextWriter writer) {
			base.Render(writer);
		}
		protected void RenderPlaceHolder(HtmlTextWriter writer) {
			_ajax.RenderPlaceHolder(writer);
		}

		#endregion

		#region IAjaxControl

		public AjaxBase Ajax { get { return _ajax; } }
		public new void RenderControl(HtmlTextWriter writer) { this.Render(writer); }
		// these properties allow declarative setting by reflection
		public AjaxBase.RenderModes RenderMode { set { _ajax.RenderMode = value; } }
		public string BindProperty { set { _ajax.BindProperty = value; } }
		public string RefreshFor { set { _ajax.RefreshFor = value; } }

		#endregion

	}
}
