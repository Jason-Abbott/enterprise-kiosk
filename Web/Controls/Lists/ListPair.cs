using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;
using web = System.Web.UI.WebControls;

namespace Idaho.Web.Controls {
	/// <summary>
	/// A pair of select lists that can have their items transferred
	/// back and forth
	/// </summary>
	public class ListPair : SelectList, IPostBackDataHandler {

		private SortedDictionary<string, string> _source;
		private SortedDictionary<string, string> _target;
		private SortedDictionary<string, string> _inherited;
		private Button _add;
		private Button _remove;
		private int _buttonWidth = 120;				// pixels
		private string _targetID = string.Empty;
		private string _sourceID = string.Empty;
		private string _inheritedID = string.Empty;
		private const string _targetName = "{0}";
		private const string _sourceName = "{0}_source";
		private const string _inheritedName = "{0}_inherited";
		private const string _targetStyle = "{0}Target";
		private const string _sourceStyle = "{0}Source";
		private int _inheritedRows = 4;
		private string _inheritedTitle = string.Empty;
		private const string _styleClass = "listPair";
		private string _originalID = string.Empty;
		private string[] _selected = null;

		#region ISelect

		public new string[] Selected {
			get {
				/*
				if (_target == null) { return null; }
				List<string> values = new List<string>();
				foreach (string key in _target.Keys) { values.Add(_target[key]); }
				return values.ToArray();
				*/
				return _selected;
			}
			set {
				if (_source == null) { return; }
				if (_target == null) { _target = new SortedDictionary<string, string>(); }
				foreach (string s in value) {
					if (_source.Contains(s)) {
						_target.Add(s, _target[s]);
						_source.Remove(s);
					}
				}
			}
		}
		public string TopSelection {
			get { return (_target == null) ? string.Empty : _target.Values[0]; }
		}
		
		#endregion
	
		#region IPostBackDataHandler (from ISelect)

		public override bool LoadPostData(string key, NameValueCollection posted) {
			if (!string.IsNullOrEmpty(posted[this.UniqueID])) {
				_selected = posted[this.UniqueID].Split(',');
			}
			return false;
		}

		#endregion

		#region Properties

		public new IListable DataSource {
			set { if (value != null) { _source = value.KeysAndValues; } }
		}
		public IListable DataTarget {
			set { if (value != null) { _target = value.KeysAndValues; } }
		}
		public SortedDictionary<string, string> Source {
			get { return _source; }	set { _source = value; }
		}
		public SortedDictionary<string, string> Target {
			get { return _target; } set { _target = value; }
		}
		/// <summary>
		/// List of values added by an inheritance relationship
		/// </summary>
		/// <remarks>
		/// These are values that are in the same category as the source and
		/// target lists but are excluded from add and removel. Designed for
		/// use with permissions scenarios.
		/// </remarks>
		public SortedDictionary<string, string> Inherited {
			get { return _inherited; } set { _inherited = value; }
		}
		public string InheritedTitle { set { _inheritedTitle = value; } }
		public int ButtonWidth { set { _buttonWidth = value; } }

		#endregion

		#region Events

		protected override void OnInit(EventArgs e) {
			_originalID = this.ID;
			_targetID = string.Format(_targetName, _originalID);
			_sourceID = string.Format(_sourceName, _originalID);
			_inheritedID = string.Format(_inheritedName, _originalID);

			_add = new Button();
			_remove = new Button();
			_add.ID = string.Format("btnAdd_{0}", this.ID);
			_add.Resx = "ListAdd";
			_add.OnClick = Resource.SayFormat("Script_ListTransfer", _sourceID, _targetID);
			_remove.ID = string.Format("btnRemove_{0}", this.ID);
			_remove.Resx = "ListRemove";
			_remove.OnClick = Resource.SayFormat("Script_ListTransfer", _targetID, _sourceID);
			_add.Width = _remove.Width = _buttonWidth;

			_add.OnInit();
			_remove.OnInit();

			this.Page.OnSubmitScript = Resource.SayFormat("Script_SelectList", _targetID);
			base.ShowLabel = !string.IsNullOrEmpty(base.Resx);
			base.HeadItem = HeadItems.DoNotDisplay;
			base.OnInit(e);
		}
		protected override void OnLoad(EventArgs e) {
			_add.OnLoad();
			_remove.OnLoad();
			// remove all target items from source
			if (_source != null) { _source.Remove(_target); }
			base.OnLoad(e);
		}

		#endregion

		protected override void Render(HtmlTextWriter writer) {
			bool renderInherited = (_inherited != null && _inherited.Count > 0);

			_add.OnPreRender();
			_remove.OnPreRender();

			writer.Write("<div class=\"");
			writer.Write(_styleClass);
			writer.Write("\"");
			if (Style.Count > 0) {
				this.RenderStyle(writer);
				this.Style.Clear();
			}
			writer.Write(">");

			// source list
			writer.Write("<div class=\"");
			writer.Write(string.Format(_sourceStyle, _styleClass));
			writer.Write("\">");
			this.RenderList(_sourceID, _source, writer);
			writer.Write("</div>");

			// buttons
			writer.Write("<div style=\"width: ");
			writer.Write(_buttonWidth + 6);
			writer.Write("px\">");
			_add.RenderControl(writer);
			_remove.RenderControl(writer);
			writer.Write("</div>");

			// target list
			base.ShowLabel = false;
			if (renderInherited) { this.Rows -= (_inheritedRows + 1); }

			writer.Write("<div class=\"");
			writer.Write(string.Format(_targetStyle, _styleClass));
			writer.Write("\">");
			this.RenderList(_targetID, _target, writer);

			// inherited list
			if (renderInherited) {
				this.Rows = _inheritedRows;
				if (!string.IsNullOrEmpty(_inheritedTitle)) {
					writer.Write("<h3>");
					writer.Write(_inheritedTitle);
					writer.Write("</h3>");
				}
				this.RenderList(_inheritedID, _inherited, writer);
			}
			writer.Write("</div></div>");
			//this.RenderPostbackTrigger(writer);
		}

		private void RenderList(string id, SortedDictionary<string, string> list, HtmlTextWriter writer) {
			this.ID = id;
			this.ClearStyle();
			this.RenderBeginTag(writer);
			if (list != null && list.Count > 0) {
				foreach (string k in list.Keys) {
					this.RenderOption(list[k], k, writer);
				}
			}
			this.RenderEndTag(writer);
		}
	}
}
