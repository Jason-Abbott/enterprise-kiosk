using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Idaho.Web {
	/// <summary>
	/// Base page
	/// </summary>
	public class Page : System.Web.UI.Page {

		private ClientResourceList _styleSheet = new ClientResourceList("style", "css");
		private ClientResourceList _scriptFile = new ClientResourceList("script", "js");
		private StringBuilder _scriptBlock = new StringBuilder();
		private StringBuilder _styleBlock = new StringBuilder();
		private Profile _profile = null;
		private bool _requireAuthentication = false;
		private bool _fixIeCssPng = false;
		private HttpContext _context;
		private MasterPage _master;
		private List<Controls.Validation> _validations;
		private string _name = string.Empty;
		// item ID
		private Guid _itemID = Guid.Empty;
		private bool _loadedItemID = false;
		private Guid _subItemID = Guid.Empty;
		private bool _loadedSubItemID = false;
		// step
		private string _step = string.Empty;
		private bool _loadedStep = false;
		// action
		private Actions _action = Actions.None;
		private bool _loadedAction = false;
		// fields
		private Web.Controls.Field fldAction;
		private Web.Controls.Field fldStep;
		private Web.Controls.Field fldItemID;
		private Web.Controls.Field fldSubItemID;

		/// <summary>
		/// This page's postback action
		/// </summary>
		[Flags]
		public enum Actions {
			None = 0x0,
			Add = 0x2,
			Update = 0x4,
			Delete = 0x8,
			Next = 0x10,
			Previous = 0x20,
			Finish = 0x40,
			Load = 0x80,
			Restart = 0x100,
			Save = 0x200
		}

		/// <summary>
		/// Result of user attempting page view 
		/// </summary>
		/// <remarks>
		/// Primarily for use by the TryView() method to indicate response
		/// to AJAX call.
		/// </remarks>
		public enum ViewResult { Granted = 1, Denied = 0, NeedLogin = -1 }

		#region Static Fields

		private static string _loginPage = string.Empty;
		public static string LoginPage { set { _loginPage = value; } }

		#endregion

		#region Properties

		/// <summary>
		/// Design theme to use for style sheets and graphics
		/// </summary>
		protected string Theme { set { _styleSheet.Theme = value; } }

		/// <summary>
		/// Set true to include style sheet to fix IE PNG support
		/// </summary>
		protected bool FixIeCssPng { set { _fixIeCssPng = value; } }

		/// <summary>
		/// Store object ID for postbacks
		/// </summary>
		/// <remarks>
		/// If most business entities inherit from Entity then all will have a GUID that
		/// can be held by the page for postbacks or links.
		/// </remarks>
		public Guid ItemID {
			get {
				if (!_loadedItemID) {
					string id = (this.IsPostBack) ? fldItemID.Value : Request.QueryString[fldItemID.ID];
					if (!string.IsNullOrEmpty(id)) {
						Regex re = new Regex(Pattern.Guid);
						if (re.IsMatch(id)) { this.ItemID = new Guid(id); }
					}
					_loadedItemID = true;
				}
				return _itemID;
			}
			set {
				_itemID = value;
				fldItemID.Value = _itemID.ToString();
				_loadedItemID = true;
			}
		}

		/// <summary>
		/// Store sub-object ID for postbacks
		/// </summary>
		/// <remarks>
		/// If most business entities inherit from Entity then all will have a GUID that
		/// can be held by the page for postbacks or links.
		/// </remarks>
		public Guid SubItemID {
			get {
				if (!_loadedSubItemID) {
					string id = (this.IsPostBack) ? fldSubItemID.Value : Request.QueryString[fldSubItemID.ID];
					if (!string.IsNullOrEmpty(id)) {
						Regex re = new Regex(Pattern.Guid);
						if (re.IsMatch(id)) { this.SubItemID = new Guid(id); }
					}
					_loadedSubItemID = true;
				}
				return _subItemID;
			}
			set {
				_subItemID = value;
				fldSubItemID.Value = _subItemID.ToString();
				_loadedSubItemID = true;
			}
		}

		/// <summary>
		/// The identifier of the step field
		/// </summary>
		/// <remarks>
		/// Used by other classes that build or reference the form or query string.
		/// </remarks>
		//protected internal string StepFieldID { get { return fldStep.ID; } }

		/*
		protected internal int Step {
			get {
				if (_step < 0) {
					string step = (this.IsPostBack) ? fldStep.Value : Request.QueryString[this.StepFieldID];
					if (!int.TryParse(step, out _step)) { _step = -1; }
					this.Step = _step;
				}
				return _step;
			}
			set {
				_step = value;
				fldStep.Value = _step.ToString();
			}
		}*/

		/// <summary>
		/// Store step for multi-page flows
		/// </summary>
		/// <remarks>
		/// If using the Step control, this will typicall contain the ID of the step
		/// </remarks>
		/*
		protected internal string Step {
			get {
				if (!_loadedStep) {
					string step = (this.IsPostBack) ? fldStep.Value
						: Request.QueryString[this.StepFieldID];
					_loadedStep = true;
				}
				return _step;
			}
			set {
				_step = value;
				fldStep.Value = _step;
				_loadedStep = true;
			}
		}
		*/
		/// <summary>
		/// The control containing the step that should be rendered
		/// </summary>
		//protected internal Control StepControl {
		//	set { if (value != null) { this.Step = value.ID; } }
		//}

		/// <summary>
		/// Desired action for form post
		/// </summary>
		/// <remarks>
		/// This will usually be controlled by Web.Controls.Button which generates
		/// script to populate a hidden field with the action ID on postback.
		/// 
		/// Some pages set the action by EcmaScript so it may be found in a hidden
		/// field rather than ViewState.
		/// </remarks>
		protected internal Actions Action {
			get {
				if (!_loadedAction) {
					string action = (this.IsPostBack) ? fldAction.Value : Request.QueryString[fldAction.ID];
					int actionID;
					if (int.TryParse(action, out actionID)) {
						_action = (Actions)actionID;
					} else {
						_action = Actions.None;
					}
					this.Action = _action;
				}
				return _action;
			}
			set {
				_action = value;
				fldAction.Value = ((int)_action).ToString();
				_loadedAction = true;
			}
		}

		/// <summary>
		/// A collection of validation control instances
		/// </summary>
		/// <remarks>
		/// These are generated by InputControl subclasses when validation
		/// types are specified.
		/// </remarks>
		internal List<Controls.Validation> Validation {
			get {
				if (_validations == null) { _validations = new List<Controls.Validation>(); }
				return _validations;
			}
		}
		protected MasterPage MasterBase {
			get {
				if (_master == null) { _master = (MasterPage)base.Master; }
				return _master;
			}
		}

		/// <summary>
		/// Return cleaned copy of referring page
		/// </summary>
		public string ReferringPage {
			get {
				if (Request.UrlReferrer != null) {
					string name = Request.UrlReferrer.ToString();
					return name.Substring(name.LastIndexOf("/") + 1);
				} else {
					return string.Empty;
				}
			}
		}

		/// <summary>
		/// Is authentication required to view this page
		/// </summary>
		public bool RequireAuthentication {
			set { _requireAuthentication = value; }
			protected get { return _requireAuthentication; }
		}

		public new HttpContext Context {
			get {
				if (_context == null) { _context = HttpContext.Current; }
				return _context;
			}
			protected set { _context = value; }
		}

		public Web.Profile Profile {
			get {
				if (_profile == null) { _profile = Profile.Load(this.Context); }
				return _profile;
			}
			set { _profile = value; }
		}

		public string ScriptBlock {
			set {
				if (!string.IsNullOrEmpty(value)) {
					_scriptBlock.AppendFormat("{1}{0}", value, Environment.NewLine);
				}
			}
		}
		public string StyleBlock {
			set { _styleBlock.AppendFormat("{0}{1}{0}", Environment.NewLine, value); }
		}
		/// <summary>
		/// A function to execute after the page loads
		/// </summary>
		public string ScriptEvent {
			set {
				_scriptBlock.AppendFormat("{0}AfterPageLoad(function(){{ {1} }});{0}",
					Environment.NewLine, value);
			}
		}
		/// <summary>
		/// A fully formed script function to execute when page form submits
		/// </summary>
		public string OnSubmitScript {
			set { _scriptBlock.AppendFormat("document.forms[0].onsubmit = function(){{ {0} }};", value); }
		}
		public void ScriptVariable(string name, string value) {
			this.ScriptVariable(name, value, false);
		}
		/// <summary>
		/// Render name/value as EcmaScript variable assignment
		/// </summary>
		/// <param name="literal">
		/// Insert value exactly as is, otherwise attempt conversion to
		/// JavaScript Object Notation
		/// </param>
		public void ScriptVariable(string name, string value, bool literal) {
			if (!literal) { value = value.ToJSON(); }
			_scriptBlock.AppendFormat("var {0} = {1};", name, value);
		}
		/// <summary>
		/// Initialize script variable to null
		/// </summary>
		public void ScriptVariable(string name) {
			this.ScriptVariable(name, EcmaScript.Null, true);
		}

		public ClientResourceList StyleSheet { get { return _styleSheet; } }
		public ClientResourceList ScriptFile { get { return _scriptFile; } }

		/// <summary>
		/// Title displayed at top of browser window
		/// </summary>
		public new string Title {
			set { this.MasterBase.Title.InnerText = value; }
			get { return this.MasterBase.Title.InnerText; }
		}

		/// <summary>
		/// Build name of derived .aspx page
		/// </summary>
		public string FileName {
			get {
				if (string.IsNullOrEmpty(_name)) {
					_name = Request.Path;
					_name = _name.Substring(_name.LastIndexOf("/") + 1);
				}
				return _name;
			}
		}

		#endregion

		#region Constructors

		internal Page(HttpContext context) : base() { _context = context; }
		protected Page() : base() {
			fldAction = new Controls.Field("action", Web.Controls.Field.Types.Hidden);
			fldStep = new Controls.Field("step", Web.Controls.Field.Types.Hidden);
			fldItemID = new Controls.Field("id", Web.Controls.Field.Types.Hidden);
			fldSubItemID = new Controls.Field("subid", Web.Controls.Field.Types.Hidden);
		}

		#endregion

		#region Events

		/// <summary>
		/// Add script and style references
		/// </summary>
		protected override void OnInit(EventArgs e) {
			_scriptFile.InsertResource(0, EcmaScript.Resource.Page);
			_scriptFile.InsertResource(0, EcmaScript.Resource.Extensions);
			_scriptFile.AddResource(EcmaScript.Resource.DOM);
			_scriptFile.AddResource(EcmaScript.Resource.Cookies);
			_scriptFile.AddResource(EcmaScript.Resource.AJAX);
			this.Form.Controls.Add(fldAction);
			this.Form.Controls.Add(fldStep);
			this.Form.Controls.Add(fldItemID);
			this.Form.Controls.Add(fldSubItemID);
			base.OnInit(e);
		}
		protected override void OnLoad(EventArgs e) {
			// load action and ID
			//_action = Utility.NullSafe<Actions>(this.ViewState[_actionKey], Actions.None);
			//_itemID = Utility.NullSafe<Guid>(this.ViewState[_itemIdKey], Guid.Empty);
			base.OnLoad(e);
		}

		/// <summary>
		/// Build scripts
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			string pattern = string.Empty;

			if (_validations != null && _validations.Count > 0) {
				_scriptFile.AddResource(EcmaScript.Resource.Validation);
				_scriptBlock.Append(this.ValidationScriptBlock());
			}
			if (_styleSheet.Count > 0) {
				pattern = string.Format(
                    "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}/{{0}}\" />{1}",
						Utility.BasePath, Environment.NewLine);
				this.MasterBase.StyleSheets.Controls.Add(_styleSheet.ToControl(pattern));
			}
			if (_fixIeCssPng) {
				_styleSheet.Clear();
				_styleSheet.AddResource("png-fix");
				pattern = string.Format(
					"<!--[if lte IE 6]>{1}<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}/{{0}}\" />{1}<![endif]-->{1}",
						Utility.BasePath, Environment.NewLine);
				this.MasterBase.StyleSheets.Controls.Add(_styleSheet.ToControl(pattern));
			}
			if (_styleBlock.Length > 0) {
				_styleBlock.Insert(0, "<style>");
				_styleBlock.Append("</style>");
				this.MasterBase.StyleBlock.Controls.Add(new LiteralControl(_styleBlock.ToString()));
			}
			if (_scriptFile.Count > 0) {
				pattern = string.Format(
                    "<script type=\"text/ecmascript\" src=\"{0}/{{0}}\"></script>{1}",
						Utility.BasePath, Environment.NewLine);
				this.MasterBase.ScriptFiles.Controls.Add(_scriptFile.ToControl(pattern));
			}
			this.RenderScriptBlock(_scriptBlock, this.MasterBase.ScriptBlock);

			// store action and ID if set
			//if (_action != Actions.None) { this.ViewState.Add(_actionKey, _action); }
			//if (!_itemID.Equals(Guid.Empty)) { this.ViewState.Add(_itemIdKey, _itemID); }

			base.OnPreRender(e);
		}

		private void RenderScriptBlock(StringBuilder sb, PlaceHolder ph) {
			if (sb.Length > 0) {
				string script = sb.ToString();
				if (Handlers.Resource.AllowMinify) { script = EcmaScript.Compress(script); }
				script = "<script type=\"text/ecmascript\">" + script + Environment.NewLine + "</script>";
				ph.Controls.Add(new LiteralControl(script));
			}
		}

		/// <summary>
		/// Generate script block for validation controls
		/// </summary>
		private string ValidationScriptBlock() {
			StringBuilder script = new StringBuilder();
			bool first = true;

            script.Append("function Validators() { return [");
			
			foreach (Controls.Validation v in _validations) {
				if (v.IsValid) {
					if (!first) { script.Append(","); } else { first = false; }
					script.Append(Resource.SayFormat("Script_CreateValidation",
						v.For, v.Type, v.Message, v.Required.ToString().ToLower()));
				}
			}
			script.Append("]; }");
			return script.ToString();
		}

		#endregion
	
		#region Send

		public void SendBack(string sendTo) {
			if (string.IsNullOrEmpty(sendTo)) { sendTo = this.ReferringPage; }
			Response.Redirect(sendTo, true);
		}

		public void SendBack() { this.SendBack(string.Format("{0}/", Utility.BasePath)); }

		public void SendToLogin() {
			if (string.IsNullOrEmpty(_loginPage)) {
				throw new NullReferenceException("No LoginPage has been specified");
			}
			Profile.DestinationPage = Request.Url.PathAndQuery;
			Response.Redirect(_loginPage, true);
		}

		#endregion

		#region Links

		/*
		protected string NextStepLink {
			get {
				return string.Format("{0}?{1}={2}&{3}={4}",
					this.FileName, fldItemID.ID, this.ItemID, this.fldStep.ID, this.Step + 1);
			}
		}
		protected string PreviousStepLink {
			get {
				int step = this.Step - 1;
				if (step < 0) { step = -1; }
				return string.Format("{0}?{1}={2}&{3}={4}",
					this.FileName, fldItemID.ID, this.ItemID, this.fldStep.ID, step);
			}
		}
		*/
		#endregion

		/// <summary>
		/// Was the specified action indicated
		/// </summary>
		protected bool HasAction(Actions action) {
			return action.Contains(this.Action);
		}

		/// <summary>
		/// Can the given user view this page
		/// </summary>
		/// <remarks>
		/// This will usually be overriden to implement permission checking.
		/// </remarks>
		protected virtual bool CanView(Profile profile) {
			return (!_requireAuthentication || profile.Authenticated);
		}

		/// <summary>
		/// Respond to user attempt to view this page
		/// </summary>
		/// <param name="url">The URL that should be used to view this page instance</param>
		/// <remarks>
		/// This can be called to display a login screen as required before actually
		/// navigating to the page.
		/// </remarks>
		/// <returns>
		/// Array containing result code enumeration and url or message to display.
		/// </returns>
		[WebInvokable()]
		protected string[] TryView(Profile profile, string url) {
			string message = string.Empty;
			ViewResult result;

			if (this.CanView(profile)) {
				result = ViewResult.Granted;
				message = url;
			} else if (profile.Authenticated) {
				result = ViewResult.Denied;
				message = Resource.Say("Message_UnauthorizedScreen");
			} else {
				result = ViewResult.NeedLogin;
				message = Resource.Say("Message_NeedLogin");
			}
			return new string[] { ((int)result).ToString(), message }; 
		}
	}
}