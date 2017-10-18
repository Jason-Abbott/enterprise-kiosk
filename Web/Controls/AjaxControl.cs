using Idaho.Attributes;
using Idaho.Web.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Inherit from this class to enable several asynchronous (AJAX) patterns
	/// without writing any script
	/// </summary>
	/// <remarks>
	/// When used asynchronously, the control can be in two states:
	/// 1) in the page control heirarchy, rendering normally, or 2) being
	/// invoked out-of-band, in isolation, with no page context.
	/// </remarks>
	public abstract class AjaxControl : HtmlControl, IAjaxControl {

		private AjaxBase _ajax = null;

		#region Constructors

		/// <summary>
		/// Context needs to be provided if called on a custom thread
		/// </summary>
		public AjaxControl(HttpContext context) {
			Assert.NoNull(context, "NullContext");
			_ajax = new AjaxBase(this, context);
		}
		public AjaxControl() : this(HttpContext.Current) { }

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

		#endregion

		#region Rendering

		protected override sealed void Render(HtmlTextWriter writer) {
			if (_ajax.IsRenderingContent) {
				if (!_ajax.EventsCompleted) { _ajax.PreRender(); }
				if (this.Visible) { this.RenderContent(writer); }
			} else {
				_ajax.RenderPlaceHolder(writer);
			}
		}
		/// <summary>
		/// Invoke the render method of the base HTML control
		/// </summary>
		protected void RenderBase(HtmlTextWriter writer) {
			base.Render(writer);
		}
		/// <summary>
		/// Appeareance of control when rendered inline
		/// </summary>
		protected abstract void RenderContent(HtmlTextWriter writer);

		#endregion

		#region IAjaxControl

		public AjaxBase Ajax { get { return _ajax; } }
		public override void RenderControl(HtmlTextWriter writer) {
			base.RenderControl(writer);
		}
		// these properties allow declarative setting by reflection
		public AjaxBase.RenderModes RenderMode { set { _ajax.RenderMode = value; } }
		public string BindProperty { set { _ajax.BindProperty = value; } }

		#endregion
		
	}
}
