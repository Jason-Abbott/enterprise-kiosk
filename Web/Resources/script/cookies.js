/// <reference path="extensions.js"/>

function Cookie() {
	/// <summary>Manage browser cookies</summary>
	var _this = this;
	var _supported = (typeof document.cookie == "string");
	var _expires = new Date("January 1, 2020 12:00:00");

	this.Get = function(name) {
		/// <summary>Get value of cookie with given name</summary>
		/// <returns>String</returns>
		if (_supported) {
			var cookies = unescape(document.cookie);
			var re = new RegExp(name + "=([^;]+)");
			if (re.test(cookies)) { return unescape(re.exec(cookies)[1]); }
		}
		return null;
	}
	this.Set = function(name, value, expires, path, domain, secure) {
		/// <summary>Set cookie value</summary>
		/// <param name="name">Name of cookie</param>
		/// <param name="value">Value of cookie</param>
		/// <param name="expires">Expiration date (otherwise never)</param>
		/// <param name="path">Limit to path (otherwise none)</param>
		/// <param name="domain">Limit to domain (otherwise none)</param>
		/// <param name="secure">Not sure</param>
		if (_supported) {
			expires = ((expires) ? expires : _expires);
			document.cookie = name + "=" + escape(value) +
				((expires) ? ";expires=" + expires.toGMTString() : "") +
				((path) ? ";path=" + path : "") +
				((domain) ? ";domain=" + domain : "") +
				((secure) ? ";secure" : "");
			//alert(expires.toGMTString());
		}
	}
	this.Delete = function(name, path, domain) {
		/// <summary>Delete cookie</summary>
		/// <param name="name">Name of cookie</param>
		/// <param name="path">Limit to path (otherwise all)</param>
		/// <param name="domain">Limit to domain (otherwise all)</param>
		if (_this.Get(name)) {
			document.cookie = name + "=" +
			((path) ? ";path=" + path : "") +
			((domain) ? ";domain=" + domain : "") +
			";expires=Thu, 01-Jan-70 00:00:01 GMT";
		}
	}
}