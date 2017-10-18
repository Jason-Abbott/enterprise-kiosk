using System;
using System.Collections.Specialized;
using System.Web.UI;
	
namespace Idaho.Web.Controls {
	public abstract class InputControl : HtmlControl, IInputControl {

		private NoteDisplay _showNote = NoteDisplay.None;
		private bool _showLabel = true;
		private bool _required = false;
		private bool _hasFocus = false;
		private bool _initialized = false;
		private bool _readOnly = false;
		private int _maxLength = 0;
		private bool _loaded = false;
		private string _label = string.Empty;
		private string _validationAlert = string.Empty;
		private Validation.Types _validationType = Validation.Types.None;
		private string _resx = string.Empty;
		private string _note = string.Empty;
		public enum NoteDisplay { None, Inline, NewLine }

		protected bool Initialized { get { return _initialized; } set { _initialized = value; } }
		protected bool Loaded { get { return _loaded; } set { _loaded = value; } }
		public int MaxLength { get { return _maxLength; } set { _maxLength = value; } }

		#region IInputControl Properties

		public bool ReadOnly { set { _readOnly = value; } protected get { return _readOnly; } }
		public string Resx { set { _resx = value; } protected get { return _resx; } }
		public string ResourceKey { set { this.Resx = value; } protected get { return this.Resx; } }

		/// <summary>
		/// Should this control have focus when the page loads
		/// </summary>
		public bool HasFocus { set { _hasFocus = value; } }

		public Validation.Types ValidationType {
			get { return _validationType; }
			set { _validationType = value; }
		}
		public string ValidationAlert {
			get { return _validationAlert; }
			set { _validationAlert = value; }
		}
		public string Label { get { return _label; } set { _label = value; } }
		public string Note { get { return _note; } set { _note = value; } }
		public bool ShowLabel { get { return _showLabel; } set { _showLabel = value; } }
		public bool Required { get { return _required; } set { _required = value; } }
		public NoteDisplay ShowNote { get { return _showNote; } set { _showNote = value; } }

		#endregion

		#region IPostBackDataHandler (from IInputControl)

		public abstract bool LoadPostData(string key, NameValueCollection posted);
		public void RaisePostDataChangedEvent() { }

		#endregion

		#region Rendering

		/// <summary>
		/// Render standard form labels and notes
		/// </summary>
		public void RenderLabel(HtmlTextWriter writer, string forID) {
			if (_showLabel) {
				this.GetLabel();
				writer.Write("<label");
				if (_required) { writer.Write(" class=\"required\""); }
				writer.Write(" for=\"");
				writer.Write(forID);
				writer.Write("\">");
				writer.Write(_label);
				writer.Write("</label>");
			}
		}
		public void RenderLabel(HtmlTextWriter writer) {
			this.RenderLabel(writer, this.ID);
		}

		/// <summary>
		/// Render note optionally displayed by form field
		/// </summary>
		public void RenderNote(HtmlTextWriter writer) {
			if (_showNote != NoteDisplay.None) {
				if (string.IsNullOrEmpty(_note) && !string.IsNullOrEmpty(_resx)) {
					_note = Resource.Say(string.Format("Note_{0}", _resx));
				}
				writer.Write(_showNote == NoteDisplay.Inline ? "<span" : "<div");
				writer.Write(" class=\"note\">");
				writer.Write(_note);
				writer.Write(_showNote == NoteDisplay.Inline ? "</span>" : "</div>");
			}
		}

		/// <summary>
		/// Load post back data is triggered by field having unique ID
		/// </summary>
		public void RenderPostbackTrigger(HtmlTextWriter writer) {
			writer.Write("<input type=\"hidden\" id=\"");
			writer.Write(this.ClientID);
			writer.Write("\" name=\"");
			writer.Write(this.UniqueID);
			writer.Write("\" />");
		}

		/// <summary>
		/// Base attributes does not include name which is necessary for postback handler
		/// </summary>
		protected override void RenderAttributes(HtmlTextWriter writer) {
			if (!string.IsNullOrEmpty(_resx)) {
				string tip = Resource.Say(string.Format("Tip_{0}", _resx));
				if (!string.IsNullOrEmpty(tip)) { base.Attributes.Add("title", tip); }
			}
			/*
			if (base.Attributes.
			writer.Write(" name=\"");
			writer.Write(this.UniqueID);
			writer.Write("\"");
			*/
			//MyBase.RenderCss(writer)
			base.RenderAttributes(writer);
		}

		#endregion

		#region Validation

		/// <summary>
		/// Manually add validation control to form
		/// </summary>
		protected internal void RegisterValidation() {
			if (_validationType != Validation.Types.None && this.Visible) {
				this.GetAlert();
				Validation validation = new Validation();
				validation.Type = _validationType;
				validation.Message = _validationAlert;
				validation.Required = _required;
				validation.Control = this;
				validation.For = this.ID;
				validation.Register(this.Page);
			}
		}
		internal void RegisterValidation(Validation.Types type) {
			_validationType = type;
			this.RegisterValidation();
		}
		internal void RegisterValidation(Validation.Types type, string alert) {
			_validationType = type;
			_validationAlert = alert;
			this.RegisterValidation();
		}

		#endregion

		/// <summary>
		/// Generate script to focus on this field if indicated
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			if (_hasFocus && this.Visible) {
				this.Page.ScriptBlock = Resource.SayFormat("Script_Focus", this.ID);
			}
			base.OnLoad(e);
		}

		/// <summary>
		/// Get label value for this control
		/// </summary>
		private void GetLabel() {
			if (string.IsNullOrEmpty(_label) && !string.IsNullOrEmpty(_resx)) {
				_label = Resource.Say(string.Format("Label_{0}", _resx));
				if (string.IsNullOrEmpty(_label)) {
					throw new System.Exception(Resource.Say("Error_NoLabelResource", _resx));
				}
			}
		}

		/// <summary>
		/// Get validation string for this control
		/// </summary>
		protected void GetAlert() {
			if (string.IsNullOrEmpty(_validationAlert)) {
				if (!string.IsNullOrEmpty(_resx)) {
					_validationAlert = Resource.Say(string.Format("Validate_{0}", _resx));
					if (!string.IsNullOrEmpty(_validationAlert)) { return; }
				}
				this.GetLabel();
				_validationAlert = _label;
			}
		}
	}
}