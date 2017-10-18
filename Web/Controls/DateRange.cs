using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Idaho.Web.Controls {
	public class DateRange : InputControl {

		private Idaho.DateRange _range = Idaho.DateRange.Empty;
		private Unit _width = Unit.Empty;
		private string _between = "through";
		private int _maxLength = 0;
		private Idaho.DateRange.PreciseTo _precision = Idaho.DateRange.PreciseTo.Default;
		private DatePicker _start;
		private DatePicker _end;
		private bool _initialized = false;
		private bool _loaded = false;

		#region Properties

		public Unit Width {
			set {
				_width = value;
				if (_start != null && _end != null) {
					_start.Width = _end.Width = _width;
				}
			}
		}
		public Idaho.DateRange Range { set { _range = value; } get { return _range; } }

		/// <summary>
		/// The text displayed between input fields
		/// </summary>
		public string Between { set { _between = value; } }
		public int MaxLength { set { _maxLength = value; } }

		/// <summary>
		/// The level of precision
		/// </summary>
		/// <remarks>
		/// This controls whether time is displayed and how equality is tested.
		/// </remarks>
		public Idaho.DateRange.PreciseTo Precision { set { _range.Precision = _precision = value; } }

		#endregion

		#region IPostBackDataHandler (from InputControl)

		public override bool LoadPostData(string key, NameValueCollection posted) {
			string start = posted[string.Format("{0}_start", this.ID)];
			string end = posted[string.Format("{0}_end", this.ID)];
			_range = Idaho.DateRange.Parse(start, end, _precision);
			return false;
		}

		#endregion

		protected override void OnInit(EventArgs e) {
			if (this.Visible) {
				this.Page.RegisterRequiresPostBack(this);
				this.SetupFields();
				this.ShowLabel = true;
				_start.OnInit();
				_end.OnInit();
				base.OnInit(e);
				_initialized = true;
			}
		}

		protected override void OnLoad(EventArgs e) {
			if (this.Visible) {
				if (!_initialized) { this.OnInit(e); }
				if (!_range.IsEmpty) {
					_start.Value = _range.Start;
					_end.Value = _range.End;
				}
				// for precision other than day, set format to show time
				if (_precision != Idaho.DateRange.PreciseTo.Day) {
					_start.Format = _end.Format = DatePicker.DateFormat.DateAndTime;
					_start.ShowTime = _end.ShowTime = true;
				}
				_start.Attributes.Add("title", Resource.Say("Tip_{0}Start", base.ResourceKey));
				_end.Attributes.Add("title", Resource.Say("Tip_{0}End", base.ResourceKey));
				_start.Required = _end.Required = true;
				_start.ValidationAlert = _end.ValidationAlert = Resource.Say("Validate_DateTime");

				base.OnLoad(e);
				_start.OnLoad();
				_end.OnLoad();
				_loaded = true;
			}
		}

		protected override void OnPreRender(EventArgs e) {
			if (this.Visible) {
				if (!_loaded) { this.OnLoad(e); }
				base.OnPreRender(e);
				_start.ShowLabel = _end.ShowLabel = false;
				_start.OnPreRender();
				_end.OnPreRender();
			}
		}

		protected override void Render(HtmlTextWriter writer) {
			this.RenderLabel(writer, _start.ID);
			_start.RenderControl(writer);
			writer.Write("<span class=\"between\">");
			writer.Write(_between);
			writer.Write("</span>");
			_end.RenderControl(writer);
			this.RenderNote(writer);
			this.RenderPostbackTrigger(writer);
		}

		private void SetupFields() {
			_start = new DatePicker();
			_end = new DatePicker();

			_start.ID = string.Format("{0}_start", this.ID);
			_end.ID = string.Format("{0}_end", this.ID);
			_start.Page = _end.Page = this.Page;
			if (_maxLength != 0) { _start.MaxLength = _end.MaxLength = _maxLength; }
			if (!_width.IsEmpty) { _start.Width = _end.Width = _width; }
		}
	}
}
