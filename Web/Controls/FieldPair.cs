using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Idaho.Web.Controls {
	public class FieldPair : InputControl {

		private Field.Types _type = Field.Types.Text;
		private string _value1;
		private string _value2;
		private string _between;
		private Unit _width = new Unit(7.0, UnitType.Em);
		private const string _idFormat = "{0}_field{1}";

		public Field.Types Type { set { _type = value; } }
		public string Value1 { get { return _value1; } set { _value1 = value; } }
		public string Value2 { get { return _value2; } set { _value2 = value; } }
		/// <summary>
		/// Text to display between the fields
		/// </summary>
		public string Between { set { _between = value; } }
		public System.Web.UI.WebControls.Unit Width { set { _width = value; } }

		private Field _field1;
		private Field _field2;

		#region IPostBackDataHandler (from InputControl)

		public override bool LoadPostData(string key, NameValueCollection posted) {
			_value1 = posted[string.Format(_idFormat, this.ID, 1)];
			_value2 = posted[string.Format(_idFormat, this.ID, 2)];
			return false;
		}

		#endregion

		protected override void OnInit(EventArgs e) {
			if (this.Visible) {
				this.Page.RegisterRequiresPostBack(this);
				this.SetupFields();
				this.ShowLabel = true;
				_field1.OnInit();
				_field2.OnInit();
				base.OnInit(e);
			}
		}
		protected override void OnLoad(EventArgs e) {
			if (this.Visible) {
				if (!this.Initialized) { this.OnInit(e); }
				base.OnLoad(e);
				_field1.OnLoad();
				_field2.OnLoad();
			}
		}

		/// <summary>
		/// Render control for field pair
		/// </summary>
		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			if (!this.Loaded) { this.OnLoad(new EventArgs()); }
			this.RenderLabel(writer, _field1.ID);
			_field1.Value = _value1;
			_field1.RenderControl(writer);
			writer.Write(string.Format(" {0} ", _between));
			_field2.Value = _value2;
			_field2.RenderControl(writer);
			this.RenderPostbackTrigger(writer);
			this.RenderNote(writer);
		}

		/// <summary>
		/// Create field controls
		/// </summary>
		private void SetupFields() {
			_field1 = new Field(_type);
			_field2 = new Field(_type);

			if (this.ValidationType == Validation.Types.None) {
				// infer validation if none specified
				this.ValidationType = (_type == Field.Types.Password) ?
					Validation.Types.Password : Validation.Types.Name;
			}
			_field1.Page = _field2.Page = this.Page;
			_field1.ShowLabel = _field2.ShowLabel = false;
			_field1.Style.Add("width", _width.ToString());
			_field2.Style.Add("width", _width.ToString());
			_field1.MaxLength = _field2.MaxLength = this.MaxLength;
			_field1.Required = _field2.Required = this.Required;
			_field1.ValidationType = _field2.ValidationType = this.ValidationType;

			_field1.ID = string.Format(_idFormat, this.ID, 1);
			_field2.ID = string.Format(_idFormat, this.ID, 2);
		}
	}
}