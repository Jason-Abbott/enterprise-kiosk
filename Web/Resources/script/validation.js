/// <reference path="extensions.js"/>
/// <reference path="dom.js"/>
/// <reference path="page.js"/>
/// <reference path="ajax.js"/>

var Validation;
AfterPageLoad(InitValidation);

function InitValidation() {
	/// <summary>Create validation object for form</summary>
	/// <remarks>http://www.aplus.co.yu/scripts/validating-forms/</remarks>
	Validation = new ValidationObject();
	Validation.Fields = (typeof(Validators) == "undefined") ? new Array() : Validators();
}

function ValidationObject() {
	/// <summary>Encapsulate validation rules for input elements</summary>
	var _this = this;
	var _errors = new FieldErrors();
	this.Fields;			// array of validator objects
	this.Functions = [];	// array of custom validation functions

	Page.Form.onsubmit = function() { return _this.Check(); };
	AddHandler(GarbageCollect, window, "unload");

	this.Disable = function() {
		/// <summary>Disable form submission</summary>
		Page.Form.onsubmit = null;
	}

	this.Check = function() {
		/// <summary>Perform validation on all fields</summary>
		/// <returns>Boolean (any errors?)</returns>
		_errors.Clear();
		var e, f;
		
		for (var x = 0; x < _this.Functions.length; x++) {
			// execute all custom validation functions
			e = _this.Functions[x]();
			if (e) { _errors.Add(e); }
		}
		
		for (var x = 0; x < _this.Fields.length; x++) {
			// perform individual field validations
			f = _this.Fields[x];
			if (!f.Valid()) { _errors.Add(f); }
			else { f.ClearError(); }
		}
		if (_errors.Exist) { alert(_errors.Display()); return false; }
		else { return true; }
	}

	function FieldErrors() {
		/// <summary>All errors found by Check()</summary>
		this.Focused = false;							// has focus been set on a node
		this.Exist = false;								// do errors exist
		this.Messages = [];
		var _this = this;
		var _stringTitle = false;
		var _fieldTitle = false;

		this.Add = function(validator) {
			/// <summary>Add a new validator</summary>
			var message;
			if (typeof(validator) == "string") {		// treat parameter as string
				message = " - " + validator;
				if (!_stringTitle) {
					if (_this.Messages.length > 0) { _this.Messages.push(""); }
					_this.Messages.push("The following issues were encountered:");
					_stringTitle = true;
				}
			} else {									// treat parameter as Validator object
				message = " - " + validator.Message;
				validator.SetError();
				if (!_this.Focused) { validator.Focus(); _this.Focused = true; }
				if (!_fieldTitle) {
					if (_this.Messages.length > 0) { _this.Messages.push(""); }
					_this.Messages.push("The following fields could not be validated:");
					_fieldTitle = true;
				}
			}
			_this.Messages.push(message);
			_this.Exist = true;
		}
		this.Display = function() {
			/// <summary>Build message to display errors</summary>
			/// <returns>String</returns>
			var message = "";
			for (var x = 0; x < _this.Messages.length; x++) {
				message += _this.Messages[x] + "\n";
			}
			return message;
		}
		this.Clear = function() {
			/// <summary>Clear error message</summary
			_this.Messages.length = 0;
			_this.Exist = false;
			_stringTitle = false;
			_fieldTitle = false;
		}
	}
	
	this.Field = function(id, type, message, required) {
		/// <summary>Validation information for an input field</summary>
		/// <param name="id">Exact ID of node</param>
		/// <param name="type">Name of validation (no "Is" prefix)</param>
		/// <param name="message">Message to display when validation fails</param>
		/// <param name="required">Is the field required to post the form</param>
		var _this = this;
		var _validation = Validation["Is" + type];
		var _type = type;
		var _node;
		
		OnInit();
		
		function OnInit() {
			_node = document.getElementById(id);
			if (!_node && type == "Radio") {
				// look for node collection based on id
				_node = [];
				var node;
				for (var x = 1; x < 20; x++) {
					node = document.getElementById(id + "_" + x);
					if (node) { _node.push(node); }			
				}
			}
			if (!_node) {
				Page.Exception("Validation.Field",
					"Unable to initialize validation: no node for " + id);
			}
		}

		this.Message = message;
		this.Required = required;

		this.Valid = function() {
			/// <summary>Is field valid</summary>
			/// <returns>Boolean</returns>
			var value = "";
			if (typeof(_node.type) == "undefined" && type == "Radio") {
				return ((!_this.Required) || _validation(_node));
			}
			if (/^select-/.test(_node.type)) {
				if (_node.selectedIndex != -1) {
					value = _node.options[_node.selectedIndex].value;
				}
			} else {
				value = _node.value;
			}
			if (value.length > 0) {
				if (value == _this.IgnoreValue()) { return true; }
			} else {
				return !_this.Required;
			}
			return _validation(_node)
		}

		this.IgnoreValue = function() {
			/// <summary>Check for default values that should be ignored</summary>	
			/// <returns>String (default value)</returns>
			switch (_type) {
				case "URL":	return "http://";
				case "Select": if (!_this.Required) { return "0"; }
				default: return "";
			}
		}

		this.ClearError = function() {
			/// <summary>Clear error (style) on input field</summary>
			if (!_node.className) { return; }
			_node.className = _node.className.replace("error", "");
		}
		this.SetError = function() {
			/// <summary>Set error on node</summary>
			if (!_node.className) { return; }
			if (_node.className.indexOf("error") == -1) {
				_node.className += " error";
			}
		}
		this.HasValue = function() {
			/// <summary>Does node have any value</summary>
			/// <returns>Boolean</returns>
			if (!_node) { return; }
			return (_node.value.length > 0 && _node.value != _this.IgnoreValue())
		}
		this.ID = function() {
			/// <summary>HTML ID of the node</summary>
			return id;
		}
		this.Equals = function(field) {
			/// <summary>Is field identical (same ID) as given field</summary>
			/// <returns>Boolean</returns>
			return (field.ID() == id);
		}
		this.Match = function(text) {
			/// <summary>Does field ID contain given text</summary>
			/// <returns>Boolean</returns>
			return (id.indexOf(text) != -1);
		}
		this.MatchRegEx = function(re) {
			/// <summary>Does field ID match the regular expression</summary>
			/// <returns>Boolean</returns>
			return re.test(id);
		}
		this.Focus = function() {
			/// <summary>Set focus on this field</summary>
			if (_node.type == "text") { _node.focus(); }
		}
		this.GarbageCollect = function() {
			/// <summary>Release memory</summary>
			_this = _validation = _node = null;
		}
	}

	this.IsSelect = function(node) {
		/// <summary>Ensure some selection has been made on the node</summary>
		/// <remarks>assumes that layout options, like lines, have a value less then 0</remarks>
		/// <returns>Boolean</returns>
		if (node.type == "select-one") {
			var re = /[\w,:\.]/;
			var val = node.options[node.selectedIndex].value;
			// true if option value > 0 or non-numeric
			return (re.test(val) && val != 0);
		} else if (node.type == "select-multiple") {
			for (var x = 0; x < node.options.length; x++) {
				if (node.options[x].selected) { return true; }
			}
		}
		return false;				// maybe default true if not select?
	}

	this.IsRadio = function(node) {
		/// <summary>Ensure one radio button was selected</summary>
		/// <returns>Boolean</returns>
		var list = [];

		if (typeof(node.length) != "undefined") {
			// see if given node is already a collection
			for (var x = 0; x < node.length; x++) { list[x] = node[x]; }
		} else if (/\w+1$/.test(node.id)) {
			// see if this is the first of several radio elements
			var x = 1;
			var baseName = node.id.replace("1", "");
			node = DOM.GetNode(baseName + x, true);
			while (node != null && node.type == "radio") {
				list[x - 1] = node;
				x++;
				node = DOM.GetNode(baseName + x, true);
			}
		}
		// cycle through each item in the radio collection
		for (var x = 0; x < list.length; x++) {
			if (list[x].checked) { return true; }
		}
		// if we made it here then no radio is checked
		return false;
	}

	this.IsCCExpire = function(node) {
		/// <summary>Allow only future expiration dates</summary>
		/// <returns>Boolean</returns>
		var date = _this.GetDate(node);
		if (date) {
			var today = new Date();
			if (date >= today) { return true; }
		}
		return false;
	}

	this.IsBirthDate = function(node) {
		/// <summary>Is date valid and in the past</summary>
		/// <returns>Boolean</returns>
		var date = _this.GetDate(node);
		if (date) {
			var today = new Date();
			var thisYear = CleanYear(today.getYear());
			var maxAge = 120;		// not many people older than that
			// can't have birthday in future or more than MaxAge
			if (date < today && thisYear > (thisYear - maxAge)) { return true; }
		}
		return false;
	}

	this.IsDate = function(node) {
		/// <summary>Has date been entered</summary>
		/// <returns>Boolean</returns>
		if (_this.GetDate(node)) { return true; }
		return false;
	}
	this.IsDateTime = function(node) {
		/// <summary>Has date and time been entered</summary>
		/// <returns>Boolean</returns>
		var re = /^((\d{1,2})[\/-\\](\d{1,2})[\/-\\](\d{2,4})\s(\d{1,2})\:(\d{1,2})\s[AP]M)$/;
		if (_this.GetDate(node, re)) { return true; }
		return false;
	}

	function CleanYear(year) {
		/// <summary>Make years four digits; assume century break on xx40</summary>
		/// <returns>Integer</returns>
		if (year.length == 2) { year = (year > 40 ? "19" : "20") + year; }
		return year;
	}
	
	this.GetDate = function(node, re) {
		/// <summary>Build valid date object</summary>
		/// <returns>Date</returns>
		if (re == null) { re = /^((\d{1,2})[\/-\\](\d{1,2})[\/-\\](\d{2,4}))$/; }
		var YEAR = 4; var MONTH = 2; var DAY = 3; var HOUR = 5; var MINUTE = 6;
		if (re.test(node.value)) {
			// format is right--now check each date value
			var matches = re.exec(node.value);
			var month = matches[MONTH];
			var day = matches[DAY];
			var year = CleanYear(matches[YEAR]);
			
			if (month <= 12 && month >= 1 && day <= 31 && day >= 1 && year <= 2100 && year >= 1850) {
				// -1 on month seems necessary for js date glitch
				if (matches.length > YEAR + 1) {
					var hour = matches[HOUR];
					var minute = matches[MINUTE];
					
					if (hour <= 12 && hour >= 1 && minute <= 59 && minute >= 0) {
						return new Date(year, month - 1, day, hour, minute);
					}
				} else {
					return new Date(year, month - 1, day);
				}
			}
		} 
		// invalid date format
		return false;
	}

	this.IsMoney = function(node) {
		/// <summary>Is value monetary</summary>
		/// <returns>Boolean</returns>
		var money = node.value.replace(/[^\d\.]/g,"");
		if (parseFloat(money) != money * 1) {
			return false;	// non-numeric values in node
		}
		/*var cents = money * 100;
		if (Math.abs(cents - Math.floor(cents)) > 0) {
			return false;	// fractional pennies not allowed
		}*/
		return true;
	}

	this.IsCCN = function(node) {
		/// <summary>Mod10 check</summary>
		/// <returns>Boolean</returns>
		var sCCN = ToNumeric(node.value)
		
		// temp validation to check out with test CCN
		var re = /^41{14,15}$/;
		if (re.test(sCCN)) { return true; }
		// end temp stuff ---------------------------
		
		var re = /^\d{15,16}$/;
		// fail if wrong length
		if (!(re.test(sCCN))) {	return false; }

		var lLength = sCCN.length;
  		var bEven = lLength & 1;
		var lCheckSum = 0;

		for (var i = 0; i < lLength; i++) {
			var thisNum = parseInt(sCCN.charAt(i));
			if (!((i & 1) ^ bEven)) {
				thisNum *= 2;
				if (thisNum > 9) { thisNum -= 9; }
			}
			lCheckSum += thisNum;
		}
		// fail if non-zero Mod10
		if (lCheckSum % 10 != 0) { return false; }
		return true;
	}

	this.IsPosting = function(node) {
		/// <summary>Allow only basic HTML</summary>
		/// <returns>Boolean</returns>
		if (node.value == "") { return false; }
		var re = /(<|&lt;)[^abiu\/]/gi;
		return (!(re.test(node.value) || (node.value.indexOf("<img") != -1)));
	}
	
	this.IsHtmlFile = function(node) {
		/// <summary>Allow only HTML files</summary>
		/// <returns>Boolean</returns>
		if (_this.IsFile(node)) {
			var re = /\.(htm|html)$/;
			return (re.test(node.value));
		}
		return false;
	}
	
	this.IsPlainText = function(node) {
		/// <summary>Allow only plain text</summary>
		/// <returns>Boolean</returns>
		// e.g. precludes <div> or &#216; or %20
		var re = /<|>|\&\#?\w{1,10}\;|\%\d{2,3}/g;
		return (!(re.test(node.value)));
	}

	this.IsImage = function(node) {
		/// <summary>Allow only image extensions</summary>
		/// <returns>Boolean</returns>
		var index = node.value.lastIndexOf(".")
		if (index == -1) { return false; }
		var extension = node.value.substring(index + 1, node.value.length);
		extension = extension.toLowerCase();
		return (extension == "gif" || extension == "jpg" || extension == "jpeg");
	}

	this.IsActiveURL = function(node) {
		/// <summary>Verify that URL is active</summary>
		/// <returns>Boolean</returns>
		if (_this.IsURL(node)) {
			var request = new ServerCall(null);
			return request.Exists(node.value);
		}
		return false;
	}

	// converts a node to all numbers
	function ToNumeric(node) { return node.replace(/\D/g, ""); }

	// basic regular expression pattern checks
	function TestField(node, re) {		return re.test(node.value); }
	this.IsPassword = function(node) {	return TestField(node, /^.{6,}$/); }
	this.IsCVV = function(node) {		return TestField(node, /\d{3,4}/); }
	this.IsEmail = function(node) {		return TestField(node, /^([\w\._\-]+@[\w\-]+\.[\w\-]+\.*[\w\-]*.*[\w\-]*)$/); }
	this.IsZip = function(node) {		return TestField(node, /^(\d{5})$/); }
	this.IsZip4 = function(node) {		return TestField(node, /^(\d{4})$/); }
	this.IsZipCode = function(node) {	return TestField(node, /^\d{5}\-?\d{0,4}$/); }
	this.IsNumeric = function(node) {	return TestField(node, /^(\d+)$/); }
	//http://www.sundancemediagroup.com/articles/ragged_text_in_Sony_Vegas.htm
	this.IsURL = function(node) {		return TestField(node, /\.\w{2,3}/); }
	this.IsName = function(node) {		return TestField(node, /[a-zA-Z'_]{2,}/); }
	this.IsFile = function(node) {		return true; } // TestField(node, /^(\w\:|\\)\\[^\/\:\*\?\"\<\>\|]+\.\w{3,8}$/); }

	this.IsPhone = function(node) {
		/// <summary>Is value a valid phone number</summary>
		/// <returns>Boolean</returns>
		var phone = parseInt(ToNumeric(node.value));
		return (phone < 99999999999 && phone > 10000000);
	}
	this.IsSSN = function(node) { var ssn = ToNumeric(node.value); return (ssn.length >= 7 && ssn.length <= 9); }
	this.IsString = function(node) { return (node.value != ""); }
	this.IsISBN = function(node) {
		var isbn = ToNumeric(node.value);
		return (isbn.length == 10 || isbn.length == 13);
	}
	this.IsUPC = function(node) {
		/// <returns>Boolean</returns>
		//http://www.schworak.com/upc/index.asp?action=help
		var isbn = ToNumeric(node.value);
		return (isbn.length == 8 || isbn.length == 12);
	}
	this.IsNonZero = function(node) {
		/// <returns>Boolean</returns>
		if (_this.IsNumeric(node) && node.value > 0) { return true; } 
		return false;
	}

	function GarbageCollect() {
		/// <summary>Release memory</summary>
		_errors = null;
		if (_this.Fields) {
			for (var x = 0; x < _this.Fields.length; x++) { _this.Fields[x].GarbageCollect(); }
		}
		_this.Fields = null;		
		_this.Functions = null;
		_this = null;
	}
}