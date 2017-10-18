using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class SelectList : InputControl, ISelect {

		private string[] _selected;
		private bool _multiple = true;
		private SelectList.HeadItems _headItem = HeadItems.Choose;
		private int _rows = 1;
		private bool _showLink = false;
		private SortedDictionary<string, string> _keyValues = null;
		private List<string> _exclusions = null;
		private bool _selectHead = false;
		private string _emptyValue = "0";
		private string _headText = string.Empty;
		private bool _numberItems = false;
		private bool _hideIfEmpty = true;
		private KeyValuePair<Regex, string> _styleCondition = null;
		private int _number = 1;
		private KeyValuePair<string, string> _head = null;
		private StyleClassDelegate _getStyleClass = null;

		public delegate string StyleClassDelegate(string value, string name);

		public enum HeadItems {
			Choose,
			New,
			DoNotDisplay,
			/// <summary>displays head item with "None"</summary>
			NoneChosen,
			Any,
			CustomText
		}

		#region Properties

		/// <summary>
		/// Specify a delegate to apply CSS styling to list items
		/// </summary>
		public StyleClassDelegate StyleClassSetter {
			set { _getStyleClass = value; }
		}

		/// <summary>
		/// Specify a style class as the second part of the value and
		/// a regular expression pattern in the first part
		/// </summary>
		public KeyValuePair<Regex, string> StyleCondition { set { _styleCondition = value; } }

		/// <summary>
		/// Do not render if there are no data items
		/// </summary>
		public bool HideIfEmpty {
			protected get { return _hideIfEmpty; }
			set { _hideIfEmpty = value; }
		}

		/// <summary>
		/// Prepend items with numbering
		/// </summary>
		public bool NumberItems { set { _numberItems= value; } }

		/// <summary>
		/// Values to exclude from display
		/// </summary>
		public List<string> Exclude { set { _exclusions = value; } }

		/// <summary>
		/// Text to display at top of list in lieu of enumerated head item
		/// </summary>
		public string HeadText {
			set {
				_headText = value;
				if (!string.IsNullOrEmpty(_headText)) {
					_headItem = HeadItems.CustomText;
				}
			}
		}

		/// <summary>
		/// Path to append to for redirection when selection changes
		/// </summary>
		/// <example>edit.aspx?id=</example>
		public string OnChangePage {
			set {
				this.OnChange = string.Format(
					"Page.Redirect('{0}' + DOM.GetValue(this))", value);
			}
		}

		public IListable DataSource {
			set { if (value != null) { _keyValues = value.KeysAndValues; } }
		}

		/// <summary>
		/// Make head item selected, none others
		/// </summary>
		public bool SelectHead { set { _selectHead = value; _selected = null; } }

		public SelectList.HeadItems HeadItem {
			get { return _headItem; }
			set { _headItem = value; }
		}
		/// <summary>
		/// Allow multiple items to be selected
		/// </summary>
		public bool Multiple { get { return _multiple; } set { _multiple = value; } }
		public bool Posted {
			get { return ((this.Selections != null) && this.Selections.Length > 0); }
		}
		/// <summary>
		/// Show HTML link to select all items
		/// </summary>
		public bool ShowLink { get { return _showLink; } set { _showLink = value; } }
		public int Rows { get { return _rows; } set { _rows = value; } }

		/// <summary>
		/// All selected items
		/// </summary>
		public virtual string[] Selections {
			get { return _selected; } set { _selected = value; }
		}
		/// <summary>
		/// Selected item (first one if multiple allowed)
		/// </summary>
		public virtual string Selection {
			get { return (this.Posted) ? _selected[0] : string.Empty; }
			set { _selected = new string[] { value }; }
		}

		public SortedDictionary<string, string> KeysAndValues {
			set {
				_keyValues = value;
				if (_keyValues != null && _keyValues.Count > 0) {
					Regex guid = new Regex(Pattern.Guid);
					if (guid.IsMatch(_keyValues.Keys[0])) { _emptyValue = Guid.Empty.ToString(); }
				}
			}
		}

		#endregion

		#region IPostBackDataHandler

		public override bool LoadPostData(string key, NameValueCollection posted) {
			if (!string.IsNullOrEmpty(posted[key])) { _selected = posted[key].Split(','); }
			return false;
		}

		#endregion

		protected override void OnLoad(EventArgs e) {
			if (_getStyleClass == null) {
				if (_styleCondition == null || _styleCondition.Key == null) {
					_getStyleClass = EmptyStyleClass;
				} else {
					_getStyleClass = RegexStyleClass;
				}
			}
			base.OnLoad(e);
		}

		private string RegexStyleClass(string value, string name) {
			return _styleCondition.Key.IsMatch(name)
				? _styleCondition.Value : string.Empty;
		}

		private string EmptyStyleClass(string value, string name) {
			return string.Empty;
		}

		/// <summary>
		/// Indicate if given value has been selected
		/// </summary>
		protected virtual bool IsSelected(string value) {
			if (_selected == null) { return false; }
			else { return (Array.IndexOf<string>(_selected, value) >= 0); }
		}

		/// <summary>
		/// Add a name and value to the list
		/// </summary>
		public void Add(string value, string name) {
			if (_keyValues == null) { _keyValues = new SortedDictionary<string, string>(); }
			_keyValues.Add(value, name);
		}
		public void Add(string name) { this.Add(name, name); }

		#region Render

		/// <summary>
		/// Render markup if given name/value collection
		/// </summary>
		protected override void Render(HtmlTextWriter writer) {
			bool hasData = _keyValues != null && _keyValues.Count > 0;
			if (hasData || !_hideIfEmpty) {
				this.RenderBeginTag(writer);
				if (hasData) {
					foreach (string k in _keyValues.Keys) {
						if (_exclusions == null || !_exclusions.Contains(k)) {
							this.RenderOption(_keyValues[k], k, writer);
						}
					}
				}
				this.RenderEndTag(writer);
			}
		}

		/// <summary>
		/// Write opening select tag
		/// </summary>
		protected override void RenderBeginTag(HtmlTextWriter writer) {
			this.RenderLabel(writer);
			writer.Write("<select");
			if (_rows > 1) {
				writer.Write(" size=\"{0}\"", _rows);
				if (_multiple) { writer.Write(" multiple=\"multiple\""); }
			}
			base.RenderAttributes(writer);
			writer.Write(">");

			if (_rows == 1 && _headItem != HeadItems.DoNotDisplay) {
				_head = new KeyValuePair<string, string>();
				_head.Value = _emptyValue;

				switch (_headItem) {
					case HeadItems.Choose:
						_head.Key = Resource.Say("Select_Choose");
						break;
					case HeadItems.New:
						_head.Key = Resource.Say("Select_New");
						_head.Value = "new";
						break;
					case HeadItems.NoneChosen:
						_head.Key = Resource.Say("Select_None");
						break;
					case HeadItems.Any:
						_head.Key = Resource.Say("Select_Any");
						break;
					case HeadItems.CustomText:
						_head.Key = _headText;
						break;
					default: return;
				}
				writer.Write("<option value=\"{0}\" class=\"choose\">{1}</option>",
					_head.Value, _head.Key);
			}
		}

		/// <summary>
		/// Write selected attribute
		/// </summary>
		protected void RenderSelected(string id, HtmlTextWriter writer) {
			if (this.IsSelected(id)) { writer.Write(" selected=\"selected\")"); }
		}
		protected void RenderSelected(Guid id, HtmlTextWriter writer) {
			this.RenderSelected(id.ToString(), writer);
		}

		/// <summary>
		/// Write closing select tag
		/// </summary>
		protected void RenderEndTag(HtmlTextWriter writer) {
			writer.Write("</select>");
			if (_rows > 1) {
				if (_showLink) {
					writer.Write("<a class=\"listAction\" href=\"javascript:DOM.ClearSelection('");
					writer.Write(this.ID.Trim());
					writer.Write("')\">");
					writer.Write(Resource.Say("Action_Clear"));
					writer.Write("</a>");
				}
				if (this.ShowNote != NoteDisplay.None &&
					string.IsNullOrEmpty(this.Note) &&
					string.IsNullOrEmpty(this.ResourceKey)) {

					this.ResourceKey = "Note_MultiSelectNote";
					this.ShowNote = NoteDisplay.NewLine;
				}
			}
			this.RenderNote(writer);
		}

		/// <summary>
		/// Render a single select option and check for option grouping
		/// </summary>
		protected void RenderOption(string name, string value, string styleClass, HtmlTextWriter writer) {
			writer.Write("<option value=\"");
			writer.Write(value);
			writer.Write("\"");
			if (!string.IsNullOrEmpty(styleClass)) {
				writer.Write(" class=\"");
				writer.Write(styleClass);
				writer.Write("\"");
			}
			this.RenderSelected(value, writer);
			writer.Write(">");
			if (_numberItems) {
				writer.Write("{0}. ", _number);
				_number++;
			}
			writer.Write(name);
			writer.Write("</option>");
		}
		protected void RenderOption(string name, string value, HtmlTextWriter writer) {
			this.RenderOption(name, value, _getStyleClass(value, name), writer);
		}
		protected void RenderOption(string name, int id, string styleClass, HtmlTextWriter writer) {
			this.RenderOption(name, id.ToString(), styleClass, writer);
		}
		protected void RenderOption(string name, int id, HtmlTextWriter writer) {
			this.RenderOption(name, id, string.Empty, writer);
		}
		protected void RenderOption(string name, Guid id, HtmlTextWriter writer) {
			this.RenderOption(name, id.ToString(), writer);
		}
		protected void RenderOption(string name, HtmlTextWriter writer) {
			this.RenderOption(name, name, writer);
		}

		protected void RenderGroupBegin(string name, HtmlTextWriter writer) {
			writer.Write("<optgroup label=\"{0}\">", name);
		}
		protected void RenderGroupEnd(HtmlTextWriter writer) {
			writer.Write("</optgroup>");
		}

		#endregion

		/// <summary>
		/// Button object that will link to page for selected item
		/// </summary>
		/// <param name="b">Button object</param>
		/// <param name="urlFormat">String.Format for URL with inserted item value</param>
		public void ViewWithButton(Button b, string urlFormat) {
			if (b != null && !string.IsNullOrEmpty(urlFormat)) {
				string url = string.Format(urlFormat, "' + DOM.GetValue('" + this.ID + "') + '");
				b.OnClick = Idaho.Resource.SayFormat("Script_Redirect", url, EcmaScript.Null);
			}
		}
	}
}