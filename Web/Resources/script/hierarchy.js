/// <reference path="extensions.js"/>
/// <reference path="dom.js"/>
/// <reference path="page.js"/>

function HierarchyObject(nodeID, cascadeDown, cascadeUp) {
	/// <summary>Encapsulate functionality of hierarchical list object</summary>
	/// <param name="nodeID">ID of node containing lists</param>
	/// <param name="cascadeDown">Item selection also selects children</param>
	/// <param name="cascadeUp">Item selection also selects parents</param>

	var _this = this;
	var _id = null;
	var _root = null; // top level UL

	AddHandler(UnloadHandler, window, "unload");

	this.OnInit = function() {
		var div = DOM.GetNode(nodeID);
		var methodName = "Hierarchy.OnInit";
		if (div) {
			_root = DOM.GetFirstChild(div, "UL");
			if (_root) {
				AddHandlers(_root);
			} else {
				Page.Exception(methodName, "Did not find expected lists in " + nodeID);
			}
		} else {
			Page.Exception(methodName, "Could not find " + nodeID);
		}
	}
	this.Clear = function() {
		/// <summary>Clear all checkboxes in list</summary>
		var boxes = _root.getElementsByTagName("INPUT");
		for (var x = 0; x < boxes.length; x++) { boxes[x].checked = false; }
	}

	function AddHandlers(ul) {
		/// <summary>Attach event handlers to list items</summary>
		/// <param name="ul">Root list node to begin attaching</param>
		var node = null;
		if (ul && ul.childNodes) {
			for (var x = 0; x < ul.childNodes.length; x++) {
				node = ul.childNodes[x];
				if (node.tagName == "LI" && node.childNodes) {
					for (var y = 0; y < node.childNodes.length; y++) {
						switch (node.childNodes[y].tagName) {
							case "INPUT":
								node.childNodes[y].onclick = ClickHandler;
								break;
							case "UL":
								AddHandlers(node.childNodes[y]);
								break;
							case "LABEL":
								node.childNodes[y].onmouseover = MouseOverHandler;
								node.childNodes[y].onmouseout = MouseOutHandler;
						}
					}
				}
			}
		}
	}

	function MouseOverHandler(e) { this.className = "active"; }
	function MouseOutHandler(e) { this.className = ""; }

	function ClickHandler(e) {
		e = DOM.NormalizeEvent(e);
		if (this && this.parentNode) {
			var checked = this.checked;
			if (cascadeDown) {
				var boxes = this.parentNode.getElementsByTagName("INPUT");
				if (boxes) {
					for (var x = 0; x < boxes.length; x++) {
						boxes[x].checked = checked;
					}
				}
			}
			if (cascadeUp) {
				//alert("up");
			}
		}
	}

	this.GetValue = function() {
		/// <summary>Build list of all selected checkbox values</summary>
		/// <returns>String</returns>
		var value = "";

		if (cascadeDown) {
			// actual selection is most generic
			value = GetTopSelections(value, _root);
		} else if (cascadeUp) {
			// actual selection is most specific
			value = GetBottomSelections(value, _root);
		} else {
			// all selected checkboxes
			var boxes = _root.getElementsByTagName("INPUT");
			for (var x = 0; x < boxes.length; x++) {
				if (boxes[x].checked) { value += boxes[x].value + ","; }
			}
		}
		if (value.length > 0) {
			value = value.substring(0, value.length - 1);
		}
		//alert(value);
		return value;
	}

	function GetTopSelections(values, ul) {
		/// <summary>Append selected values, recursing to find first selection</summary>
		if (ul) {
			var li = null;
			var box = null;
			for (var x = 0; x < ul.childNodes.length; x++) {
				// iterate over LI elements
				li = ul.childNodes[x];
				box = DOM.GetFirstChild(li, "INPUT");

				if (box.checked) {
					// accept selection
					//alert(DOM.GetFirstChild(li, "LABEL").innerHTML);
					values += box.value + ",";
				} else {
					// recurse as necessary
					values = GetTopSelections(values, DOM.GetFirstChild(li, "UL"));
				}
			}
		}
		return values;
	}

	function GetBottomSelections(values, ul) {
		Page.Exception("Hierarchy.GetBottomSelections", "Not yet implemented");
		return null;
	}

	function UnloadHandler() { _this = _root = _lists = null; }

	this.OnInit();
}