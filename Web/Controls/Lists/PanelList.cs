using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;

namespace Idaho.Web.Controls {
	/// <summary>
	/// A scrollable div of items
	/// </summary>
	/// <typeparam name="T">Type of listed entity ID</typeparam>
	public abstract class PanelList<T> : HtmlControl, IPostBackDataHandler {
		
		private bool _linkToSamePage = true;
		private bool _showInfoIcon = false;
		private string _infoIconTargetFieldID = "id";
		private string _infoIconTip = string.Empty;
		private bool _checkboxes = false;
		private string _checkboxName = string.Empty;
		private string _label = string.Empty;
		private List<T> _selected;
		protected bool? _selectAll = null;

		#region Properties

		public string CheckBoxName { set { _checkboxName = value; } }

		/// <summary>
		/// Were all items selected
		/// </summary>
		public virtual bool SelectAll {
			set { _selectAll = value; }
			get { return _selectAll == true; }
		}

		/// <summary>
		/// Do the list items link to the same page this control is on
		/// </summary>
		/// <remarks>
		/// If true, this will assume a link format of [thisPage]?id=[id].
		/// </remarks>
		public bool LinkToSamePage {
			set { _linkToSamePage = value; } get { return _linkToSamePage; }
		}

		/// <summary>
		/// Render icon next to item to update hidden ID field
		/// </summary>
		public bool ShowInformationIcon {
			get { return _showInfoIcon; } set { _showInfoIcon = value; }
		}

		/// <summary>
		/// Optional tip to display for information icon
		/// </summary>
		public string InformationIconTip {
			get { return _infoIconTip; } set { _infoIconTip = value; }
		}

		/// <summary>
		/// The ID of the hidden field that will hold the ID of the clicked
		/// list item information icon
		/// </summary>
		public string InformationIconTargetFieldID {
			set { _infoIconTargetFieldID = value; }
		}

		/// <summary>
		/// The dialog control that will be displayed when the information
		/// icon is clicked
		/// </summary>
		public Dialog<T> InformationIconTargetDialog {
			set { _infoIconTargetFieldID = value.HiddenFieldID; }
		}

		/// <summary>
		/// Should checkboxes be rendered by each item
		/// </summary>
		public bool ShowCheckbox {
			set { _checkboxes = value; } protected get { return _checkboxes; }
		}

		public List<T> Selected {
			get {
				if (_selected == null) { _selected = new List<T>(); }
				return _selected;
			}
			protected set { _selected = value; }
		}

		public string Label { set { _label = value; } }

		public bool HasSelection { get { return (_selected != null && _selected.Count > 0); } }

		#endregion

		#region IPostBackDataHandler

		public bool LoadPostData(string key, NameValueCollection posted) {
			string values = posted[_checkboxName];
			if (!string.IsNullOrEmpty(values)) {
				_selected = new List<T>();
				if (values.Contains(",")) {
					foreach (string s in values.Split(',')) {
						this.AddSelection(s);
					}
				} else {
					this.AddSelection(values);
				}
			}
			return false;
		}
		public void RaisePostDataChangedEvent() { }

		private void AddSelection(string raw) {
			object value = null;

			if (typeof(T).Equals(typeof(Guid))) {
				value = (object)(new Guid(raw));
			} else if (typeof(T).Equals(typeof(int))) {
				value = (object)int.Parse(raw);
			} else {
				value = (object)raw;
			}
			_selected.Add((T)value);
		}

		#endregion

		/// <summary>
		/// The ID for a particular item
		/// </summary>
		/// <remarks>
		/// This only needs to be implemented for lists linking to the same page so this control
		/// knows how to build the link.
		/// </remarks>
		public abstract string ItemID(int index);

		/// <summary>
		/// Number of items in the implementing control
		/// </summary>
		public abstract int Count { get; }

		#region Events

		protected override void OnInit(EventArgs e) {
			_checkboxName = "cbx" + this.ID;
			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e) {
			if (this.Count == 0) { this.Visible = false; }
			if (string.IsNullOrEmpty(this.CssClass)) { this.CssClass = "panelList"; }
			if (_checkboxes) { this.Page.RegisterRequiresPostBack(this); }
			base.OnLoad(e);
		}

		#endregion

		#region Render

