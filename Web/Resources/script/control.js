/// <reference path="extensions.js"/>
/// <reference path="dom.js"/>
/// <reference path="ajax.js"/>
/// <reference path="grid.js"/>
/// <reference path="page.js"/>

function Control(id) {
	/// <summary>Encapsulate AJAX methods for .Net controls</summary>
	/// <param name="id">ID of control</param>

	var _this = this;
	var _node = DOM.GetNode(id, true);
	var _progressNode = DOM.GetNode(id + "Progress", true);
	var _timer = null;
	var _grid = false;
	var _showProgress = false;
	var _suppressAlerts = false;
	var _properties = new Object();	// hash table of property nodes
	
	this.Assembly = null;			// assembly name of .NET control
	this.AttachMenu = false;		// attach events for context menu
	this.ShowErrors = true;			// alert errors
	this.ShowProgress = false;		// show progress animation
	this.ShowWaitCursor = false;	// display cursor indicating wait
	this.ProgressMessage = "";		// message to show for progress
	this.WaitMessage = "";			// message to show in place of content during load
	this.EnableDrag = false;		// allow dragging and dropping
	this.IsGrid = false;			// is attached control an HTML table
	this.AfterRender = [];			// functions to call after rendering
	this.RefreshInterval = 0;		// milliseconds to repeat, if set
	this.Grid = null;				// can reference grid.js object
	this.EmptyResultMessage = null;	// message to show if no results
	this.Timeout = 15;				// timeout for async call
	this.ContainerID = "";			// for dialogs: tag ID of containing element
	this.TitleBarID = "";			// for dialogs: tag ID of title bar element
	this.Style = "";
	this.CssClass = "";
	this.Node = _node;
	this.ID = id;
	
	AddHandler(UnloadHandler, window, "unload");

	function Load() {
		/// <summary>Use xmlHttp to load control content</summary>
		_showProgress = (_this.WaitMessage && _this.ShowProgress);
		var request = new ServerCall(_this.Assembly, PropertyValues());
		request.Timeout = _this.Timeout;
		request.OnComplete = Render;
		if (_suppressAlerts) {
			request.Timeout = 0;
			request.ShowErrors = false;
		} else {
			request.ShowErrors = _this.ShowErrors;
		}
		if (_showProgress) {
			_node.innerHTML = "<div class=\"ajaxProgress\">" + _this.WaitMessage + "</div>";
		}
		_node.className += " loading";
		
		if (_this.ShowWaitCursor) { document.body.style.cursor = "progress"; }
		request.Start();
	}

	function Render(html) {
		/// <summary>Callback from xmlHttp that renders the control</summary>
		/// <param name="html">Content returned from AJAX method</param>
		document.body.style.cursor = "auto";
		if (_showProgress) { Page.Progress.End(); }
		if (!html) { html = ""; }
		if (_node) {
			_node.innerHTML = html;
			for (var x = 0; x < _this.AfterRender.length; x++) {
				switch (typeof(_this.AfterRender[x])) {
					case "string": eval(_this.AfterRender[x]); break;
					case "function": _this.AfterRender[x](_this); break;
				}
			}
			if (html == "" && _this.EmptyResultMessage) {
				Page.Message.Show(_this.EmptyResultMessage);
			}
			if (_this.EnableDrag && _this.ContainerID != "" && _this.TitleBarID != "") {
				SetDragHandlers();
			}
			_node.className = _node.className.replace(" loading", "");
			_node.style.display = "block";
		} else {
			Page.Exception("Control.Render", "Unable to find " + id);
			_this.StopTimer();
		}
	}

	function PropertyValues() {
		/// <summary>Generate a hash of all properties for xmlHttp</summary>
		/// <returns>Object (hash)</returns>
		var values = new Object();
		for (key in _properties) {
			values[key] = _properties[key].value();
		}
		values["cssClass"] = _this.CssClass;
		values["style"] = _this.Style;
		return values;
	}

	this.SetupGrid = function(itemLink, showMenu) {
		/// <summary>Extra setup for grid controls</summary>
		/// <param name="itemLink">URL to use for linking items</param>
		/// <param name="showMenu">Whether right-click menu should be enabled</param>
		_this.Grid = new GridObject(id, itemLink, showMenu, true);
		_this.Grid.Control = _this;
		_this.AfterRender.unshift(_this.Grid.SetupEvents);
	}

	this.Sort = function(column, direction) {
		/// <summary>Repeat AJAX call to change sort order for grid</summary>
		/// <param name="column">Name of column to sort on</param>
		/// <param name="direction">Named direction to sort</param>
		SetProperty("sortBy", column);
		SetProperty("sortDirection", direction);
		var showProgress = _this.ShowProgress;
		_this.Refresh();
		_this.ShowProgress = showProgress;
	}

	function Repeat() {
		/// <summary>Set control to reload repeatedly (RefreshInterval must be set)</summary>
		if (_this.RefreshInterval == 0) { return; }
		_suppressAlerts = true;
		_timer = setInterval(Load, _this.RefreshInterval);
	}

	this.Render = function() {
		/// <summary>Load control content</summary>
		Load();
	}
	this.Refresh = function() {
		/// <summary>Load control content quietly (without showing progress)</summary>
		_this.ShowProgress = false; Load();
	}
	this.Clear = function() {
		/// <summary>Clear control HTML</summary>
		_node.innerHTML = "";
	}
	this.Fade = function() {
		/// <summary>Fade control node opacity</summary>
		this.Hide();
	}
	this.Hide = function() {
		/// <summary>Change control node style to invisible</summary>
		_node.style.display = "none"; Page.Modal(false);
	}
	this.RefreshFor = function(list) {
		/// <summary>Indicate events control should refresh for</summary>
		/// <param name="list">Array of element "on" event references</param>
		/// <example>RefreshFor([dlgPersonEntityID.onchange])</example>
		ListenFor(list, Load);
	}
	this.StopTimer = function() {
		/// <summary>Cancel control refresh or delay timer</summary>
		clearInterval(_timer); _suppressAlerts = false;
	}
	this.StartTimerFor = function(list) {
		/// <summary>Indicate events that should start refresh timer</summary>
		/// <param name="list">Array of element "on" event references</param>
		ListenFor(list, Repeat);
	}
	
	function ListenFor(list, fn) {
		/// <summary>Establish events the control will listen for</summary>
		/// <param name="list">Array of element "on" event references</param> 
		/// <param name="fn">Function to execute when event fires</param>
		if (!list) { return; }
		var pair;
		var nodeID, eventName;
		var node;
		for (var x = 0; x < list.length; x++) {
			node = null;
			pair = list[x].split(".");
			nodeID = pair[0]; eventName = pair[1];
			if (eventName == "global") {
				// artificial event to handle page script load completion
				AfterPageLoad(fn); continue;
			} else if (nodeID == "window" && nodeID == "body") {
				node = eval(nodeID);
			} else {
				node = DOM.GetNode(nodeID);
			}
			if (!node) {
				if (_this.ShowErrors) {
					Page.Exception("Control.ListenFor",
						"Cannot refresh control \"" + id + "\" for " + list[x] +
						"\n\"" + nodeID + "\" cannot be found");
				}
				continue;
			}
			AddHandler(fn, node, eventName.replace("on", ""));
		}
	}
	
	this.Bindings = function(list) {
		/// <summary>Associate dynamic control properties with input fields</summary>
		/// <param name="list">Array of name=value pairs</param>
		Parse(list, true);
	}
	this.Properties = function(list) {
		/// <summary>Associate control properties with input values</summary>
		/// <param name="list">Array of name=value pairs</param>
		/// <example>Properties(["Message=The requested page requires authentication","id=login"])</example>
		Parse(list, false);
	}
	function Parse(list, dynamic) {
		/// <summary>Create hash of listed properties and input fields</summary>
		/// <param name="list">Array of name=value pairs</param>
		/// <param name="dynamic">Setup events so control refreshes when fields change</param>
		if (!list) { return; }
		var pair;
		var key, value;
		for (var x = 0; x < list.length; x++) {
			pair = list[x].split("=");
			key = pair[0]; value = pair[1];
			if (dynamic && !DOM.GetNode(value)) {
				// attempted bind to field that doesn't exist
				if (_this.ShowErrors) {
					Page.Exception("Control.Parse", "Cannot bind to field \"" + value +
						"\"\nNo field with that name could be found");
				}
				continue;	
			}
			_properties[key] = new Object();	// nested hash
			_properties[key].field = value;
			// make value a function so dynamic properties can be read at invokation
			if (dynamic) {
				_properties[key].value = function() { return DOM.GetValue(this.field); }
			} else {
				_properties[key].value = function() { return this.field; }
			}
		}
	}
	// public accessor
	this.Property = _properties;
	
	function SetProperty(key, value) {
		/// <summary>Add custom property to parameter collection</summary>
		if (!_properties[key]) { _properties[key] = new Object(); }
		_properties[key].field = value;
		_properties[key].value = function() { return this.field; }
	}

	this.GetValue = function() {
		Page.Exception("Control object " + id, "GetValue() has not been implemented");
	}		

	function SetDragHandlers() {
		/// <summary>Setup handlers for dragging control position</summary>
		var __lastMousePosition = { X : 0, Y : 0 };
		var __nodePosition = { X : 0, Y : 0 };
		var __container = DOM.GetNode(_this.ContainerID, false, _node);
		var __titleBar = DOM.GetNode(_this.TitleBarID, false, __container);

		if (!__titleBar) { return; }
		if (__container.style.left) {
			// get position from style
			__nodePosition = {
				X : parseInt(__container.style.left),
				Y : parseInt(__container.style.top)
			};
		} else {
			// infer position
			var node = __container;
			while (node.offsetParent) {
				__nodePosition.X += (node.offsetLeft - node.scrollLeft);
				__nodePosition.Y += (node.offsetTop - node.scrollTop);
				node = node.offsetParent;
			}
		}
		AddHandler(MouseDownHandler, __titleBar, "mousedown");
		
		function MouseDownHandler(e) {
			e = DOM.NormalizeEvent(e);
			__lastMousePosition = { X : e.screenX, Y : e.screenY };
			AddHandler(MoveHandler, document, "mousemove");
			AddHandler(UpHandler, document, "mouseup");
		}
		function MoveHandler(e) {
			e = DOM.NormalizeEvent(e);
			var mousePosition = { X : e.screenX, Y : e.screenY };
			var newNodePosition = {
				X : __nodePosition.X + (mousePosition.X - __lastMousePosition.X),
				Y : __nodePosition.Y + (mousePosition.Y- __lastMousePosition.Y)
			}
			__container.style.left = newNodePosition.X + "px";
			__container.style.top = newNodePosition.Y + "px";
			return false;
		}
		function UpHandler(e) {
			RemoveHandler(MoveHandler, document, "mousemove");
			RemoveHandler(UpHandler, document, "mouseup");
		}
	}
	
	function UnloadHandler() {
		/// <summary>Release memory</summary>
		_progressNode = _node = _properties = null;
	}
}