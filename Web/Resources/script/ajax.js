/// <reference path="extensions.js"/>
/// <reference path="dom.js"/>
/// <reference path="page.js"/>

function ServerCall(type, parameters, method) {
	/// <summary>Create an xmlHttp (AJAX) request</summary>
	/// <param name="type">Name of .Net type or .aspx page containing method</param>
	/// <param name="parameters">Object listing name/value pairs</param>
	/// <param name="method">Name of method to be called</param>
	var _this = this;
	var _http = GetXmlHttp();
	var _timer;

	AddHandler(GarbageCollect, window, "unload");

	this.OnComplete = function() {
		/// <summary>Abstract method called by successful callback</summary>
	};
	this.OnError = function() {
		/// <summary>Abstract method called when an error occurs</summary>
	};
	this.Service = "ajax.ashx";
	this.ShowErrors = true;
	this.IsPageType = false;
	this.Timeout = 15;	// seconds
	this.Retries = 0;	// retries after timeout
	this.Parameters = new Object();
	this.Start = function() {
		/// <summary>Start the xmlHttp call</summary>
		if (_http) {
			_this.Service += ((_this.IsPageType) ? "?page=" : "?type=") + escape(type);
			if (method) { _this.Service += "&method=" + escape(method); }
			if (_this.Timeout > 0) { _timer = setTimeout(Cancel, _this.Timeout * 1000); }

			_http.open("POST", _this.Service, true);
			_http.setRequestHeader("Content-Type","application/x-www-form-urlencoded");
			_http.onreadystatechange = function() {
				var response;
				if (_http.readyState == 4) {
					clearTimeout(_timer);
					try {
						//window.clipboardData.setData("ajax", _http.responseText);
						//alert(_http.responseText);
						response = eval(_http.responseText);
						//alert(response.Value);
					} catch(e) {
						//alert(_http.responseText);
						var error = _http.responseText;
						//error = error.substring(error.lastIndexOf("<!--") + 5, error.length - 5);
						response = { Errors:[e, error] };
					}
					if (!response) { response = { Errors:["The server did not respond"] }; }
					if (!HasErrors(response.Errors)) {
						// response is either escaped HTML or an EcmaScript literal
						var value = (response.IsHtml) ? unescape(response.Value) : response.Value;
						_this.OnComplete(value);
					}
				}
			};
			//alert(ParameterList());
			_http.send(ParameterList());
		} else {
			// unable to create object
			HasErrors( { Errors:["Unable to create server connection object"] } );
		}
	}

	function ParameterList() {
		/// <summary>Standardize formatting of parameters</summary>
		var list = "";
		var value = null;
		if (!parameters) { parameters = _this.Parameters; }
		for (p in parameters) {
			value = parameters[p];
			if (value && typeof(value) == "string" && value.length > 0) {
				value = value.replace(/(^'|'$)/g, "");
			}
			list += p + "=" + escape(value) + "&";
		}
		if (list.length > 0) { list = list.substr(0, list.length -1); }
		return list;
	}

	function Cancel() {
		/// <summary>Cancel xmlHttp call if timeout value exceeded</summary>
		_http.onreadystatechange = function() { return; };
		_http.abort();
		if (_this.Retries > 0) { _this.Retries -= 1; _this.Start(); }
		else {
			var timeout = _this.Timeout + " second" + ((_this.Timeout != 1) ? "s" : "");
			if (confirm("Error: there was no response from the server after "
				+ timeout + "\n(for " + type + ").\n\nThis may have left the screen in "
				+ "an inconsistent state.\nIf so, press OK to reload the screen.")) {
				// reload page
				location.reload(true);
			} else {
				_this.OnComplete(null);
			}
		}
	}
	function HasErrors(errors) {
		/// <summary>Check given error response for actionable errors</summary>
		/// <returns>Boolean (were errors found)</returns>
		if (errors.length == 0) { return false; }
		if (_this.ShowErrors) {
			var source = "";
			if (typeof(type) != "undefined") { source += type; }
			if (typeof(method) != "undefined") { source += "." + method; }
			if (source.length > 0) { source = " (" + source + ")"; }
			Page.Progress.End();
			Page.Modal(false);
			Page.Exception("AJAX Error" + source, unescape(errors));
		}
		_this.OnError();
		return true;
	}
	
	this.Exists = function(url) {
		/// <summary>Use xmlHttp call to verify existence of given URL</summary>
		/// <returns>Boolean (does URL exist)</returns>
		if (_http) {
			var qs = "method=Utility.UrlCheck&parameters=" + escape(url);
			_http.open("GET", _this.Service + qs, false);
			_http.send(null);
			try { return eval(_http.responseText); } catch(e) {  }
		} 
		// if something fails then default to true
		return true;
	}

	function GetXmlHttp() {
		/// <summary>Get the xmlHttp object for specific client browser</summary>
		var xmlHttp = false;

		try { xmlHttp = new XMLHttpRequest(); }
		catch (e1) {
			try { xmlHttp = new ActiveXObject("Msxml2.XMLHTTP"); } 
			catch (e2) {
				try { xmlHttp = new ActiveXObject("Microsoft.XMLHTTP"); }
				catch (e3) {
					Page.Exception("GetXmlHttp", "Unable to initialize object");
					xmlHttp = false;
				}
			}
		}
		return xmlHttp;
	}

	function GarbageCollect() {
		/// <summary>Release memory</summary>
		_http = null;
	}
}