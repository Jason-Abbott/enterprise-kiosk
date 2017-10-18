/// <reference path="extensions.js"/>
/// <reference path="dom.js"/>
/// <reference path="ajax.js"/>
/// <reference path="control.js"/>

function GridObject(id, url, showRowMenu, delaySetup, checkboxEvents, mouseHighlights) {
/// <summary>Encapsulate AJAX methods for .Net grid controls</summary>
/// <param name="id">ID of control</param>
/// <param name="url">URL to view when grid item is clicked</param>
/// <param name="showRowMenu">Enable rigth-click context menu</param>
/// <param name="delaySetup">Do not immediately setup event handlers</param>
/// <param name="checkboxEvents">Render checkbox for each row to be handled</param>
/// <param name="mouseHighlights">Add mouse events to highlight rows</param>

	var _this = this;
	var _menu;
	var _rowMenuNode;
	var _headMenuNode;
	var _gridNode;
	var _rows;
	var _rowID;
	var _lastRowID = "[id]";
	var _activeRow = null;
	var _activeMenu = null;
	var _showHeadMenu;
	var _firstRow;

	this.CheckboxEvents = (checkboxEvents);
	this.MouseHighlights = (mouseHighlights);
	this.ClickMessage = null;
	this.Checkbox = null;
	this.Rows = null;
	this.Control = null;
	
	this.SetupEvents = function() {
		AddHandler(GarbageCollect, window, "unload");
		var status = "";
		_rowMenuNode = DOM.GetNode(id + "Menu", true);
		_headMenuNode = DOM.GetNode(id + "HeadingMenu", true);
		_gridNode = DOM.GetNode(id, true);
		if (!_gridNode) { return; }
		_rows = _gridNode.childNodes[0].childNodes;	// implicit TBODY is first child
		if (_rows && _rows.length > 1) {		
			_firstRow = (/head/.test(_rows[1].className)) ? 2 : 1;
		} else {
			_firstRow = 0;
		}
		if (showRowMenu) {
			_menu = new MenuObject(id + "Menu");
			_lastRowID = "[id]";
			_rowMenuNode.Rect = new DOM.Rectangle(_rowMenuNode);
		}
		if (!_menu) { showRowMenu = false; }
		if (url) {
			status = "Click to view [id] or right-click for more options";
		} else if (me.CheckboxEvents) {
			status = "Click to select [id]";
		} else {
			status = null;
		}
		// enable heading menu if node found
		if (_headMenuNode) {
			for (var x = 0; x < _firstRow; x++) {
				_rows[x].onmouseover = Activate;
				_rows[x].onmouseout = Deactivate;
				_rows[x].heading = true;
			}
			_headMenuNode.Rect = new DOM.Rectangle(_headMenuNode);
			_showHeadMenu = true;
			var button = DOM.GetNode(id + "HeadingMenuButton", true)
			button.onclick = SelectColumns;
		} else {
			_showHeadMenu = false;
		}
		for (var x = _firstRow; x < _rows.length; x++) {
			if (me.MouseHighlights) {
				_rows[x].onmouseover = Activate;
				_rows[x].onmouseout = Deactivate;
			}
			_rows[x].heading = false;
			_rows[x].Activate = Activate;
			_rows[x].Checkbox = DOM.GetNode("cbx" + _rows[x].id, true, _rows[x]);

			if (url) {
				_rows[x].onclick = function() {
					if (me.ClickMessage) { Page.Progress.Start(me.ClickMessage); }
					if (/\(.+\)$/.test(url)) {	// if parenthesis present, must be function
						eval(url.replace("[id]", _rowID));
					} else {
						Page.Redirect(url + "?id=" + _rowID);
					}
				}
			} else if (me.CheckboxEvents) {
				// if no specified click action then have click check box
				_rows[x].onclick = function() {
					var node = DOM.GetNode("cbx" + this.id, true, this);	
					node.checked = !node.checked;
				}
				// cancel event bubble
				if (_rows[x].Checkbox) {
					_rows[x].Checkbox.onclick = function(e) {
						if (!e) { e = window.event; }
						e.cancelBubble = true;
					}
				}
			}
		}
		if (me.CheckboxEvents) {	// setup listener for master checkbox click
			var allChecked = true;
			for (var x = 1; x < _rows.length; x++) {
				if (_rows[x].Checkbox && !_rows[x].Checkbox.checked) { allChecked = false; continue; }
			}
			_gridNode.Checkbox = DOM.GetNode(id + "Checkbox", true, _gridNode);
			_gridNode.Checkbox.onclick = ToggleCheckboxes;
			_gridNode.Checkbox.checked = allChecked;
			me.Checkbox = DOM.GetNode(id + "Checkbox", true, _gridNode); //_gridNode.Checkbox;
		}
		me.Rows = _rows;
	}
	
	if (!delaySetup) { _this.SetupEvents(); }
	
	function Activate() {
	/// <summary>Activate row style and context menu</summary>
		if (_activeRow) { Deactivate(_activeRow); }
		if (_activeMenu == null) {
			_activeRow = this;
			if (!this.heading) {
				this.className += " over";
				_rowID = this.id;
				if (status) { window.status = status.replace("[id]", _rowID); }
				if (showRowMenu) { ContextMenu(true); }
			} else {
				_activeRow = _rows[_firstRow - 1];
				if (_showHeadMenu) { ContextMenu(true); }
			}
		}
	}
	function Deactivate(node) {
		if (!node) { node = this; }
		if (_activeMenu == null) {
			if (!this.heading) {
				node.className = node.className.replace("over", "");
				_rowID = null;
				window.status = "";
				if (showRowMenu) { ContextMenu(false); }
			} else {
				if (_showHeadMenu) { ContextMenu(false); }
			}
			_activeRow = null;
		}
	}

	function ToggleCheckboxes() {
	/// <summary>Toggle all row checkboxes in response to master</summary>
		var checkbox;
		for (var x = 1; x < _rows.length; x++) {
			checkbox = DOM.GetNode("cbx" + _rows[x].id, true)
			if (checkbox) { checkbox.checked = this.checked; }
		}
	}

	this.Selected = function() {
	/// <summary>Get IDs of all selected rows</summary>
	/// <returns>Array</returns>
		var selected = [];
		if (!me.CheckboxEvents) { return selected; }
		for (var x = 1; x < _rows.length; x++) {
			if (DOM.GetNode("cbx" + _rows[x].id, true).checked) {
				selected.push(_rows[x].id);
			}
		}
		return selected;
	}
	this.ClearSelection = function() {
		if (!me.CheckboxEvents) { return; }
		for (var x = 1; x < _rows.length; x++) {
			DOM.GetNode("cbx" + _rows[x].id, true).checked = false;
		}
	}

	function ContextMenu(enable) {
	/// <summary>Enable or disable the context menu</summary>
		if (enable) {
			document.oncontextmenu = (_activeRow.heading) ? ShowHeadMenu : ShowRowMenu;
			document.onclick = HideMenu;
		} else {
			document.oncontextmenu = null;
			document.onclick = null;
		}
	}

	function ShowHeadMenu() {
	/// <summary>Show the header menu</summary>
		ShowMenu(_headMenuNode, 1, event);	
		return false;
	}
	function ShowRowMenu() {
		// substitute row ID for place-holder [id] in menu text
		_menu.ShowItem("Activate", (!/pending/.test(_activeRow.className)));
		_menu.Replace(_lastRowID, _rowID);
		_lastRowID = _rowID;
		ShowMenu(_rowMenuNode, -5, event);		
		return false;
	}
	function ShowMenu(menuNode, fromTop, event) {
		var rowRect = new DOM.Rectangle(_activeRow);
		var win = new DOM.Window();
		var fromLeft = event.clientX - rowRect.Left;
		var fromTop = (rowRect.Bottom - menuNode.Rect.Top) + fromTop;
		
		menuNode.style.display = "block";
		
		if (fromLeft + menuNode.offsetWidth > rowRect.Right) {
			fromLeft = (rowRect.Right - menuNode.offsetWidth) - rowRect.Left;
		}
		if (fromTop + menuNode.offsetHeight > win.Height - menuNode.Rect.Top) {
			fromTop = (win.Height - menuNode.offsetHeight) - (menuNode.Rect.Top + 1);
		}
		with (menuNode.style) { left = fromLeft + "px"; top = fromTop + "px"; }
		_activeMenu = menuNode;
	}
	function HideMenu() {
		if (_activeMenu && !DOM.ClickedIn(_activeMenu, event)) {
			// only hide if clicked outside of menu area
			with (_activeMenu.style) { top = left = "0px"; display = "none"; }
			if (_activeRow) { _activeRow.onmouseout(); }
			_activeMenu = null;
		}
	}

	function SelectColumns() {
	/// <summary>Process column selection from heading menu</summary>
		var button = this;
		var selected = 0;
		var boxes = DOM.GetElementsByID(/\d_/, _headMenuNode);
		for (var x = 0; x < boxes.length; x++) {
			if (boxes[x].checked) { selected |= boxes[x].value }
		}
		button.disabled = true;
		_activeMenu = _activeRow = null;
		_lastRowID = "[id]";
		with (me.Control) {
			ShowProgress = true;
			WaitMessage = "Changing Column Selection";
			Properties(["ShowFields=" + selected]);
			Refresh();
		}
	}

	function GarbageCollect() {
	/// <summary>Release memory</summary>
		if (_rows) {
			for (var x = _firstRow; x < _rows.length; x++) {
				_rows[x].onmouseover = null;
				_rows[x].onmouseout = null;
				_rows[x].onclick = null;
				_rows[x].Activate = null;
				if (_rows[x].Checkbox) { _rows[x].Checkbox.onclick = null; }
				_rows[x].Checkbox = null;
			}
		}
		if (_gridNode && _gridNode.Checkbox) {
			_gridNode.Checkbox.onclick = null;
			_gridNode.Checkbox = null;
			_gridNode = null;
		}
		_rows = _menu = _activeRow = null;
	}
}