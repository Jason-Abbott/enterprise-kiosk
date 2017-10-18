using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class EnumCheckbox : Idaho.Web.Controls.HtmlControl, IPostBackDataHandler {

		private System.Type _type;
		private bool _group = true;
		private int _mask = -1;
		private int _selected = 0;
		private bool _flagsOnly = false;

		#region Properties

		/// <summary>
		/// Only display flag enumerations (powers of two)
		/// </summary>
		public bool FlagsOnly { set { _flagsOnly = value; } }

		/// <summary>
		/// If grouped, all checkbox fields have the same HTML name which
		/// causes them to post as a single comma-delimited value
		/// </summary>
		public bool Group { get { return _group; } set { _group = value; } }
		public int Selected { get { return _selected; } set { _selected = value; } }
		//public T Selected<T> { get { return (T)_selected; } }
		//public Enum Selected { set { _selected = (int)value; } }

		/// <summary>
		/// Enumeration type
		/// </summary>
		public Type Type { get { return _type; } set { _type = value; } }
		public string TypeName { set { _type = System.Type.GetType(value); } }

		/// <summary>
		/// Bitmask to filter returned enums
		/// </summary>
		public int Mask { get { return _mask; } set { _mask = value; } }

		#endregion

		#region IPostBackDataHandler

		/// <summary>
		/// Get enum values
		/// </summary>
		public bool LoadPostData(string key, NameValueCollection posted) {
			if (posted[key] != string.Empty) {
				string[] selected = posted[key].Split(',');
				_selected = 0;
				//_selected;
				for (int x = 0; x < selected.Length; x++) {
					// combine bits
					_selected = _selected.AddBits(int.Parse(selected[x]));
				}
			}
			return false;
		}
		public void RaisePostDataChangedEvent() { }

		#endregion

		/// <summary>
		/// Check if value is present in bitmask of selected enumerations
		/// </summary>
		protected bool IsSelected(int value) {
			return _selected.ContainsBits(value);
		}

		/// <summary>
		/// Render a list of enumerations
		/// </summary>
		protected override void Render(HtmlTextWriter writer) {
			int[] values = (int[])Enum.GetValues(_type);
			string[] names = Enum.GetNames(_type);
			string id;

			Array.Sort(names, values);
			for (int x = 0; x < values.Length; x++) {
				if (_mask == -1 || _mask.ContainsBits(values[x])) {
					if (!_flagsOnly || values[x].IsFlag()) {
						id = string.Format("{0}_{1}", this.ID, names[x]);
						writer.Write("<div class=\"enum\"><input id=\"");
						writer.Write(id);
						if (_group) {
							// treat as group
							writer.Write("\" name=\"");
							writer.Write(this.UniqueID);
						}
						writer.Write("\" type=\"checkbox\" value=\"");
						writer.Write(values[x]);
						writer.Write("\"");
						if (this.IsSelected(values[x])) { writer.Write(" checked=\"checked\""); }
						writer.Write("><label for=\"");
						writer.Write(id);
						writer.Write("\"");
						this.RenderStyle(writer);
						writer.Write(">");
						writer.Write(names[x].FixSpacing());
						writer.Write("</label></div>");
					}
				}
			}
		}
	}
}