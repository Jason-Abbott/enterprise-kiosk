using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class Link : HtmlContainerControl {

		private bool _preAuthorize = false;
		private string _url = string.Empty;
		private Type _pageType = null;
		private Type _controlType = null;

		#region Properties

		/// <summary>
		/// If specified, url will be EcmaScript to post form with action
		/// </summary>
		public Page.Actions Action {
			set { _url = "javascript:" + Resource.SayFormat("Script_PostAction", (int)value); }
		}
		/// <summary>
		/// If true, will render script to make an AJAX call to validate
		/// authorization prior to linking to page
		/// </summary>
		public bool PreAuthorize { set { _preAuthorize = value; } }

		public string Url { set { _url = value; } }

		public string Resx { set { _url = Resource.Say("URL_{0}", value); } }
		public string ResourceKey { set { this.Resx = value; } }
		
		/// <summary>
		/// The page type to direct to
		/// </summary>
		/// <remarks>
		/// To be used in conjunction with pre-authorization. The AJAX
		/// call needs to reflect against the page type to determine
		/// authorization. The Url can usually be inferred from the type
		/// name.
		/// </remarks>
		public Type PageType {
			set { _pageType = value; _preAuthorize = (_pageType != null); }
		}

		/// <summary>
		/// The control type to load
		/// </summary>
		public Type ControlType { set { _controlType = value; } }

		#endregion

		public Link() : base("a") { }

		public void FormatUrl(string resx, object value1) {
			this.Url = Resource.SayFormat("URL_" + resx, value1);
		}
		public void FormatUrl(string resx, object value1, object value2) {
			this.Url = Resource.SayFormat("URL_" + resx, value1, value2);
		}
		public void FormatUrl(string resx, object value1, object value2, object value3) {
			this.Url = Resource.SayFormat("URL_" + resx, value1, value2, value3);
		}

		protected override void OnPreRender(EventArgs e) {
			if (_preAuthorize) {
				Assert.NoNull(_pageType, "NullPageType");
				base.OnClick = Resource.SayFormat("Script_Redirect", _pageType.Name);
			} else if (_controlType != null) {
				base.OnClick = Resource.SayFormat("Script_ShowDialog",
					_controlType.AssemblyQualifiedName, true.ToString().ToLower());
			} else {
				base.Attributes.Add("href", _url);
			}
			base.OnPreRender(e);
		}
	}
}
