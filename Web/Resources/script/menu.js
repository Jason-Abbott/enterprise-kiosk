/// <reference path="extensions.js"/>

Hover = function() {
	var nodes = document.getElementById("menu").getElementsByTagName("LI");
	for (var i=0; i < nodes.length; i++) {
		nodes[i].onmouseover=function() { this.className+=" hover"; }
		nodes[i].onmouseout=function() {
			this.className=this.className.replace(new RegExp(" hover\\b"), "");
		}
	}
}
if (window.attachEvent) window.attachEvent("onload", Hover);