		protected override void Render(HtmlTextWriter writer) {
			this.RenderBeginTag(writer);
			if (_linkToSamePage) {
				for (int x = 0; x < this.Count; x++) {
					writer.Write("<li>");
					if (_checkboxes) { this.RenderCheckBox(x, writer); }
					writer.Write("<a href=\"");
					writer.Write(this.Page.FileName);
					writer.Write("?id=");
					writer.Write(this.ItemID(x));
					writer.Write("\">");
					this.RenderItem(x, writer);
					writer.Write("</a></li>");
				}
			} else {
				// the implementing control must supply link markup with each item
				for (int x = 0; x < this.Count; x++) {
					writer.Write("<li>");
					if (_checkboxes) { this.RenderCheckBox(x, writer); }
					this.RenderItem(x, writer);
					writer.Write("</li>");
				}
			}
			this.RenderEndTag(writer);	
		}

		protected void RenderLabel(HtmlTextWriter writer) {
			if (!string.IsNullOrEmpty(_label)) {
				writer.Write("<label for=\"{0}\">{1}</label>", this.ID, _label);
			}
		}

		protected override void RenderBeginTag(HtmlTextWriter writer) {
			this.RenderLabel(writer);
			writer.Write("<div");
			this.RenderAttributes(writer);
			writer.Write("><ul>");
		}

		protected void RenderEndTag(HtmlTextWriter writer) {
			writer.Write("</ul></div>");
		}

		protected virtual void RenderItem(int index, HtmlTextWriter writer) {
			throw new System.Exception("The method or operation is not implemented.");
		}
		protected virtual void RenderItem(string name, T id, HtmlTextWriter writer) {
			writer.Write("<li>");
			if (_checkboxes) {
				bool selected = this.RenderCheckBox(id, writer);
				writer.Write("<label ");
				if (selected) { writer.Write("class=\"selected\" "); }
				writer.Write("for=\"cbx{0}_{1}\">{2}</label>", this.ID, id, name);
			} else {
				writer.Write(name);
			}
			this.RenderInformationIcon(name, id, writer);
			writer.Write("</li>");
		}
		protected virtual void RenderInformationIcon(string name, T id, HtmlTextWriter writer) {
			if (_showInfoIcon) {
				string tip = _infoIconTip;
				if (string.IsNullOrEmpty(tip)) {
					tip = "View more information about " + name;
				}
				writer.Write("<img class=\"linkOut\" src=\"/images/icons/link-out.gif\"");
				writer.Write (" alt=\"{0}\" title=\"{0}\" ", tip);
				writer.Write("onmouseout=\"Button(this, false);\" ");
				writer.Write("onmouseover=\"Button(this, true);\" ");
				writer.Write("onclick=\"DOM.SetValue('{0}', '{1}');\"/>",
					_infoIconTargetFieldID, id);
			}
		}
		/*
		protected void RenderItem(Entity e, HtmlTextWriter writer) {
			writer.Write("<li>");
			if (_checkboxes) {
				this.RenderCheckBox(e.ID, writer);
				writer.Write("<label for=\"cbx{0}_{1}\">{2}</label>", this.ID, e.ID, e.ToString());
			} else {
				writer.Write(e.ToString());
			}
			writer.Write("</li>");
		}
		*/
		protected bool RenderCheckBox(T id, HtmlTextWriter writer, bool selected) {
			writer.Write("<input type=\"checkbox\" id=\"cbx");
			writer.Write(this.ID);
			writer.Write("_");
			writer.Write(id);
			writer.Write("\" name=\"");
			writer.Write(_checkboxName);
			writer.Write("\" value=\"");
			writer.Write(id);
			writer.Write("\"");
			if (selected) { writer.Write(" checked=\"checked\""); }
			writer.Write("/>");

			return selected;
		}
		protected bool RenderCheckBox(T id, HtmlTextWriter writer) {
			bool selected = (_selectAll == true || 
				(_selected != null && _selected.Contains(id)));
			return this.RenderCheckBox(id, writer, selected);
		}
		protected bool RenderCheckBox(int id, HtmlTextWriter writer) {
			return this.RenderCheckBox((T)(object)id, writer);
		}
		/*
		protected void RenderCheckBox(string id, HtmlTextWriter writer) {
			this.RenderCheckBox(id, writer, false);
		}
		
		
		protected void RenderCheckBox(Guid id, HtmlTextWriter writer) {
			this.RenderCheckBox(id.ToString(), writer);
		}
		*/

		#endregion

	}
}
