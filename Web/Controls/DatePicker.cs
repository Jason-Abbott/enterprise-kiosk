using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Text field with button to pop calendar
	/// </summary>
	/// http://www.dynarch.com/projects/calendar/
	public class DatePicker : InputControl {

		private DateTime _value = DateTime.MinValue;
		private Unit _defaultWidth = new Unit(7.0, UnitType.Em);
		private Unit _width = Unit.Empty;
		private string _format = DateFormat.DateOnly;
		private int _defaultMaxLength = 10;
		private int _maxLength = 0;
		private bool _showTime = false;
		private Image _button = null;

		#region Properties

		public DateTime Value { get { return _value; } set { _value = value; } }
		public string Format { set { _format = value; } }
		public bool HasValue { get { return _value != DateTime.MinValue; } }
		public int MaxLength { set { _maxLength = value; } }
		public Unit Width { set { _width = value; } }
		public bool ShowTime { set { _showTime = value; } }

		#endregion

		internal class DateFormat {
			public const string DateOnly = "%m/%d/%Y";
			public const string DateAndTime = "%m/%d/%Y %I:%M %p";
		}

		#region IPostBackDataHandler (from InputControl)

		public override bool LoadPostData(string key, NameValueCollection posted) {
			string value = posted[key];
			if (!string.IsNullOrEmpty(value)) { _value = DateTime.Parse(value); }
			return false;
		}

		#endregion

		#region Events

		/// <summary>
		/// Include the script and styles used by this control
		/// </summary>
		protected override void OnInit(EventArgs e) {
			_button = new Image(this.Page);
			_button.UseCache = true;
			_button.Resource = "images/calendar/calendar_icon.png";
			_button.Transparency = true;
			_button.CssClass = "calIcon";
			this.Page.StyleSheet.AddResource("calendar");
			this.Page.ScriptFile.AddResource("calendar/calendar");
			this.Page.ScriptFile.AddResource("calendar/calendar-en");
			this.Page.ScriptFile.AddResource("calendar/calendar-setup");
			base.OnInit(e);
			_button.OnInit();
		}

		/// <summary>
		/// Generate the script needed to setup calendar events
		/// </summary>
		/// <remarks>
		/// Script parameters are described in calendar-setup.js
		/// </remarks>
		protected override void OnLoad(EventArgs e) {
			_button.ID = string.Format("{0}_button", this.ID);
			StringBuilder script = new StringBuilder();
			script.Append("Calendar.setup( {");
			script.AppendFormat("inputField: \"{0}\", ", this.ID);
			script.AppendFormat("ifFormat: \"{0}\", ", _format);
			script.AppendFormat("button: \"{0}\"", _button.ID);
			if (_showTime) {
				script.Append(", showsTime: true, timeFormat: \"12\"");
				_defaultMaxLength = 20;
				_defaultWidth = new Unit(10.0, UnitType.Em);
			}
			script.Append(" } );");
			this.Page.ScriptBlock = script.ToString();
			this.ValidationType = (_showTime) ? Validation.Types.DateTime : Validation.Types.Date;
			this.RegisterValidation();
			this.ShowLabel = true;
			base.OnLoad(e);
		}

		#endregion

		/// <summary>
		/// Write date picker with button to pop calendar
		/// </summary>
		protected override void Render(HtmlTextWriter writer) {
			if (_width.IsEmpty) { _width = _defaultWidth; }
			if (_maxLength == 0) { _maxLength = _defaultMaxLength; }
			string tip = Resource.Say("Tip_CalendarIcon");

			_button.AlternateText = tip;
			_button.Attributes.Add("title", tip);

			this.RenderLabel(writer);
			writer.Write(Environment.NewLine);
			writer.Write("<input style=\"width: ");
			writer.Write(_width.ToString());
			writer.Write("; text-align: right;\" type=\"text\"");
			this.RenderAttributes(writer);
			writer.Write(" maxlength=\"");
			writer.Write(_maxLength);
			writer.Write("\"");

			if (_value != null && _value != DateTime.MinValue && _value != DateTime.MaxValue) {
				writer.Write(" value=\"");
				writer.Write(HttpUtility.HtmlEncode(_value.ToShortDateString()));
				if (_showTime) {
					writer.Write(" ");
					writer.Write(HttpUtility.HtmlEncode(_value.ToShortTimeString()));
				}
				writer.Write("\"");
			}
			writer.Write(" />");
			_button.RenderControl(writer);
		}
	}
}