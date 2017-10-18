using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class EnumList : Idaho.Web.Controls.SelectList {

		private Type _type = null;
		private int _mask = -1;
		private bool _bitmask = true;
		private bool _fitAllRows = false;
		private List<int> _exclude;
		private bool _categorize = false;
		private string _defaultCategory = "Other";
		// bitmask
		private int _selected = 0;

		#region Properties

		public bool IsBitmask { set { _bitmask = value; } }

		/// <summary>
		/// Integer value of selected enumeration(s)
		/// </summary>
		public new int Selected { get { return _selected; } set { _selected = value; } }
		public new bool Posted { get { return (!(_selected == 0)); } }
		public new int FirstSelection { get { return _selected; } }

		/// <summary>
		/// Type of enumeration to list
		/// </summary>
		public Type Type { get { return _type; } set { _type = value; } }

		public bool FitAllRows { set { _fitAllRows = value; } protected get { return _fitAllRows; } }
		
		/// <summary>
		/// Look for EnumDescription attribute to create select option groups
		/// </summary>
		public bool Categorize { set { _categorize = value; } }

		/// <summary>
		/// Category name to use when categorization is required but no
		/// category attribute is found on the enum field
		/// </summary>
		public string DefaultCategory { set { _defaultCategory = value; } }
		
		/// <summary>
		/// List of enumeration values to exclude from display
		/// </summary>
		public int Exclude {
			set {
				if (_exclude == null) { _exclude = new List<int>(); }
				_exclude.Add(value);
			}
		}

		/// <summary>
		/// Bitmask to filter returned enums
		/// </summary>
		public int Mask { get { return _mask; } set { _mask = value; } }

		#endregion

		#region IPostBackDataHandler

		public override bool LoadPostData(string key, NameValueCollection posted) {
			if (posted[key] != string.Empty) {
				if (this.Rows == 1) {
					_selected = int.Parse(posted[key]);
				} else {
					string[] selected = posted[key].Split(',');
					_selected = 0;
					for (int x = 0; x < selected.Length; x++) {
						_selected = _selected | int.Parse(selected[x]);
					}
				}
			}
			return false;
		}

		#endregion

		protected bool IsSelected(int value) {
			if (_selected == 0) {
				return false;
			} else {
				return (_bitmask) ? _selected.ContainsBits(value) : (_selected == value);
			}
		}
		protected override bool IsSelected(string value) {
			int test = 0;
			if (int.TryParse(value, out test)) {
				return this.IsSelected(test);
			} else {
				return false;
			}
		}

		/// <summary>
		/// Render a list of enumerations
		/// </summary>
		protected override void Render(HtmlTextWriter writer) {
			int[] values = (int[])Enum.GetValues(_type);
			string[] names = Enum.GetNames(_type);
			var list = new List<EnumField>();
			string group = string.Empty;

			for (var x = 0; x < values.Length; x++) {
				list.Add(new EnumField(names[x], values[x], _defaultCategory));
			}
			if (_categorize) {
				// get category names from enum field attributes
				foreach (EnumField e in list) {
					var f = _type.GetField(e.Name);
					if (f != null) {
						var a = f.GetAttribute<Attributes.EnumDescription>();
						if (a != null) { e.Category = a.Category; }
					}
				}
			}
			list.Sort();

			if (_fitAllRows) { this.Rows = list.Count; }

			this.RenderBeginTag(writer);

			foreach (EnumField e in list) {
				if ((_mask == -1 || e.Value.ContainsBits(_mask))
					&& (!_bitmask || e.Value.IsFlag())
					&& (_exclude == null || !_exclude.Contains(e.Value))) {

					if (_categorize && group != e.Category) {
						if (group != string.Empty) { this.RenderGroupEnd(writer); }
						group = e.Category;
						this.RenderGroupBegin(group, writer);
					}
					this.RenderOption(e.Name.FixSpacing(), e.Value, writer);
				}
			}
			if (_categorize) { this.RenderGroupEnd(writer); }
			this.RenderEndTag(writer);
		}

		private class EnumField : IComparable<EnumField> {
			public string Name { get; set; }
			public string Description { get; set; }
			public string Category { get; set; }
			public int Value { get; set; }

			public EnumField(string name, int value) {
				this.Name = name;
				this.Value = value;
			}
			public EnumField(string name, int value, string category)
				: this(name, value) {

				this.Category = category;
			}

			public int CompareTo(EnumField other) {
				int compare = 0;
				if (!string.IsNullOrEmpty(this.Category)) {
					compare = this.Category.CompareTo(other.Category);
				}
				if (compare == 0) {
					compare = this.Name.CompareTo(other.Name);
				}
				return compare;
			}
		}
	}
}