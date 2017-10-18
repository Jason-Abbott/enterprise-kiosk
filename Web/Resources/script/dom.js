/// <reference path="extensions.js"/>

var DOM = new DomObject();
Button = DOM.Button;

function DomObject() {
	/// <summary>Standard Document Object Model methods</summary>
	var _this = this;
	var _fading = new Object();
	var _namespace = null;

	this.Show = function(id) {
		/// <summary>Retrieve and change style of given node to be visible</summary>
		_this.GetNode(id, true).style.display = "block";
	}
	this.ClearTimer = function(timer) {
		/// <summary>Clear the given interval timer</summary>
		if (timer) { window.clearInterval(timer); timer = null; }
	}
	this.CurrentDate = function() {
		/// <summary>Current date formatted as mm/dd/yy hh:mm:ss</summary>
		/// <returns>String</returns>
		var d = new Date();
		return ((d.getMonth() + 1) + "/" + d.getDate() + "/" + d.getFullYear() + " "
			+ d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds() + "." + d.getMilliseconds());
	}
	this.NormalizeEvent = function(e) {
		/// <summary>Standardize event members between browsers</summary>
		/// <param name="e">The generated event</param>
		/// <returns>Event object</returns>
		e = e ? e : window.event;
	    if (!e.preventDefault) { e.preventDefault = function () { return false; } }
		if (!e.stopPropagation) { e.stopPropagation = function () {
			if (window.event) { e.cancelBubble = true; } } }
		return e;
	}

	this.InsertBeforeLast = function(parent, child) {
		/// <summary>Insert new node before last node in parent collection</summary>
		/// <param name="parent">Node being inserted within</param>
		/// <param name="child">New node being added</param>
		parent.insertBefore(child, parent.childNodes[parent.childNodes.length - 2]);
	}
	this.ShowSuperNodes = function(node, visible) {
		/// <summary>Hide nodes that IE won't allow visible overlapping</summary>
		/// <param name="node">The node that needs to be visible</param>
		/// <param name="visible">Whether the node is to be displayed or hidden</param>
		var nodes = document.getElementsByTagName("select");
		for (var x = 0; x < nodes.length; x++) {
			if (_this.Overlap(node, nodes[x])) {
				// only applies to IE so use IE-specific style
				nodes[x].style.visibility = (visible) ? "visible" : "hidden";
			}
		}
		nodes = null;
	}
	this.ClickedIn = function(node, event) {
		/// <summary>Did click occur within given node</summary>
		/// <param name="node">Node tested for click activity</param>
		/// <param name="event">Event object containing click information</param>
		/// <returns>Boolean</returns>
		var rect = new Rectangle(node);
		var x = event.clientX;
		var y = event.clientY;
		return (y > rect.Top && y < rect.Bottom && x > rect.Left && x < rect.Right)
	}
	this.Overlap = function(node1, node2) {
		/// <summary>Do the two nodes overlap</summary>
		/// <returns>Boolean</returns>
		var rect1 = new Rectangle(node1);
		var rect2 = new Rectangle(node2);
		return (
			rect1.Bottom > rect2.Top && rect1.Top < rect2.Bottom &&
			rect1.Right > rect2.Left && rect1.Left < rect2.Right);
	}
	this.Window = function() {
		/// <summary>Store window size in .Width and .Height properties</summary>
		var _width;
		var _height;
		if (self.innerWidth) {
			_width = self.innerWidth;
			_height = self.innerHeight;
		} else if (document.documentElement && document.documentElement.clientWidth) {
			_width = document.documentElement.clientWidth;
			_height = document.documentElement.clientHeight;
		} else if (document.body) {
			_width = document.body.clientWidth;
			_height = document.body.clientHeight;
		}
		this.Width = _width;
		this.Height = _height;
	}
	this.Rectangle = function(node) {
		/// <summary>Create Rectangle object encapsulating node</summary>
		/// <returns>Rectangle</returns>
		if (!node) { Page.Exception("DOM.Rectangle", "Unable to get coordinates for null node"); }
		else { return new Rectangle(node); }
	}
	function Rectangle(node) {
		if (!node) { return; }
		var _left = 0;
		var _top = 0;
		var _scrollTop = 0;
		var _scrollLeft = 0;
		var _width = node.offsetWidth;		// this will be 0 for display: none
		var _height = node.offsetHeight;
		
		while (node.offsetParent) {
			_left += (node.offsetLeft - node.scrollLeft);
			_top += (node.offsetTop - node.scrollTop);
			_scrollTop += node.scrollTop;
			_scrollLeft += node.scrollLeft;
			node = node.offsetParent;
		}
		this.Left = _left;
		this.Top = _top;
		this.Right = _left + _width;
		this.Bottom = _top + _height;
		this.Middle = _top + (_height / 2);
		this.Height = _height;
		this.Width = _width;
		this.ScrollTop = _scrollTop;
		this.ScrollLeft = _scrollLeft;
	}
	this.SetToggle = function(node, open) {
		/// <summary>Swap image source to represent toggle state</summary>
		/// <param name="node">Node containing the image</param>
		/// <param name="open">Toggle state (open or closed)</param>
		var imageSuffix = ["+.","-."];
		var oldState = (open) ? 0 : 1;
		var newState = (open) ? 1 : 0;
		if (typeof(node.style.filter) == "string") {	// IE
			node.style.filter = node.style.filter.replace(imageSuffix[oldState], imageSuffix[newState]);
		} else {
			node.src = node.src.replace(imageSuffix[oldState], imageSuffix[newState]);	
		}
	}
	this.FadeOut = function(node) {
		/// <summary>Fade out the given node at a default speed</summary>
		_this.Fade(node, 50, null, null, false);
	}
	this.Fade = function(node, speed, step, endAt, fadeIn) {
		/// <summary>Fade node in or out</summary>
		/// <param name="node">Node to be faded</param>
		/// <param name="speed">Interval speed in milliseconds</param>
		/// <param name="step">Number of steps (resolution) to target opacity</param>
		/// <param name="endAt">Target opacity between 0 and 1</param>
		/// <param name="fadeIn">Is node fading in or out</param>
		if (node == null) { return; }
		_this.CancelFade(node);
		function FadeNode() {						// create a closure
			this.timer = window.setInterval("DOM.FadeLoop('" + node.id + "')", speed);
			this.node = node;
			this.fadeIn = (fadeIn) ? true : false;	// make explicit boolean
			this.fadeStep = (step) ? step : 0.1;
			this.endOpacity = (endAt) ? endAt : ((fadeIn) ? 0.999 : 0);
			this.currentOpacity = (fadeIn) ? 0 : _this.GetOpacity(node, 0.999);
		} 
		_fading[node.id] = new FadeNode();
	}
	this.FadeLoop = function(nodeID) {
		/// <summary>Updade node opacity in interval loop</summary>
		var fadeNode = _fading[nodeID];
		if ((fadeNode.fadeIn && (fadeNode.currentOpacity < fadeNode.endOpacity)) || (!fadeNode.fadeIn && (fadeNode.currentOpacity > fadeNode.endOpacity))) {
			_this.SetOpacity(fadeNode.node, fadeNode.currentOpacity);
			fadeNode.currentOpacity += (fadeNode.fadeIn) ? fadeNode.fadeStep : -fadeNode.fadeStep;
		} else if (fadeNode.node) {
			_this.ClearTimer(fadeNode.timer);
			fadeNode.node.style.display = (fadeNode.endOpacity == 0 && !fadeNode.fadeIn) ? "none" : "block";
			fadeNode.node = null;
			_fading[nodeID] = null;
		}
	}
	this.CancelFade = function(node) {
		/// <summary>Cancel fading interval timer for given node</summary>
		if (node == null) { return; }
		if (_fading[node.id]) {
			_this.ClearTimer(_fading[node.id].timer); _fading[node.id] = null;
		}
	}
	// http://www.sitepoint.com/blog-post-view.php?id=211431
	this.SetOpacity = function(node, level) {
		/// <summary>Set style opacity on given node</summary>
		/// <param name="level">Target opacity between 0 and 1</param>
		with (node.style) {
			filter = "alpha(opacity=" + (100 * level) + ")";	// IE
			KHTMLOpacity = level;								// Konqueror, old Safari
			MozOpacity = level;									// old Mozilla
			opacity = level;									// W3C
		}
	}
	this.GetOpacity = function(node, nom) {
		/// <summary>Get the style opacity of given node</summary>
		/// <param name="nom">Nominal opacity of no attribute found</param>
		/// <returns>Float</returns>
		if (!node) { return nom; }
		if (node.style && node.style.opacity) { return node.style.opacity * 1; }
		if (node.style && node.style.filter && /alpha\(/.test(node.style.filter)) {
			var re = /opacity=(\d+)/;
			var matches = re.exec(node.style.filter);
			return (matches[1] / 100);
		}
		return nom;
	}
	this.ClassForId = function(id, className) {
		/// <summary>Set style class name on node with given ID</summary>
		document.getElementById(id).className = className;
	}
	this.Button = function(img, active) {
		/// <summary>Set image rollover for button node</summary>
		/// <param name="img">Node object for image</param>
		/// <param name="active">Whether button is active</param>
		var useFilter = (typeof (img.style.filter) == "string" &&
			img.style.filter.length > 0);
		var re = (active) ? /\.(png|gif|jpg)/ : /_on\.(png|gif|jpg)/;
		var changeTo = (active) ? "_on.$1" : ".$1";

		if (useFilter) {
			img.style.filter = img.style.filter.replace(re, changeTo);
		} else {
			img.src = img.src.replace(re, changeTo);
		}
	}
	this.InferNamespace = function(greedy) {
		/// <summary>Infer .Net namespace so subsequent node searches can be exact</summary>
		/// <param name="greedy">If greedy then namespace is everything to last underscore</param>
		/// <returns>String</returns>
		var elements = Page.Form.elements;
		var re = (greedy) ? /^_[^_]+_\w+$/ : /^_[^_]+_[^_]+$/;
		for (var x = 0; x < elements.length; x++) {
			if (re.test(elements[x].id)) {
				id = elements[x].id;
				return id.substr(0, id.lastIndexOf("_") + 1);
			}
		}
		return null;
	}
	this.ClearSelection = function(node) {
		/// <summary>Clear all selections from list</summary>
		Select(node, false);
	}
	this.SelectList = function(node) {
		/// <summary>Select all items in list</summary>
		Select(node, true);
	}
	function Select(node, selected) {
		/// <summary>Clear or select all items in list</summary>
		if (typeof(node) == "string") { node = _this.GetNode(node); }
		if (node) {
			for (var x = 0; x < node.options.length; x++) {
				node.options[x].selected = selected;
			}
		}
	}
	this.DisableCheckbox = function(node, disabled) {
		/// <summary>Disable Controls.Field type of checkbox</summary>
		node.disabled = disabled;
		node.nextSibling.className = (disabled) ? "disabled" : "";
	}
	this.SetError = function(node) {
		/// <summary>Apply style and focus to node in error</summary>
		node.className += " error";
		if (node.select) { node.select(); }
	}
	this.ClearError = function(node) {
		/// <summary>Remove error styling from node</summary>
		node.className = node.className.replace("error", "");
	}
	this.ListTransfer = function(sourceID, targetID, noError) {
		/// <summary>Transfer selected items from one list to another</summary>
		/// <param name="sourceID">ID of source list</param>
		/// <param name="targetID">ID of target list</param>
		/// <param name="noError">Suppress error messages</param>
		var source = _this.GetNode(sourceID);
		var target = _this.GetNode(targetID);
		var option;
		// clones can be used by the handler to rollback changes
		var oldTarget = target.cloneNode(true);
		var oldSource = source.cloneNode(true)
	
		if (!source || !target) {
			if (!noError) { Page.Exception("DOM.ListTransfer", "Unable to find " + sourceID + " or " + targetID); }
			return;
		 }
		_this.ClearSelection(target);
		
		for (var x = 0; x < source.options.length; x++) {
			if (source.options[x].selected) {
				option = source.options[x];
				target.options[target.options.length] = new Option(option.text, option.value, false, true);
			}
		}
		// remove every selected source item
		for (var x = source.options.length - 1; x >= 0; x--) {
			if (source.options[x].selected) {
				if (x < source.options.length - 1) {
					// move other items up
					with (source.options[x]) {
						text = source.options[x + 1].text;
						value = source.options[x + 1].value;
						selected = false;
					}
					source.options[x + 1] = null;
				} else {
					// removing last item
					source.options[x] = null;
				}
			}
		}
		_this.OnListTransfer(source, target, oldSource, oldTarget);
	}
	this.OnListTransfer = function(source, target, oldSource, oldTarget) {
		/// <summary>Abstract function called after ListTransfer()</summary>
		/// <param name="source">Node object for source list</param>
		/// <param name="target">Node object for target list</param>
		/// <param name="oldSource">Clone of source object before transfer</param>
		/// <param name="oldTarget">Clone of target object before transfer</param>
	}
	
	this.MoveListItem = function(node, distance, noError) {
		/// <summary>Move item up or down in list</summary>
		/// <param name="node">Reference to or ID of select node</param>
		/// <param name="distance">Index distance to move selected item</param>
		/// <param name="noError">Suppress error messages</param>
		var name = "";
		if (typeof(node) == "string") { name = node; node = _this.GetNode(node); }
		if (!node || !/^select-/.test(node.type)) {
			if (!noError) { MissingElementException("MoveListItem", name); }
			return;
		}
		var index = node.selectedIndex;
		var length = node.options.length;
		var newIndex = index + distance;
		
		if (index < 0) {
			alert("Please select the item you would like to move");
			return;		
		}
		if (newIndex >= 0 && newIndex < length) {
			// otherwise movement is outside of list
			var value = node.options[index].value;
			var text = node.options[index].text;
			var add = (distance < 0) ? -1 : 1;
			
			for (var x = index; ((add < 0) ? (x > newIndex) : (x < newIndex)); x += add) {
				node.options[x].value = node.options[x + add].value;
				node.options[x].text = node.options[x + add].text;
			}
			node.options[index].selected = false;
			node.options[newIndex].value = value;
			node.options[newIndex].text = text;
			node.options[newIndex].selected = true;
			
			_this.OnMoveListItem(node);	
		}
	}
	this.OnMoveListItem = function(node) {
		/// <summary>Abstract function called after MoveListItem()</summary>
	}

	this.SetSelected = function(node, value) {
		/// <summary>Select item in list with given value</summary>
		/// <returns>Boolean (was item found)</returns>
		if (node && /^select-/.test(node.type)) {
			if (typeof(value) == "object") {
				for (var x = 0; x < value.length; x++) {
					_this.SetSelected(node, value[x]);
				}
				return true;
			} else {
				for (var x = 0; x < node.options.length; x++) {
					if (node.options[x].value == value) {
						node.options[x].selected = true;
						return true;
					}
				}
			}
		}
		return false;
	}

	this.RemoveSelected = function(node, exact) {
		/// <summary>Remove the selected item from the list</summary>
		/// <param name="node">Reference to or ID of input node</param>
		var name = "";
		if (typeof (node) == "string") { name = node; node = _this.GetNode(node); }
		if (!node) { MissingElementException("RemoveSelected", name); return false; }

		if (node.selectedIndex && node.selectedIndex >= 0) {
			if (node.options.length - 1 > node.selectedIndex) {
				// following options must be moved up
				for (var x = node.selectedIndex; x < node.options.length - 1; x++) {
					node.options[x].value = node.options[x + 1].value;
					node.options[x].text = node.options[x + 1].text;
				}
			}
			node.length = node.length - 1;
			return true;
		}
		return false;
	}
	
	this.SetValue = function(node, input, exact) {
		/// <summary>Set value of node</summary>
		/// <param name="node">Reference to or ID of input node</param>
		/// <param name="exact">If given node ID, is it exact</param>
		/// <returns>Boolean (was value set)</returns>
		var name = "";
		if (typeof(node) == "string") { name = node; node = _this.GetNode(node, exact); }
		if (!node) { MissingElementException("SetValue", name); return false; }
		switch (node.type) {
			case "select-one":
			case "select-multiple":
				return _this.SetSelected(node, input);
			case "text":
			case "hidden":
				node.value = input;
				FireEvent(node, "change");
				return true;
			case "radio":
			case "checkbox":
				node.checked = (input);
				return true;
		}
		return false;
	}

	function FireEvent(node, type) {
		/// <summary>Fire DOM event of type on node</summary>
		/// <remarks>
		/// When multiple handlers are attached to an event the normal
		/// on[event]() methods don't work consistently
		/// http://jehiah.cz/archive/firing-javascript-events-properly
		/// </remarks>
		/// <param name="type">Name of event excluding "on"</param>
		if (document.createEventObject) {
			// IE
			var e = document.createEventObject();
			return node.fireEvent('on' + type, e)
		} else {
			// FF and others
			var e = document.createEvent("HTMLEvents");
			e.initEvent(type, true, true); // event type, bubbling, cancelable
			return !node.dispatchEvent(e);
		}
	}
	
	this.GetName = function(node, noError, exact) {
		/// <summary>Generically retrieve name of selected item in list</summary>
		/// <param name="node">Reference to or ID of select node</param>
		/// <param name="exact">If ID given, is it exact</param>
		/// <param name="noError">Suppress error messages if node not found</param>
		/// <returns>String</returns>
		var name = "";
		if (typeof(node) == "string") { name = node; node = _this.GetNode(node, exact); }
		if (!node) {
			if (!noError) { MissingElementException("GetName", name); }
			return null;
		}
		switch (node.type) {
			case "select-one" :
				return node.options[node.selectedIndex].text;
			case "select-multiple":
				var result = "";
				for (var x = 0; x < node.options.length; x++) {
					if (node.options[x].selected) { result += node.options[x].text + ","; }
				}
				if (result.length > 0) { result = result.substring(0, result.length - 1 ); }
				return result;
			default:
				return node.value;
		}
	}

	this.GetValue = function(node, noError, exact) {
		/// <summary>Generically retrieve value from input node</summary>
		/// <param name="node">Reference to or ID of node</param>
		/// <param name="exact">If ID given, is it exact</param>
		/// <param name="noError">Suppress error messages if node not found</param>
		/// <returns>String</returns>
		var name = "";
		if (typeof (node) == "string") { name = node; node = _this.GetNode(node, exact); }
		if (!node) {
			if (!noError) { MissingElementException("GetValue", name); }
			return null;
		}
		switch (node.type) {
			case "select-one":
				return node.options[node.selectedIndex].value;
			case "select-multiple":
				var result = "";
				for (var x = 0; x < node.options.length; x++) {
					if (node.options[x].selected) { result += node.options[x].value + ","; }
				}
				if (result.length > 0) {
					result = result.substring(0, result.length - 1);
				}
				return result;
			case "text":
			case "hidden":
				return node.value;
			case "radio":
				return (node.checked) ? node.value : false;
			case "checkbox":
				return node.checked;
			default:
				if (node.tagName == "DIV") {
					// ajax controls have an object name equal to the node ID
					var control = eval(node.id);
					if (control && control.GetValue) {
						// get value reported by control object
						// (usually Idaho.Web.Controls.PanelList)
						return control.GetValue();
					} else {
						// get value of all checkboxes in control
						var boxes = node.getElementsByTagName("INPUT");
						if (boxes) {
							var result = "";
							for (var x = 0; x < boxes.length; x++) {
								if (boxes[x].checked) { result += boxes[x].value + ","; }
							}
							if (result.length > 0) {
								result = result.substring(0, result.length - 1);
							}
							return result;
						}
					}
				}
				return node.value;
		}
	}
	function MissingElementException(method, name) {
		/// <summary>Raise message indicating inability to find element of given name</summary>
		/// <param name="method">Name of the method that caused error</param>
		Page.Exception("DOM." + method, "Element " + name + " could not be found");
	}

	this.CopyContent = function(source, target, exact) {
		/// <summary>Copy HTML content of non-input element</summary>
		var sourceName="";
		var targetName="";
		
		if (typeof(source) == "string") {
			sourceName = source;
			source = _this.GetNode(sourceName, exact);
		}
		if (typeof(target) == "string") {
			targetName = target;
			target = _this.GetNode(targetName, exact);
		}
		if (!source || !target){
			Page.Exception("DOM.CopyContent","Element " + sourceName + " or " + targetName + " could not be found");
			return;
		}
		target.innerHTML = source.innerHTML;
	}

	this.GetNode = function(id, exact, startNode) {
		/// <summary>Return node reference for given ID</summary>
		/// <param name="id">The HTML ID of the node</param>
		/// <param name="exact">ID name is exact, otherwise matches partially</param>
		/// <param name="startNode">Start search at given node, otherwise starts at document</param>
		/// <returns>Node object</returns>
		if (exact == false) {
			if (_namespace != null) { 
				var node = document.getElementById(_namespace + id);
				if (node != null) { return node; }
			}
			// okay, we'll do it the hard way
			var re = new RegExp(id + "$", "i");
			var nodes = _this.GetNodesByIdMatch(re, startNode);
			if (nodes.length > 0) {
				_namespace = nodes[0].id.replace(id, "");
				return nodes[0];
			}
			return null;
		} else {
			return document.getElementById(id);
		}
	}
	this.GetChildNodes = function(node, tagName) {
		/// <summary>Return direct child nodes with tag name</summary>
		/// <param name="node">Parent node object</param>
		/// <param name="tagName">Name of tags to find</param>
		/// <returns>Object array (nodes)</returns>
		var matches = [];
		var re = new RegExp(tagName, "i");
		if (node && node.childNodes) {
			for (var x = 0; x < node.childNodes.length; x++) {
				if (re.test(node.childNodes[x].tagName)) {
					matches.push(node.childNodes[x]);
				}
			}
		}
		return matches;
	}
	this.GetFirstChild = function(node, tagName) {
		/// <summary>Return first direct child node with tag name</summary>
		/// <param name="node">Parent node object</param>
		/// <param name="tagName">Name of tag to find</param>
		/// <returns>Object (node)</returns>
		var matches	= _this.GetChildNodes(node, tagName);
		return (matches && matches.length > 0) ? matches[0] : null;
	}
	this.GetNodeWithNamespaceID = function(id, greedy) {
		/// <summary>Get node for ID known to have .Net namespace prefix</summary>
		/// <param name="greedy">If greedy then namespace is everything to last underscore</param>
		/// <returns>Node object</returns>
		var ns = _this.InferNamespace(greedy);
		return document.getElementById(ns + id);
	}
	this.GetNodesByClassName = function(className, node) {
		/// <summary>Get all nodes having a class name</summary>
		/// <param name="node">Node at which to begin search, otherwise document body is used</param>
		/// <returns>Node collection</returns>
		if (!node) { node = document.body; }
		var re = new RegExp(className, "i");
		return NodesByPatternMatch(re, node, new Array(), "className");
	}
	this.GetNodesByIdMatch = function(re, node) {
		/// <summary>Get all nodes partially matching an ID</summary>
		/// <param name="node">Node at which to begin search, otherwise document body is used</param>
		/// <returns>Node collection</returns>
		if (!node) { node = document.body; }
		return NodesByPatternMatch(re, node, new Array(), "id");
	}
	this.GetNodesByRegExp = function(re, attribute, node) {
		/// <summary>Get all nodes with attributes matching a regular expression</summary>
		/// <param name="re">Regular Expression object to match</param>
		/// <param name="attribute">Name of attribute to match</param>
		/// <param name="node">Node at which to begin search, otherwise document body is used</param>
		/// <returns>Node collection</returns>
		if (!node) { node = document.body; }
		return NodesByPatternMatch(re, node, new Array(), attribute);
	}
	function NodesByPatternMatch(re, node, nodes, attribute) {
		/// <summary>Get all nodes with attributes matching a regular expression</summary>
		/// <param name="re">Regular Expression object to match</param>
		/// <param name="attribute">Name of attribute to match</param>
		/// <param name="node">Node at which to begin search, otherwise document body is used</param>
		/// <param name="nodes">Array of matched nodes to add to (for recursion)</param>
		/// <returns>Node collection</returns>
		if (!node.childNodes) { return nodes; }
		for (var x = 0; x < node.childNodes.length; x++) {
			if (re.test(node.childNodes[x][attribute])) {
				nodes.push(node.childNodes[x]);
			}
			nodes = NodesByPatternMatch(re, node.childNodes[x], nodes, attribute)
		}
		return nodes;
	}
}