/// <reference path="extensions.js"/>

var Diagram = null;
AfterPageLoad( function() { Diagram = new DiagramObject(); Diagram.OnInit(); } );

function DiagramObject() {
	var _this = this;
	this.OnInit = function() { }
	this.OnItemClick = function() { }
	this.OnItemMouseOver = function() { }
	this.OnItemMouseOut = function() { }
}