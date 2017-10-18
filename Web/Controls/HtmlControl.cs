using System;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Base class for HTML controls
	/// </summary>
	public abstract class HtmlControl : System.Web.UI.HtmlControls.HtmlControl,
		IEquatable<HtmlControl> {

		private Idaho.Web.Page _hostPage = null;
		private bool _useCache = false;
		private HttpContext _context = null;
		private Profile _profile = null;

		#region Properties

		protected new HttpContext Context { get { return _context; } }
		public bool UseCache { get { return _useCache; } set { _useCache = value; } }
		public new Idaho.Web.Page Page {
			get {
				if (_hostPage == null) { _hostPage = (Idaho.Web.Page)base.Page; }
				return _hostPage;
			}
			set { _hostPage = value; }
		}
		public string CssClass {
			set { this.Attributes.Add("class", value); }
			get { return this.Attributes["class"]; }
		}
		/// <summary>
		/// Shorter reference to the CSS class
		/// </summary>
		public string Class { set { this.CssClass = value; } }
		public Profile Profile {
			get {
				if (_profile == null) {
					if (_hostPage != null) { _profile = _hostPage.Profile; }
					else { _profile = Profile.Load(_context); }
				}
				return _profile;
			}
		}
		public bool HasStyle {
			get {
				return (this.Attributes.CssStyle.Count > 0
					|| !string.IsNullOrEmpty(this.Attributes["class"]));
			}
		}

		public string CssStyle {
			set { this.Attributes.CssStyle.Value = value; }
			get { return this.Attributes.CssStyle.Value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Context needs to be provided if called on a custom thread
		/// </summary>
		public HtmlControl(HttpContext context) { _context = context; }
		public HtmlControl() { _context = base.Context; }

		#endregion

		#region Events

		/// <summary>
		/// Script to run when mouse clicks element
		/// </summary>
		public string OnClick {
			get { return base.Attributes["onclick"]; }
			set { base.Attributes.Add("onclick", this.CleanScript(value)); }
		}
		/// <summary>
		/// Script to run when element loses focus
		/// </summary>
		public string OnBlur {
			set { base.Attributes.Add("onblur", this.CleanScript(value)); }
		}
		/// <summary>
		/// Script to run when field element changes value
		/// </summary>
		public string OnChange {
			set { base.Attributes.Add("onchange", this.CleanScript(value)); }
		}
		/// <summary>
		/// Script to run when field element is focused (clicked)
		/// </summary>
		public string OnFocus {
			set { base.Attributes.Add("onfocus", this.CleanScript(value)); }
		}
		/// <summary>
		/// Script to run when mouse moves over element
		/// </summary>
		public string OnMouseOver {
			set { base.Attributes.Add("onmouseover", this.CleanScript(value)); }
		}
		/// <summary>
		/// Script to run when mouse moves off of element
		/// </summary>
		public string OnMouseOut {
			set { base.Attributes.Add("onmouseout", this.CleanScript(value)); }
		}
		internal void OnInit() { this.OnInit(new EventArgs()); }
		internal void OnLoad() { this.OnLoad(new EventArgs()); }
		internal void OnPreRender() { this.OnPreRender(new EventArgs()); }

		public void ParentInitialized(object sender, EventArgs e) {
			this.OnInit(e);
		}
		public void ParentLoaded(object sender, EventArgs e) {
			this.OnLoad(e);
		}
		public void ParentPreRendered(object sender, EventArgs e) {
			this.OnPreRender(e);
		}

		/// <summary>
		/// Normalize the script string
		/// </summary>
		private string CleanScript(string script) {
			if (!string.IsNullOrEmpty(script) && !script.EndsWith(";")) {
				script += ";";
			}
			return script;
		}

		#endregion

		protected void ClearStyle() {
			this.Attributes.Remove("class");
			base.Style.Clear();
		}

		/// <summary>
		/// Render tag attribuates
		/// </summary>
		/// <remarks>
		/// Override so that ID is written without prepending parent control names
		/// </remarks>
		protected override void RenderAttributes(HtmlTextWriter writer) {
			//base.RenderAttributes(writer); return;
			writer.Write(" id=\"");
			writer.Write(base.ID);
			writer.Write("\" name=\"");
			writer.Write(base.UniqueID);
			writer.Write("\"");
			foreach (string key in base.Attributes.Keys) {
				writer.Write(" ");
				writer.Write(key);
				writer.Write("=\"");
				writer.Write(base.Attributes[key]);
				writer.Write("\"");
			}
		}
		/// <summary>
		/// Render CSS information for tag
		/// </summary>
		protected void RenderStyle(HtmlTextWriter writer) {
			if (base.Style.Count > 0) {
				writer.Write(" style=\"");
				writer.Write(base.Style.Value);
				writer.Write("\"");
			}
		}

		public bool Equals(HtmlControl other) { return this.ID.Equals(other.ID); }
	}
}