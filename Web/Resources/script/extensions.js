String.prototype.Format = function() {
	/// <summary>Format a string in the style of .Net</summary>
	/// <remarks>http://frogsbrain.wordpress.com/2007/04/28/javascript-stringformat-method/</remarks>
	var pattern = /\{\d+\}/g;
	var args = arguments;
	return this.replace(pattern, function(capture) {
		return args[capture.match(/\d+/)];
	});
}

String.prototype.Contains = function(s) {
	/// <summary>Does string contain given value</summary>
	var re = new RegExp(s, "gi");
	return re.test(this);
}

Number.prototype.PadLeft = function(count) {
	/// <summary>Add leading zeros to a number</summary>
	/// <param name="count">Total length of the resulting string</param>
	if (isNaN(this) || count < 1) { return null; }

	var length = this.toString().length;
	var output = "";

	for (var i = 0; i < count - length; i++) { output += "0"; }
	output += this;

	return output;
}

Date.prototype.ChangeTime = function(time) {
	/// <summary>Update the time in a date object to a new time</summary>
	var d = this;
	var t = new Date();
	var i = -1;

	if (time) {
		if (typeof (time) == "string") {
			var military = true;
			var parts = [];

			if (time.Contains("T")) { time = time.substr(time.indexOf("T") + 1); }
			if (/[AP]M/.test(time)) {
				time = time.replace(/\s*[AP]M\s*/gi, "");
				military = false;
			}
			parts = time.split(":");

			for (var x = 1; x <= 3; x++) {
				if (parts.length < x) { parts[x - 1] = 0; }
			}
			t.setHours(parts[0]);
			t.setMinutes(parts[1]);
			t.setSeconds(parts[2]);
		} else if (typeof (time) == "object") {
			t = time;
		} else {
			return d;
		}
		d.setHours(t.getHours(), t.getMinutes(), t.getSeconds());
	}
	return d;
}

Date.prototype.Sortable = function() {
	/// <summary>Convert date to sortable .Net format</summary>
	/// <example>2009-03-16T12:00:00</example>
	var s = "";
	s += this.getFullYear() + "-";
	s += (this.getMonth() + 1).PadLeft(2) + "-"
	s += this.getDate().PadLeft(2) + "T";
	s += this.getHours().PadLeft(2) + ":";
	s += this.getMinutes().PadLeft(2) + ":";
	s += this.getSeconds().PadLeft(2);
	
	return s;
}

Date.prototype.ToLongFormat = function() {
	var s = "";
	var h = this.getHours();
	var m = "AM";

	if (h > 12) { h = h - 12; m = "PM" }

	switch (this.getDay()) {
		case 0: s += "Sunday"; break;
		case 1: s += "Monday"; break;
		case 2: s += "Tuesday"; break;
		case 3: s += "Wednesday"; break;
		case 4: s += "Thurdsay"; break;
		case 5: s += "Friday"; break;
		case 6: s += "Saturday"; break;
	}
	s += ", ";

	switch (this.getMonth()) {
		case 0: s += "Jan"; break;
		case 1: s += "Feb"; break;
		case 2: s += "Mar"; break;
		case 3: s += "Apr"; break;
		case 4: s += "May"; break;
		case 5: s += "Jun"; break;
		case 6: s += "Jul"; break;
		case 7: s += "Aug"; break;
		case 8: s += "Sep"; break;
		case 9: s += "Oct"; break;
		case 10: s += "Nov"; break;
		case 11: s += "Dec"; break;
	}
	s += " " + this.getDate();
	s += ", " + h;
	s += ":" + this.getMinutes().PadLeft(2);
	s += " " + m;

	return s;
}