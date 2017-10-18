/// <reference path="extensions.js"/>
/// <reference path="dom.js"/>
/// <reference path="ajax.js"/>

var Page = null;
var LoadFunctions = new Array();
var GUID_EMPTY = "00000000-0000-0000-0000-000000000000";
var NA = "Not Implemented";
var pageLoaded = false;

AddHandler(PageLoad, window, "load");

function PageLoad() {
	/// <summary>Methods executed for each page load</summary>
	Page = new PageObject();
	for (var x = 0; x < LoadFunctions.length; x++) { LoadFunctions[x](); }
	pageLoaded = true;
}

function AddHandler(fn, node, type) {
	/// <summary>Browser-agnostic method to add a bubbling event listener</summary>
	/// <param name="fn">Function to execute when event happens</param>
	/// <param name="node">Node object with event to handle</param>
	/// <param name="type">Name of event without "on" prefix</param>
	/// <returns>Boolean (was event attached)</returns>
	if (node.addEventListener) { node.addEventListener(type, fn, true); return true; }
	else if (node.attachEvent) {
		if (type == "DOMMouseScroll") {
			node.onmousewheel = fn; return true;
		} else {
			return node.attachEvent("on" + type, fn);
		}
	}
	return false;
}
function RemoveHandler(fn, node, type) {
	/// <summary>Browser-agnostic method to remove an event listener</summary>
	/// <param name="fn">Listening function to be removed</param>
	/// <param name="node">Node object with event</param>
	/// <param name="type">Name of event without "on" prefix</param>
	/// <returns>Boolean (was event dettached)</returns>
	if (node.removeEventListener) { node.removeEventListener(type, fn, true); return true; }
	else if (node.detachEvent) { return node.detachEvent("on" + type, fn); }
}
function AfterPageLoad(fn) {
	/// <summary>Function to execute after the page loads</summary>
	if (pageLoaded) { fn(); } else { LoadFunctions.push(fn); }
}

