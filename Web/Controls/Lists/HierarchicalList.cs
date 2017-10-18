using Idaho.Web.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class HierarchicalList<T> : PanelList<Guid>
		where T: Entity, IHierarchical<T> {

		private IHierarchicalCollection<T> _items = null;
		private List<T> _selected = null;
		private string _rootName = string.Empty;
		private bool _addRootItem = false;
		private bool _hideRootItem = false;
		private bool _cascadeDown = false;
		private bool _cascadeUp = false;
		private int _count = 0;

		#region Properties

		/// <summary>
		/// Parent item checkbox selects child item checkboxes
		/// </summary>
		public bool CascadeDown { set { _cascadeDown = value; } }

		/// <summary>
		/// Child item checkbox selects parent item checkbox
		/// </summary>
		public bool CascadeUp { set { _cascadeUp = value; } }

		/// <summary>
		/// The item that should be above all others
		/// </summary>
		public string RootNode {
			set {
				_rootName = value;
				_addRootItem = !string.IsNullOrEmpty(_rootName);
			}
		}

		/// <summary>
		/// Should the root node be hidden
		/// </summary>
		public bool HideRootItem { set { _hideRootItem = value; } }

		public IHierarchicalCollection<T> Items {
			set { _items = value; }
		}
		
		public override int Count {
			get {
				return (_items != null) ? _items.Count : 0;
			}
		}

		/// <summary>
		/// Were all items selected
		/// </summary>
		public override bool SelectAll {
			set { base.SelectAll = value; }
			get {
				if (_selectAll == null) {
					_selectAll = base.Selected != null &&
						_items != null &&
						base.Selected.Count == _items.TotalCount - 2;
				}
				return _selectAll == true;
			}
		}

		/// <summary>
		/// Items that were selected
		/// </summary>
		public new List<T> Selected {
			get {
				_selected = null;
				if (base.Selected != null && base.Selected.Count > 0) {
					_selected = new List<T>();
					base.Selected.ForEach(id => _selected.Add(_items[id]));
				}
				return _selected;
			}
			set {
				_selected = value;
				if (_selected != null) {
					_selected.ForEach(i => base.Selected.Add(i.ID));
				}
			}
		}

		#endregion

		#region Events

		protected override void OnLoad(EventArgs e) {
			if (_items == null || _items.Count == 0) {
				this.Visible = false;
			} else if (this.Visible) {
				this.Page.ScriptFile.AddResource(EcmaScript.Resource.HierarchicalList);
				//this.Page.ScriptFile.Add("hierarchy");
				this.CssClass = "hierarchy";
				this.Page.ScriptVariable(this.ID);
				this.Page.ScriptEvent = string.Format(
					"{0} = new HierarchyObject('{0}',{1},{2})",
					this.ID, _cascadeDown.ToJSON(), _cascadeUp.ToJSON());
			}
			base.OnLoad(e);
		}

		#endregion

		#region Render

		/// <summary>
		/// Render markup if given name/value collection
		/// </summary>
		protected override void Render(HtmlTextWriter writer) {
			if (_items != null && _items.Count > 0) {
				this.RenderBeginTag(writer);
				if (_addRootItem) {
					writer.Write("<li>");
					this.RenderGroupItem(_rootName, Guid.Empty, writer);
					writer.Write("<ul style=\"font-size: smaller;\">");
				} else if (_hideRootItem && _items.Count == 1) {
					_items = _items[0].Children;
				}
				foreach (T i in _items) { this.Render(i, writer); }
				if (_addRootItem) { writer.Write("</ul></li>"); }
				this.RenderEndTag(writer);
			}
		}

		private void Render(T i, HtmlTextWriter writer) {
			if (i != null) {
				if (i.Children != null && i.Children.Count > 0) {
					writer.Write("<li>");
					this.RenderGroupItem(i, writer);
					this.RenderInformationIcon(i.ToString(), i.ID, writer);
					writer.Write("<ul style=\"font-size: smaller;\">");
					foreach (T e in i.Children) { this.Render(e, writer); }
					writer.Write("</ul></li>");
				} else {
					// render option if single or no child item
					this.RenderItem(i.ToString(), i.ID, writer);
				}
				_count++;
			}
		}

		private void RenderGroupItem(string name, Guid id, HtmlTextWriter writer) {
			if (this.ShowCheckbox) {
				this.RenderCheckBox(id, writer);
				writer.Write("<label for=\"cbx{0}_{1}\">{2}</label>",
					this.ID, id, name);
			} else {
				writer.Write(name);
			}
		}
		private void RenderGroupItem(T i, HtmlTextWriter writer) {
			this.RenderGroupItem(i.ToString(), i.ID, writer);
		}

		#endregion

		public override string ItemID(int index) {
			throw new System.Exception("The method or operation is not implemented.");
		}
	}
}
