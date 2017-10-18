using Idaho.Web.Controls;
using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Ajax {
	/// <summary>
	/// Build the response for an AJAX control request
	/// </summary>
	/// <remarks>
	/// http://www.codeproject.com/aspnet/ASPNET_11_Compilation.asp
	/// http://somenewkid.blogspot.com/2006/06/implementing-raw-templating-part-1.html
	/// </remarks>
	internal class ControlRequest : Request {

		internal ControlRequest(Response r) : base(r) { }

		/// <summary>
		/// Use reflection to render requested control
		/// </summary>
		internal override void Process() {
			if (!this.Response.WriteCache(this.Key)) {
				string typeName = base["type"];
				StringBuilder sb = new StringBuilder();
				HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(sb));
				IAjaxControl control;
				PropertyInfo[] properties;
				System.Type controlType;

				if (string.IsNullOrEmpty(typeName)) {
					this.Response.AddError("No control specified");
					return;
				}

				// expand abbreviated type names
				typeName = AjaxBase.Expand(typeName);

				// get control type and validate
				if (Handlers.AjaxHandler.KnownType.ContainsKey(typeName)) {
					controlType = Handlers.AjaxHandler.KnownType[typeName];
				} else {
					controlType = System.Type.GetType(typeName);
				}
				if (controlType == null) {
					this.Response.AddError("Control \"{0}\" could not found", typeName);
					return;
				}
				if (!this.ValidType(controlType)) {
					this.Response.AddError("Control \"{0}\" cannot be called asynchronously", typeName);
					return;
				}

				// now that validated, create instance and set properties
				using (Page page = new Page(this.Context)) {
					control = (IAjaxControl)page.LoadControl(controlType, null);
				}
				properties = WebBindable.Properties(controlType);

				try {
					foreach (PropertyInfo p in properties) {
						// only set properties for which a value has been passed
						if (!string.IsNullOrEmpty(this.Parameters[p.Name])) {
							p.SetValue(control, EcmaScript.ToType(
								this.Parameters[p.Name], p.PropertyType, this.Context), null);
						}
					}
					// set ID so generated JavaScript references are equivalent
					if (!string.IsNullOrEmpty(this.Parameters["id"])) {
						control.ID = this.Parameters["id"];
					}
					if (!string.IsNullOrEmpty(this.Parameters["style"])) {
						control.CssStyle = this.Parameters["style"];
					}
					if (!string.IsNullOrEmpty(this.Parameters["cssClass"])) {
						control.CssClass = this.Parameters["cssClass"];
					}
					control.Ajax.Context = this.Context;
					// any control rendered by this method is isolated from
					// the normal page life cycle
					control.Ajax.RenderState = AjaxBase.RenderStates.Isolation;
					control.Ajax.RenderMode = AjaxBase.RenderModes.Asynchronous;
					control.RenderControl(writer);
				} catch (System.Exception ex) {
					Idaho.Exception.Log(ex);
					control.Ajax.Cacheable = false;
					this.Response.Error(ex);
				} finally {
					this.Response.Cacheable = control.Ajax.Cacheable;
					this.Response.CacheDependencies = control.Ajax.DependsOnType;
					this.Response.CacheKey = this.Key;
					this.Response.Complete(sb.ToString(), true);
				}
			}
		}
	}
}
