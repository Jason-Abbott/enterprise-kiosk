using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class Field : InputControl {

		private Types _type;
		private string _value = string.Empty;
		private bool _checked;
		private Int16 _rows = 0;

		public enum Types { Text, Checkbox, Password, TextArea, Hidden }

		public Int16 Rows { set { _rows = value; } }
		public bool Checked { get { return _checked; } set { _checked = value; } }
		public string Value { get { return _value; } set { _value = value; } }
		public Types Type { set { _type = value; } }

		#region IPostBackDataHandler (from InputControl)

		public override bool LoadPostData(string key, NameValueCollection posted) {
			switch (_type) {
				case Types.Text:
				case Types.Password:
				case Types.TextArea:
				case Types.Hidden:
					_value = posted[key];
					break;
				case Types.Checkbox:
					_checked = (posted[key] == "on");
					break;
			}
			return false;
		}

		#endregion

		#region Constructors

		public Field() { }
		public Field(Types type) { _type = type; }
		public Field(string id, Types type) : this(type) { this.ID = id; }

		#endregion

		protected override void OnInit(EventArgs e) {
			if (this.Required && this.ValidationType == Validation.Types.None) {
				throw new System.Exception("No validation specified for required field");
			}
			if (this.Disabled) { this.CssClass = "disabled"; }
			if (this.ReadOnly) { this.CssClass = "readonly"; }
			base.ShowLabel = true;
			base.OnInit(e);
		}
		internal new void OnInit() { this.OnInit(new EventArgs()); }

		protected override void OnLoad(EventArgs e) {
			this.RegisterValidation();
			base.OnLoad(e);
		}

		/// <summary>
		/// Write labeled control with validation and notes
		/// </summary>
		protected override void Render(HtmlTextWriter writer) {
			switch (_type) {
				case Types.Text:
				case Types.Password:
					this.RenderLabel(writer);
					writer.Write("<input type=\"");
					writer.Write(_type.ToString());
					writer.Write("\"");
					this.RenderAttributes(writer);
					if (this.MaxLength != 0) { writer.Write(" maxlength=\"{0}\"", this.MaxLength); }
					this.RenderValue(writer);
					if (this.ReadOnly) { writer.Write(" readonly=\"readonly\""); }
					writer.Write(" />");
					this.RenderNote(writer);
					break;

				case Types.TextArea:
					this.RenderLabel(writer);
					writer.Write("<textarea ");
					this.RenderAttributes(writer);
					if (this.ReadOnly) { writer.Write(" readonly=\"readonly\""); }
					writer.Write(">");
					writer.Write(_value);
					writer.Write("</textarea>");
					break;

				case Types.Checkbox:
					if (this.ReadOnly) { this.OnClick = "this.checked = !this.checked;"; }
					this.CssClass = "cbx";
					writer.Write("<div class=\"checkbox\"><input type=\"checkbox\"");
					this.RenderAttributes(writer);
					if (_checked) { writer.Write(" checked=\"checked\""); }
					writer.Write(" />");
					this.RenderLabel(writer);
					writer.Write("</div>");
					this.RenderNote(writer);
					break;

				case Types.Hidden:
					writer.Write("<input type=\"hidden\"");
					this.RenderAttributes(writer);
					this.RenderValue(writer);
					writer.Write("/>");
					break;

				default:
					throw new System.Exception("Unsupported field type");
			}
		}

		private void RenderValue(HtmlTextWriter writer) {
			if (!string.IsNullOrEmpty(_value)) {
				writer.Write(" value=\"");
				writer.Write(HttpUtility.HtmlEncode(_value));
				writer.Write("\"");
			}
		}
	}
}