function PageObject() {
	/// <summary>Encapsulate methods for the page and form</summary>
	var _this = this;
	var _url = "";
	var _form = document.forms[0];
	// hash of functions indexed by key
	var _keyHandler = new Object();

	PreFetchImages();
	//FixIeFilterPath();
	AddHandler(GarbageCollect, window, "unload");

	this.Form = _form;
	this.OnSubmit = _form.onsubmit;
	this.Message = new MessageObject();
	this.Login = new LoginObject();
	this.Progress = new ProgressObject();
	this.AttemptedUrl = _url;

	// post form with optional action ------------------------------------
	this.Post = function(action, id, subID) {
		/// <summary>Post page form</summary>
		/// <param name="action">Name of action supplied to hidden field</param>
		/// <param name="id">Value of ID to supply hidden field</param>
		/// <param name="subID">Value of subordinate ID to supply hidden field</param>
		if (action) { _this.SetAction(action); }
		if (id) {
			if (isNaN(id)) { _this.SetItemID(id); }
			else { _this.SetItemNumber(id); }
		}
		if (subID) { _this.SetSubItemID(subID); }

		if (typeof (_form.onsubmit) != "function" || _form.onsubmit()) {
			_form.onsubmit = null; _form.submit();
		}
	}

	this.Reload = function(ignoreCache) {
		/// <summary>Reload the page</summary>
		/// <param name="ignoreCache">Ignore client browser cache</param>
		window.location.reload(ignoreCache);
	}

	this.Exception = function(source, errors) {
		/// <summary>Display error</summary>
		/// <param name="source">Name of error source (can be null)</param>
		/// <param name="errors">Text of array of error texts</param>
		if (errors == null || errors.length == 0) { return false; }
		var message = "";
		if (typeof(errors) == "string") { errors = [errors]; }
		for (var x = 0; x < errors.length; x++) { message += " - " + errors[x] + "\n"; }
		if (source) { message = source + ":\n" + message; }
		alert(message); return true;
	}

	function PreFetchImages() {
		/// <summary>Setup client cache of mouse-over images</summary>
		var _cached = new Array();
		var _re = /\.(png|gif|jpg)/gi;
		var _images = document.getElementsByTagName('img');
		var _input = document.getElementsByTagName('input');

		for (var x = 0; x < _images.length; x++) { CacheSrc(_images[x]); }
		for (var x = 0; x < _input.length; x++) {
			if (_input[x].getAttribute('type') == "image") { CacheSrc(_input[x]); }
		}
		function CacheSrc(img) {
			var imagePath = null;
			if (typeof(img.style.filter) == "string") {
				var re = /src=[\'\"](.*)[\'\"],/i;
				var matches = re.exec(img.style.filter);
				if (matches != null) { imagePath = matches[1]; }
			} else {
				imagePath = img.getAttribute('src');
			}
			if (imagePath != null) {
				var imageName = imagePath.substring(imagePath.lastIndexOf("/") + 1, imagePath.length);
				if (imageName.substr(0,4) == "btn_" && imageName.indexOf("_on.") == -1) {
					_cached.push(new Image());
					_cached[_cached.length - 1].src = imagePath.replace(_re, "_on.$1");
				}
			}
		}
	}

	function AddLibrary(file) {
		/// <summary>Load external script file</summary>
		/// <param name="file">Name of file without extension</param>
		var lib = document.createElement("script");
		lib.src = file+".js";
		document.body.appendChild(lib);
	}

	function FixIeFilterPath() {
		/// <summary>Correct IE CSS filter attributes</summary>
		var match = navigator.appVersion.match(/MSIE (\d+\.\d+)/, '');
		if (match && Number(match[1]) >= 5.5 && InSubFolder()) {
			for (var x = 0;	x < document.styleSheets.length; x++) {
				UpdateFilterRules(document.styleSheets[x]);
			}
		}
		function UpdateFilterRules(styleSheet) {
			var rules = styleSheet.rules;
			var imports = styleSheet.imports;
			var filter, re;
			for (var x = 0; x < rules.length; x++) {
				filter = rules[x].style["filter"];
				re = /(\(src=['"])(\.+)/;
				if (filter && re.test(filter)) {
					match = re.exec(filter);
					if (match[2].length == 1) {
						filter = filter.replace(re, "$1$2.");
						rules[x].style["filter"] = filter;
					}
				}
			}
			// recurse through imported sheets
			for (var x = 0; x < imports.length; x++) { UpdateFilterRules(imports[x]); }
		}
		function InSubFolder() { var re = /admin|sony/; return re.test(location.href); }
	}

	function UpdateBackgrounds(styleSheet) {
		var rules = styleSheet.rules;
		var imports = styleSheet.imports;
		var url, re, match, path;

		for (var x = 0; x < rules.length; x++) {
			url = rules[x].style["backgroundImage"];
			re = /url\((.+\/\w{3,8}\.ashx.+)\)/;

			if (url && re.test(url)) {
				match = re.exec(url);
				path = match[1];
				if (path.length > 10) {
					path = path.replace("..", ".");
					rules[x].style["backgroundImage"] = "url(../images/blank.gif)";
					rules[x].style["filter"] = "progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + path + "', sizingMethod='scale')";
				}
			}
		}
		// recurse through imported sheets
		for (var x = 0; x < imports.length; x++) { UpdateBackgrounds(imports[x]); }
	}

	// sent to given url or evaluate credentials -------------------------
	this.Redirect = function(url, page, action) {
		/// <summary>Send client to given page</summary>
		/// <param name="url">Target page (may be null)</param>
		/// <param name="page">.aspx page to view after credentials test</param>
		/// <param name="action">Page action to append to query string</param>
		if (!url && _url.length > 0) {
			// continuing from login
			location.href = _url;
		} else if (/(aspx|html)(\?|\b)/.test(url)) {
			if (action) {
				url += ((/\?/.test(url)) ? "&" : "?");
				url += "action=" + action;
			}
			location.href = url;
		} else {
			// assume the given URL is actually a page class name
			_url = ((page) ? page : url + ".aspx");
			var request = new ServerCall(url, {url: _url}, "TryView");
			request.OnComplete = RedirectResponse;
			request.IsPageType = true;
			request.Start();
		}
	}
	function RedirectResponse(response) {
		/// <summary>Evaluate response to redirect attempt</summary>
		/// <remarks>match Idaho.Web.Page.ViewResult enum</remarks>
		result = { Granted: 1, Denied: 0, NeedLogin: -1 };
		if (response && response.length > 0) {
			switch (response[0] * 1) {
				case result.Granted: location.href = unescape(response[1]); break;
				case result.Denied: _this.Message.Show(unescape(response[1])); break;
				case result.NeedLogin:
					_this.Login.Text(unescape(response[1]));
					_this.Login.Show();
			}
		}
	}

	this.SetAction = function(id) { DOM.SetValue("action", id); }
	this.SetItemID = function(id) { DOM.SetValue("id", id); }
	this.SetItemNumber = function(id) { DOM.SetValue("number", id); }
	this.SetSubItemID = function(id) { DOM.SetValue("subid", id); }

	this.OnKeyPress = function(fn, key) {
		/// <summary>Attach function to key</summary>
		/// <remarks>See Idaho.KeyCode in Utility.cs</remarks>
		if (fn && key) {
			AddHandler(KeyDown, document, "keydown");
			_keyHandler[key] = fn;
		} else {
			RemoveHandler(KeyDown, document, "keydown");
			_keyHandler = new Object();
		}
	}
	this.AddClickKey = function(node, key) {
		/// <summary>Assign key press to click event for node</summary>
		var n = DOM.GetNode(node);
		if (n) { _this.OnKeyPress(n.click, key); }
	}
	this.OnEnterKey = function(fn) {
		/// <summary>Assign enter key press to function</summary>
		_this.OnKeyPress(fn, 13);
	}

	function KeyDown(e) {
		if (!e) { e = window.event; }
		// execute handler if defined
		if (_keyHandler[e.keyCode]) { _keyHandler[e.keyCode](); }
	}

	this.Modal = function(block) {
		/// <summary>Abstract function to make modal dialog</summary>
		/// <param name="block">Block actions on page (modal)</param>
	}

	this.ShowDialog = function(controlType, modal) {
		/// <summary>Abstract function to display dialog</summary>
		/// <param name="controlType">.Net type name of dialog control</param>
		/// <param name="block">Block actions on page (modal)</param>
	}

	function GarbageCollect() {
		/// <summary>Release memory</summary>
		_this.Message.GarbageCollect();
		_this.Login.GarbageCollect();
		_this.Progress.GarbageCollect();
		_this = _form = _keyHandler = null;
	}
}

// display message in common node ----------------------------------------
function MessageObject() {
	this.OnLoad = function() { }
	this.Text = function() { alert("Message.Text: " + NA); }
	this.Hide = function() { alert("Message.Hide: " + NA); }
	this.Show = function(text) {
		text = text.replace(/\<br\/{0,1}\>/, "\n");
		alert(text);
	}
	this.GarbageCollect = function() { }
}

// handle login events ---------------------------------------------------
function LoginObject() {
	this.Show = function() { alert("Login.Show: " + NA); }
	this.Cancel = function() { alert("Login.Cancel: " + NA); }
	this.Text = function() { alert("Login.Text: " + NA); }
	this.Process = function() { alert("Login.Process: " + NA); }
	this.Clear = function() { alert("Login.Clear: " + NA); }
	this.GarbageCollect = function() { }
}

// show animation to indicate progress -----------------------------------
function ProgressObject() {
	this.Start = function(message) { }
	this.End = function() { }
	this.GarbageCollect = function() { }
}

// enum to match base Page -----------------------------------------------
var Action = {
	None: 0,
	Add: 2,
	Update: 4,
	Delete: 8,
	Next: 16,
	Previous: 32,
	Finish: 64,
	Load: 128
};