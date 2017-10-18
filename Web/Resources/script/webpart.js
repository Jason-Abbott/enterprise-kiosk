/// <reference path="extensions.js"/>

function WebPartObject() {
	var _cookie = new Cookie();
	var _display = ["none","block"];	// style.display
	
	this.Toggle = function(image, id) {
		var body = DOM.GetNode(id, true);
		var newState = (body.style.display == "none") ? 1 : 0;
		ChangeState(image, id, newState);
		_cookie.Set(id, newState);
	}
	this.LoadPreferences = function() {
		//pnlWindows
		var webParts = DOM.GetNode("sideBar", true).childNodes;
		for (var x = 0; x < webParts.length; x++) {
			if (webParts[x].className == "webPart") { LoadState(webParts[x]); }
		}
	}
	this.HideAll = function() {
		var webParts = DOM.GetNode("sideBar", true).childNodes;
		for (var x = 0; x < webParts.length; x++) {
			if (webParts[x].className == "webPart") { Hide(webParts[x]); }
		}
	}
	function Hide(webPart) {
		var content = webPart.childNodes;
		var nodeID;
		var image = null;
		
		for (var x = 0; x < content.length; x++) {
			if (content[x].className == "titleBar") {
				for (var y = 0; y < content[x].childNodes.length; y++) {
					if (content[x].childNodes[y].nodeName == "IMG") {
						image = content[x].childNodes[y]; continue;
					}
				}
			} else if (content[x].className == "body") {
				nodeID = content[x].id;
			}
		}
		if (image != null && !/sideCart/.test(nodeID)) {
			// if no image tag then this web part is not minimizable
			ChangeState(image, nodeID, 0);
		}
	
	}
	function LoadState(webPart) {
		var content = webPart.childNodes;
		var nodeID;
		var image = null;
		
		for (var x = 0; x < content.length; x++) {
			if (content[x].className == "titleBar") {
				for (var y = 0; y < content[x].childNodes.length; y++) {
					if (content[x].childNodes[y].nodeName == "IMG") {
						image = content[x].childNodes[y]; continue;
					}
				}
			} else if (content[x].className == "body") {
				nodeID = content[x].id;
			}
		}
		if (image != null) {
			// if no image tag then this web part is not minimizable
			var newState = parseInt(_cookie.Get(nodeID));
			if (isNaN(newState)) { newState = 1; }
			ChangeState(image, nodeID, newState);
		}
	}
	function ChangeState(image, id, newState) {
		var body = DOM.GetNode(id, true);
		var oldState = (newState == 1) ? 0 : 1;
		var display = body.style.display;
		
		if (body.style.display != _display[newState]) {
			body.style.display = _display[newState];
			DOM.SetToggle(image, (newState == 1));
		}
	}
}