using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Collecion of HTML grid columns
	/// </summary>
	public class GridColumnCollection : List<GridColumn> {

		/// <summary>
		/// The type containing the properties that will be used to
		/// populate the grid
		/// </summary>
		private Type _itemType;
		private bool _hasGrouping = false;
		private bool _needHeader = false;
		private int _totalRows = 1;

		public GridColumnCollection(Type itemType) { _itemType = itemType; }

		#region Properties

		/// <summary>
		/// Total HTML rows used by these columns for a data row
		/// </summary>
		internal int TotalRows { set { _totalRows = value; } get { return _totalRows; } }

		public string Heading {
			get {
				foreach (GridColumn g in this) { if (g.Group) { return g.Heading; } }
				return string.Empty;
			}
		}

		/// <summary>
		/// When looping through rows, checks if header is needed
		/// </summary>
		public bool NeedHeader { get { return _needHeader; } }

		/// <summary>
		/// How many of the grid columns are visible as table columns
		/// </summary>
		public int VisibleCount {
			get {
				int less = 0;
				
				if (_hasGrouping) {
					foreach (GridColumn c in this) {
						if (c.Group) {
							less = c.IsDateTime ? 0 : 1;
							break;
						}
					}
				}
				return this.Count - less;
			}
		}

		public bool HasGrouping { get { return _hasGrouping; } }

		/// <summary>
		/// Return column with given property name
		/// </summary>
		public GridColumn this[string name] {
			get {
				foreach (GridColumn g in this) {
					if (g.Property.Name == name) { return g; }
				}
				return null;
			}
		}

		public string GroupBy {
			set {
				GridColumn c = this[value];
				if (c == null) { return; }
				foreach (GridColumn gc in this) { gc.Group = false;	}
				c.Group = true;
				_hasGrouping = true;
			}
		}

		#endregion

		/// <summary>
		/// Compute row and column spanning
		/// </summary>
		internal void ComputeSpanning() {
			int count = this.Count;
			foreach (GridColumn c in this) {
				if (c.SpanToEnd) { c.ColumnSpan = count; }
				count--;
			}
			int start = 0, end = 0;
			int x = 1, y;

			foreach (GridColumn c in this) {
				if (c.ColumnSpan > 1) {
					y = 1;
					start = x;
					end = (start + c.ColumnSpan) - 1;

					foreach (GridColumn c2 in this) {
						if (y < start || y > end) {
							c2.RowSpan++;
						} else if (!c2.Equals(c)) {
							// rows below spanning column
							c2.Row++;
						}
						y++;
					}
				}
				x++;
			}
			// total HTML row count
			foreach (GridColumn c in this) {
				if (c.Row > _totalRows) { _totalRows = c.Row; }
			}
		}

		/// <summary>
		/// Allow each column to read its object value for this row
		/// </summary>
		public void ReadRow(ILinkable o) {
			_needHeader = false;
			foreach (GridColumn g in this) {
				g.Read(o);
				if (g.NeedHeader) { _needHeader = true; }
			}
		}

		public void ClearGrouping() {
			foreach (GridColumn c in this) {
				c.Group = false;
			}
			_hasGrouping = false;
		}

		/// <summary>
		/// Add a new column
		/// </summary>
		public GridColumn Add(string propertyName, string columnName, string format) {
			GridColumn c = new GridColumn(_itemType);
			c.Title = columnName;
			c.Property = this.GetProperty(_itemType, propertyName);
			Assert.NoNull(c.Property, "NullReflectedProperty", propertyName, _itemType.Name);
			c.Format = format;
			this.Add(c);
			return c;
		}
		public GridColumn Add(string propertyName, string columnName) {
			return this.Add(propertyName, columnName, string.Empty);
		}
		public GridColumn Add(string propertyName) {
			return this.Add(propertyName, propertyName, string.Empty);
		}

		/// <summary>
		/// Private add so rules are enforced
		/// </summary>
		private new void Add(GridColumn c) { base.Add(c); }

		/// <summary>
		/// Find the property with the given name
		/// </summary>
		/// <remarks>
		/// In the case of overridden names, this won't choke on the ambiguity.
		/// </remarks>
		private PropertyInfo GetProperty(Type t, string name) {
			if (t == null || string.IsNullOrEmpty(name)) { return null; }
			PropertyInfo[] properties = t.GetProperties();
			foreach (PropertyInfo p in properties) {
				if (p.Name.Equals(name, Format.IgnoreCase)) { return p; }
			}
			// if still here then try other approach
			return t.GetProperty(name);
		}
	}
}
