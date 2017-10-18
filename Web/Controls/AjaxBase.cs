using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Methods common to AjaxControl and AjaxUserControl
	/// </summary>
	/// <remarks>
	/// This can't be implemented as a traditional base class since the ajax
	/// controls inherit from different classes
	/// </remarks>
	public sealed class AjaxBase {

		private System.Web.UI.Control _control = null;
		private Type _controlType = null;
		private HttpContext _context;
		private bool _eventsCompleted = false;
		private List<string> _afterRender = new List<string>();
		private Idaho.Web.Page _hostPage = null;
		private string _refreshFor = string.Empty;
		private string _startTimerFor = string.Empty;
		private TimeSpan _refreshInterval = TimeSpan.Zero;
		private bool _showErrors = true;
		private bool _showProgress = false;
		private bool _showTimerProgress = false;
		private TimeSpan _timeout = TimeSpan.Zero;
		private string _waitMessage = string.Empty;
		private bool _showWaitCursor = false;
		private string _containerTagID = string.Empty;
		private string _titleBarTagID = string.Empty;
		private RenderModes _renderMode = RenderModes.Synchronous;
		private RenderStates _renderState = RenderStates.InPage;
		private PropertyInfo[] _properties;
		private Idaho.Web.Profile _profile = null;
		private bool _isLazy = false;
		private string _emptyResultMessage = string.Empty;
		private PropertyBindings _bindings = null;
		private DateTime _startRenderOn;
		private bool _useCache = false;
		private bool _cacheable = false;
		private bool _enableDrag = false;
		private List<List<Control>> _controlList = null;
		private List<Type> _dependsOnType = null;
		private Dictionary<string, string> _hiddenFields = null;
		private static KeyValuePair<string, string> _abbreviation;
		private StringBuilder _script;

		#region Enumerations

		/// <summary>
		/// Indicate the current rendering state of the control
		/// </summary>
		/// <remarks>
		/// An asynchronous control has two rendering states: one
		/// to place EcmaScript loaders on the page to fire off an
		/// asynchronous call and one to render the normal control
		/// content. Complicating this is the fact that a control
		/// with asynchronous capabilities can be flagged to render
		/// synchronously, directly in the page, in which case it
		/// won't render any EcmaScript loaders, only its normal
		/// content directly to the page.
		/// </remarks>
		[Flags()]
		public enum RenderStates {
			/// <summary>Control is rendering within page context</summary>
			InPage = 1,
			/// <summary>Control is rendering asynchronously in its own thread</summary>
			Isolation = 2
		}
		/// <summary>
		/// Indicate the intended rendering method for the control
		/// </summary>
		/// <remarks>
		/// A control flagged to be rendered asynchronously will still
		/// render a portion synchronously on the page to fire off
		/// the asynchronous call.
		/// </remarks>
		[Flags()]
		public enum RenderModes {
			Lazy = 0,
			/// <summary>Render the control normally with the page</summary>
			Synchronous = 1,
			/// <summary>control rendered through reflection</summary>
			Asynchronous = 2
		}

		internal struct List { public const int Static = 0; public const int Bound = 1; }

		#endregion

		#region Constructors

		internal AjaxBase(System.Web.UI.Control control, HttpContext context) {
			if (!(control is IAjaxControl)) { throw new System.InvalidOperationException(); }
			_control = control;
			_properties = WebBindable.Properties(_control.GetType());
			_context = context;
			_bindings = new PropertyBindings((IAjaxControl)_control);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Allow dragging of control
		/// </summary>
		public bool EnableDrag {
			get { return _enableDrag; } set { _enableDrag = value; }
		}

		/// <summary>
		/// Indicate if this instance of the control is rendering its
		/// normal content as opposed to rendering a placeholder for
		/// asynchronous population (or being hidden)
		/// </summary>
		public bool IsRenderingContent {
			get {
				return (_renderMode == RenderModes.Synchronous)
					|| ((_renderMode == RenderModes.Lazy || _renderMode == RenderModes.Asynchronous)
					&& _renderState == RenderStates.Isolation && _control.Visible);
			}
		}

		/// <summary>
		/// Tag ID for element containing control, typically for dialog controls
		/// </summary>
		internal string ContainerTagID {
			set { _containerTagID = value; }
			get { return _containerTagID; }
		}
		/// <summary>
		/// Tag ID for title bar element, typically for dialog controls
		/// </summary>
		public string TitleBarTagID {
			set { _titleBarTagID = value; }
			get { return _titleBarTagID; }
		}
		/// <summary>
		/// Update HTML style with script so cursor indicates waiting
		/// </summary>
		public bool ShowWaitCursor { set { _showWaitCursor = value; } }

		/// <summary>
		/// Key-value pairs to render as hidden fields
		/// </summary>
		public Dictionary<string, string> HiddenField {
			set { _hiddenFields = value; }
			get {
				if (_hiddenFields == null) { _hiddenFields = new Dictionary<string, string>(); }
				return _hiddenFields;
			}
		}

		/// <summary>
		/// List of typename abbreviations
		/// </summary>
		/// <remarks>
		/// This is to simplify and secure the typenames emitted for AJAX
		/// controls. The AJAX responder must expand the abbreviations.
		/// </remarks>
		public static KeyValuePair<string, string> TypeAbbreviation {
			set { _abbreviation = value; }
		}

		/// <summary>
		/// The data types this control instance depends on for rendering
		/// </summary>
		public List<Type> DependsOnType { get { return _dependsOnType; } }

		/// <summary>
		/// Data source type that this control depends on
		/// </summary>
		/// <remarks>
		/// This is used to establish cache dependencies between entity
		/// collections (the type) and output cache items.
		/// </remarks>
		public ISignalingObject CacheBasedOn {
			set {
				if (this.IsRenderingContent) {
					if (_dependsOnType == null) { _dependsOnType = new List<Type>(); }
					_cacheable = true;
					value.Dependency += Idaho.Web.Ajax.Response.Cache.OnControlDataChange;
					_dependsOnType.Add(value.GetType());
				}
			}
		}

		/// <summary>
		/// Indicate whether this control's output CAN be cached (not if it SHOULD)
		/// </summary>
		public bool Cacheable { set { _cacheable = value; } get { return _cacheable; } }

		/// <summary>
		/// Does the control need EcmaScript to be setup
		/// </summary>
		/// <remarks>
		/// A control will need load script on the page when it is configured
		/// to render asynchronously (its mode) but the current instance is
		/// rendering within the page (its state is "InPage").
		/// </remarks>
		public bool NeedLoadScript {
			get {
				return this.IsRenderMode(RenderModes.Asynchronous) &&
					this.IsRenderState(RenderStates.InPage);
			}
		}

		/// <summary>
		/// Indicate if the output should be cached (if it can)
		/// </summary>
		public bool UseCache { get { return _useCache; } set { _useCache = value; } }

		public bool EventsCompleted {
			get { return _eventsCompleted; } set { _eventsCompleted = value; }
		}
		public HttpContext Context {
			get { return _context; } set { _context = value; }
		}
		public string TypeName { get { return _control.GetType().ToString(); } }

		/// <summary>
		/// Timeout in seconds for asynchronous rendering (default is 15)
		/// </summary>
		public TimeSpan Timeout { set { _timeout = value; } }

		/// <summary>
		/// Time taken to render the control
		/// </summary>
		public DateTime StartRenderOn { get { return _startRenderOn; } }

		/// <summary>
		/// Message to display if control has no content
		/// </summary>
		public string EmptyResultMessage { set { _emptyResultMessage = value; } }

		public Idaho.Web.Profile Profile {
			get {
				if (_profile == null) {
					if (_hostPage != null) {
						_profile = _hostPage.Profile;
					} else {
						_profile = Idaho.Web.Profile.Load(_context);
					}
				}
				return _profile;
			}
		}
		public PropertyBindings Bindings { get { return _bindings; } }

		/// <summary>
		/// String of javascript to execute after asynchronous call completes
		/// </summary>
		public string AfterRender {
			set {
				_afterRender.Add(value.Replace("[id]", _control.ID));
			}
		}

		/// <summary>
		/// A property that should trigger this control to update
		/// </summary>
		public string BindProperty { set { _bindings.Add(value); } }

		/// <summary>
		/// Should control fail silently or display errors in control location
		/// </summary>
		public bool ShowErrors { set { _showErrors = value; } }

		/// <summary>
		/// Display message while control is loading
		/// </summary>
		public string WaitMessage { set { _waitMessage = value; } }
		public string WaitMessageResource {
			set { _waitMessage = Resource.Say("Message_{0}", value); }
		}
		public bool ShowLoadProgress { set { _showProgress = value; } }
		public bool ShowTimerProgress { set { _showTimerProgress = value; } }
		public Idaho.Web.Page Page {
			get {
				if (_hostPage == null) { _hostPage = new Page(_context); }
				return _hostPage;
			}
			set { _hostPage = value; }
		}
		/// <summary>
		/// Frequency at which the control should automatically refresh
		/// </summary>
		public TimeSpan RefreshInterval { set { _refreshInterval = value; } }
		public string StartTimerFor {
			set {
				_startTimerFor = value;
				if (_startTimerFor != null) _renderMode = RenderModes.Asynchronous;
			}
		}
		/// <summary>
		/// A DOM element that should trigger this control to refresh if changed
		/// </summary>
		public string RefreshFor {
			set {
				_refreshFor = value;
				if (_refreshFor != string.Empty) _renderMode = RenderModes.Asynchronous;
			}
		}
		/// <summary>
		/// Indicate current rendering state
		/// </summary>
		public RenderStates RenderState { 
			get { return _renderState; }
			set { _renderState = value; }
		}
		/// <summary>
		/// Indicates how the control should load
		/// </summary>
		public RenderModes RenderMode {
			get { return _renderMode; }
			set {
				_renderMode = value;
				if (_renderMode == RenderModes.Lazy) {
					_isLazy = true;
					_renderMode = RenderModes.Asynchronous;
				}
			}
		}

		#endregion

		#region DOM Scripts

		/// <summary>
		/// Build EcmaScript to render control asynchronously
		/// </summary>
		/// <remarks>Depends on /script/ajax.js and /script/control.js</remarks>
		internal string BuildLoadScript() {
			// create script for asynchronous event handling
			string r = Environment.NewLine;
			string t = "\t";
			Type controlType = _control.GetType();
			string autoRefreshFor = Bindings.RefreshEvents();
			bool createTimer = !(string.IsNullOrEmpty(_startTimerFor)
				|| _refreshInterval.Equals(TimeSpan.Zero));
			string[] properties = this.PropertyLists();
			_script = new StringBuilder();

			// explicitly added listeners cancel auto-listeners
			if (string.IsNullOrEmpty(_refreshFor) && !string.IsNullOrEmpty(autoRefreshFor)) {
				_refreshFor = autoRefreshFor;
			}

			if (_isLazy) { _refreshFor = string.Format("{0};window.global", _refreshFor); }

			// build EcmaScript
			_script.AppendFormat("var {0} = new Control(\"{0}\");{1}", _control.ID, r);
			_script.AppendFormat("with ({0}) {{{1}", _control.ID, r);

			this.ScriptBoolean("ShowProgress", _showProgress);
			this.ScriptBoolean("ShowWaitCusor", _showWaitCursor);

			foreach (string e in _afterRender) {
				_script.AppendFormat("{0}AfterRender.push(function(){{ {1} }});{2}", t, e, r);
			}
			if (_enableDrag && !string.IsNullOrEmpty(_titleBarTagID) &&
				!string.IsNullOrEmpty(_containerTagID)) {
				// set properties to allow dragging of control node
				this.ScriptBoolean("EnableDrag", true);
				this.ScriptString("ContainerID", _containerTagID);
				this.ScriptString("TitleBarID", _titleBarTagID);
			}
			this.ScriptString("WaitMessage", _waitMessage);
			this.ScriptString("EmptyResultMessage", _emptyResultMessage);
			this.ScriptString("Assembly", Abbreviate(controlType.QualifiedName()));
			this.ScriptBoolean("ShowErrors", _showErrors);

			if (_control is AjaxControl) {
				AjaxControl a = (AjaxControl)_control;
				this.ScriptString("CssClass", a.CssClass);
				this.ScriptString("Style", a.Style.Value);
			}
			if (!_timeout.Equals(TimeSpan.Zero)) {
				_script.AppendFormat("{0}Timeout = {1};{2}", t, _timeout, r);
			}
			// the DOM events that will cause this control to reload
			if (!string.IsNullOrEmpty(_refreshFor)) {
				_script.AppendFormat("{0}RefreshFor({1});{2}", t,
					_refreshFor.ToScriptArray(), r);
			}
			// values of bound properties are reloaded whenever a specified
			// field is updated
			if (!string.IsNullOrEmpty(properties[List.Bound])) {
				_script.AppendFormat("{0}Bindings([{1}]);{2}", t, properties[List.Bound], r);
			}
			// values of static properties are loaded only once
			if (!string.IsNullOrEmpty(properties[List.Static])) {
				_script.AppendFormat("{0}Properties([{1}]);{2}", t, properties[List.Static], r);
			}
			if (createTimer) {
				_script.AppendFormat("{0}StartTimerFor({1});{2}", t,
					_startTimerFor.ToScriptArray(), r);
				_script.AppendFormat("{0}RefreshInterval = {1};{2}", t,
					_refreshInterval.TotalMilliseconds, r);
			}
			_script.Append("}");
			return _script.ToString();
		}

		private void ScriptBoolean(string name, bool value, bool ignoreFalse) {
			if (!ignoreFalse || value) {
				_script.AppendFormat("\t{0} = {1};{2}", 
					name, value.ToJSON(), Environment.NewLine);
			}
		}
		private void ScriptBoolean(string name, bool value) {
			this.ScriptBoolean(name, value, true);
		}
		private void ScriptString(string name, string value, bool ignoreNull) {
			if (!ignoreNull || !string.IsNullOrEmpty(value)) {
				_script.AppendFormat("\t{0} = \"{1}\";{2}",
					name, value, Environment.NewLine);
			}
		}
		private void ScriptString(string name, string value) {
			this.ScriptString(name, value, true);
		}

		/// <summary>
		/// Should be called by control OnInit()
		/// </summary>
		internal void Initialize() {
			_startRenderOn = DateTime.Now;
			if (_renderState == RenderStates.InPage) {
				_hostPage = (Idaho.Web.Page)_control.Page;
				_hostPage.ScriptFile.AddResource(EcmaScript.Resource.Control);
				// remove child controls to prevent their events
				if (_renderMode == RenderModes.Asynchronous) { _control.Controls.Clear(); }
			}
			_eventsCompleted = true;
		}

		/// <summary>
		/// String of name=value pairs representing server-side parameters
		/// </summary>
		/// <param name="properties">array of bindable control properties</param>
		/// <param name="bindings">list of DOM property bindings</param>
		static internal string[] PropertyLists(PropertyInfo[] properties,
			PropertyBindings bindings, IAjaxControl control) {

			// bound dynamic parameters
			StringBuilder boundList = new StringBuilder();
			// static parameters
			StringBuilder staticList = new StringBuilder();
			string pair;
			Regex empty = new Regex("^0|null|\\[\\]$");
			Type attributeType = typeof(WebBindable);

			foreach (PropertyInfo p in properties) {
				if (p.CanRead) {
					pair = string.Format("\"{0}={{0}}\",", p.Name);
					if (bindings.Contains(p)) {
						boundList.AppendFormat(pair, bindings[p]);
					} else {
						string value = p.GetValue(control, null).ToJSON();

						if (!string.IsNullOrEmpty(value) && !empty.IsMatch(value)) {
							value = value.Trim('"');
						} else if (WebBindable.AlwaysBind(p)) {
							// always bindable properties are scripted even if null
							value = EcmaScript.Null;
						} else {
							// ignore this property
							continue;
						}
						staticList.AppendFormat(pair, value);
					}
				}
			}
			if (boundList.Length > 2) { boundList.Length -= 1; }	// remove trailing comma
			staticList.AppendFormat("\"id={0}\"", control.ID);

			return new string[] { staticList.ToString(), boundList.ToString() };
		}

		#endregion

		#region Property Bindings

		/// <summary>
		/// Get bindable property with given name, if any
		/// </summary>
		/// <remarks>
		/// Search the binding collection to see if a property with the given
		/// name was registered for binding.
		/// </remarks>
		public PropertyInfo GetBindableProperty(string name) {
			return GetBindableProperty(name, _properties);
		}
		static internal PropertyInfo GetBindableProperty(string name, PropertyInfo[] properties) {
			name = name.ToLower();
			foreach (PropertyInfo p in properties) {
				if (p.Name.ToLower() == name) { return p; }
			}
			return null;
		}
		/// <summary>
		/// Can property of given name be bound to an HTML field
		/// </summary>
		private bool IsBindable(string name) { return IsBindable(name, _properties); }

		static internal bool IsBindable(string name, PropertyInfo[] properties) {
			PropertyInfo property = GetBindableProperty(name, properties);
			return (property != null);
		}

		/// <summary>
		/// Collection of DOM field/property binding elements
		/// </summary>
		/// <remarks>
		/// This causes a DOM listener to be setup for the HTML element
		/// triggering an AJAX call to set the specified property when that
		/// element is changed. The property must be readable and writeable.
		/// </remarks>
		public class PropertyBindings : Dictionary<PropertyInfo, string> {

			private IAjaxControl _control = null;

			// no default
			private PropertyBindings() { }
			internal PropertyBindings(IAjaxControl control) { _control = control; }

			/// <summary>
			/// Setup binding between DOM node property and type property
			/// </summary>
			/// <example>
			/// fldText1=Property1;fldText2=Property2
			/// </example>
			internal void Add(string binding) {
				if (!string.IsNullOrEmpty(binding)) {
					binding = binding.TrimEnd(';');
					string[] properties = binding.Contains(";") ? binding.Split(';') : new string[] { binding };
					string[] pair;

					foreach (string p in properties) {
						pair = p.Split('=');
						this.Add(pair[0], pair[1]);
					}
				}
			}
			public void Add(string propertyName, Control control) {
				this.Add(propertyName, control.ID);
			}
			public void Add(string propertyName, string controlID) {
				PropertyInfo info = _control.Ajax.GetBindableProperty(propertyName);

				if (info != null) {
					_control.Ajax.RenderMode = RenderModes.Asynchronous;
					base.Add(info, controlID);
				} else {
					throw new System.Exception(
						string.Format("The property \"{0}\" does not exist or cannot be bound on \"{1}\"",
							propertyName, _control.Ajax.TypeName));
				}
			}

			/// <summary>
			/// Create EcmaScript events to trigger binding changes
			/// </summary>
			internal string RefreshEvents() {
				StringBuilder script = new StringBuilder();
				string eventName;

				foreach (PropertyInfo p in this.Keys) {
					eventName = (p.PropertyType == typeof(bool)) ? "onclick" : "onchange";
					script.AppendFormat(";{0}.{1}", this[p], eventName);
				}
				return script.ToString();
			}

			/// <summary>
			/// Does the binding collection reference the given property
			/// </summary>
			internal bool Contains(PropertyInfo p) { return this.ContainsKey(p); }
		}

		#endregion

		#region Events

		/// <summary>
		/// Convenience function to test render mode
		/// </summary>
		public bool IsRenderState(AjaxBase.RenderStates state) {
			return _renderState.Contains(state);
		}
		public bool IsRenderMode(AjaxBase.RenderModes mode) {
			return _renderMode.Contains(mode);
		}

		/// <summary>
		/// Manually invoke prerender and earlier handlers
		/// </summary>
		/// <remarks>
		/// This is to simulate the page lifecycle for controls rendering outside
		/// a page context (RenderStates.Isolation)
		/// </remarks>
		internal void PreRender() {
			_controlList = new List<List<Control>>();
			_renderState = AjaxBase.RenderStates.Isolation;
			_controlType = typeof(System.Web.UI.Control);
			
			object[] parameters = new object[] { new EventArgs() };
			BindingFlags handlerScope = BindingFlags.Instance | BindingFlags.NonPublic;
			MethodInfo initialize = _controlType.GetMethod("OnInit", handlerScope);
			MethodInfo load = _controlType.GetMethod("OnLoad", handlerScope);
			MethodInfo ensure = _controlType.GetMethod("EnsureChildControls", handlerScope);
			MethodInfo prerender = _controlType.GetMethod("OnPreRender", handlerScope);

			// simulate page lifecycle
			this.InvokeInsideOut(_control, initialize, parameters, 0);
			this.InvokeOutsideIn(_control, load, parameters);
			this.InvokeOutsideIn(_control, ensure, null);
			this.InvokeOutsideIn(_control, prerender, parameters);
		}

		/// <summary>
		/// Invoke given method recusively on control collection
		/// </summary>
		private void InvokeOutsideIn(Control control, MethodInfo method, object[] parameters) {
			method.Invoke(control, parameters);
			foreach (Control c in control.Controls) { this.InvokeOutsideIn(c, method, parameters); }
		}

		/// <summary>
		/// Invoke given method recursively from the inside out
		/// </summary>
		private void InvokeInsideOut(Control control, MethodInfo method, object[] parameters, int level) {
			if (_controlList.Count <= level) { _controlList.Add(new List<Control>()); }

			_controlList[level].Add(control);

			foreach (Control c in control.Controls) {
				this.InvokeInsideOut(c, method, parameters, level + 1);
			}
			if (level == 0) {
				// we've recursed back to the root control
				for (int x = _controlList.Count - 1; x >= 0; x--) {
					foreach (Control c in _controlList[x]) {
						method.Invoke(c, parameters);
					}
				}
			}
		}

		#endregion

		#region Rendering

		/// <summary>
		/// Appearenace of control when rendered as asynchronous placeholder
		/// </summary>
		internal void RenderPlaceHolder(HtmlTextWriter writer) {
			writer.Write("<div id=\"");
			writer.Write(_control.ID);
			writer.Write("\">");
			if (!string.IsNullOrEmpty(_waitMessage)) {
				writer.Write("<div id=\"");
				writer.Write(_control.ID);
				writer.Write("Progress\" class=\"ajaxProgress\"");
				// only show immediate progress for lazy load
				if (!_isLazy) { writer.Write(" style=\"display: none;\""); }
				writer.Write(">");
				writer.Write(_waitMessage);
				writer.Write("</div>");
			}
			writer.Write("</div>");

			if (_hiddenFields != null) {
				foreach (string key in _hiddenFields.Keys) {
					writer.Write("<input type=\"hidden\" name=\"{0}Field\" id=\"{0}{1}\" value=\"{2}\" />",
						_control.ID, key, _hiddenFields[key]);
				}
			}
		}

		#endregion

		/// <summary>
		/// Find the abbreviation for a given type name
		/// </summary>
		internal static string Abbreviate(string typeName) {
			return typeName.Replace(_abbreviation.Value, _abbreviation.Key);
		}
		/// <summary>
		/// Find the full name for an abbreviated type name
		/// </summary>
		internal static string Expand(string abbreviation) {
			return abbreviation.Replace(_abbreviation.Key, _abbreviation.Value);
		}

		internal string[] PropertyLists() {
			return PropertyLists(_properties, _bindings, (IAjaxControl)_control);
		}
	}
}
