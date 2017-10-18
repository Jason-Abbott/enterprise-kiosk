using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Idaho.Web.Controls {
	public class GridColumn : IEquatable<GridColumn> {

		private Type _itemType;
		private string _title = string.Empty;
		private PropertyInfo _property;
		private PropertyInfo _tipTextProperty;
		private bool _isDetailLink = false;
		private int _maxLength = 0;
		private string _format = string.Empty;
		private bool _group = false;
		private bool _isDateAndTime = false;
		private TimeSpan _offset = TimeSpan.Zero;
		private bool _isIP = false;
		private string _htmlValue = string.Empty;
		private string _tipText = string.Empty;
		private string _heading = string.Empty;
		private string _url = string.Empty;
		private bool _spanToEnd = false;
		private int _row = 1;
		private int _colSpan = 1;
		private int _rowSpan = 1;
		private object _value;
		private object _groupValue;
		private string _style = string.Empty;
		private string _cssClass = string.Empty;
		private const string _emptyCell = "&mdash;";
		private CssDelegate _cssDelegate = null;

		/// <summary>
		/// Grid subclass may create delegate to add CSS styling to a column
		/// based on entity-specific criteria
		/// </summary>
		public delegate string CssDelegate(object entity);

		#region Properties

		/// <summary>
		/// Specify a delegate to add styling
		/// </summary>
		public CssDelegate StyleDelegate {
			set { _cssDelegate = value; } internal get { return _cssDelegate; }
		}

		/// <summary>
		/// Number of columns this one should span
		/// </summary>
		public int ColumnSpan { internal protected set { _colSpan = value; } get { return _colSpan; } }

		/// <summary>
		/// The HTML row this column renders on in the data row
		/// </summary>
		/// <remarks>
		/// This is calculated internally to accomodated column span settings.
		/// </remarks>
		internal int Row { set { _row = value; } get { return _row; } }

		/// <summary>
		/// Number of rows to span with a given column cell
		/// </summary>
		/// <remarks>
		/// This is calculated internally to accommodate column span settings.
		/// </remarks>
		internal int RowSpan { set { _rowSpan = value; } get { return _rowSpan; } }

		/// <summary>
		/// Similar to a heading, should this column span the other columns
		/// to the end of the row
		/// </summary>
		/// <remarks>
		/// This will set the Span property and override any value specified there.
		/// </remarks>
		public bool SpanToEnd { set { _spanToEnd = value; } internal get { return _spanToEnd; } }

		/// <summary>
		/// Does the text in this column link to the detail page
		/// </summary>
		public bool IsDetailLink {
			set {
				_isDetailLink = value;
				if (_isDetailLink && string.IsNullOrEmpty(_tipText)) {
					_tipText = Resource.Say("Tip_GridDetailLink");
				}
			}
			get { return _isDetailLink; }
		}

		/// <summary>
		/// Specify an offset to localize time
		/// </summary>
		public TimeSpan Offset { set { _offset = value; } }

		public string TipTextField {
			set {
				_tipTextProperty = _itemType.GetProperty(value);
				Assert.NoNull(_tipTextProperty, "NullReflectedProperty", value, _itemType.Name);
			}
		}

		/// <summary>
		/// Url this column should link to
		/// </summary>
		public string Url { set { _url = value; } }

		/// <summary>
		/// The maximum length of text in the column
		/// </summary>
		public int MaxLength { set { _maxLength = value; } }

		/// <summary>
		/// Display tip with mouseover
		/// </summary>
		public string TipText { set { _tipText = value; } }

		/// <summary>
		/// The formatted value for display in the column
		/// </summary>
		public string Value { get { return _htmlValue; } }

		/// <summary>
		/// The formatted value for display as a row group heading
		/// </summary>
		public string Heading { get { return _heading; } }

		/// <summary>
		/// CSS class
		/// </summary>
		public string CssClass { get { return _cssClass; } set { _cssClass = value; } }

		/// <summary>
		/// CSS style rules
		/// </summary>
		public string Style { get { return _style; } set { _style = value; } }

		/// <summary>
		/// When looping through items, checks if header is needed
		/// </summary>
		internal bool NeedHeader {
			get {
				if (_group && _value != null) {
					bool changed = false;
					if (_groupValue == null) {
						changed = true;
					} else if (_isDateAndTime) {
						DateTime t1 = (DateTime)_value;
						DateTime t2 = (DateTime)_groupValue;
						changed = !t1.SameDay(t2);
					} else {
						changed = !_value.Equals(_groupValue);
					}
					if (changed) {
						_groupValue = _value;
						return true;
					}
				}
				return false;
			}
		}
		
		/// <summary>
		/// Should the column heading be rendered
		/// </summary>
		/// <remarks>
		/// A grouped column is rendered as a header. If the grouped column
		/// is a DateTime then it is rendered both as a header and column, the
		/// header to show the date and the column to show the time. A column
		/// set to span the others does not have a visible title.
		/// </remarks>
		public bool VisibleHeading {
			get { return !_spanToEnd && (_isDateAndTime || !_group); }
		}
		public bool VisibleRow {
			get { return _isDateAndTime || !_group;  }
		}

		/// <summary>
		/// Convenience function to avoid frequent type comparison
		/// </summary>
		public bool IsDateTime { get { return _isDateAndTime; } }

		public bool IsEnum { get { return _property.PropertyType.IsEnum; } }

		/// <summary>
		/// Should these columns be grouped into a heading
		/// </summary>
		public bool Group { internal set { _group = value; } get { return _group; } }

		/// <summary>
		/// The title to render in the table heading
		/// </summary>
		public string Title { set { _title = value; } get { return _title; } }

		/// <summary>
		/// Format string to apply to view value
		/// </summary>
		public string Format { set { _format = value; } get { return _format; } }

		/// <summary>
		/// The property to read a value from
		/// </summary>
		/// <remarks>
		/// The containing GridColumnCollection has a reference to the type
		/// containing this property.
		/// </remarks>
		public PropertyInfo Property {
			set {
				_property = value;
				Type t = _property.PropertyType;

				if (t.Equals(typeof(DateTime))) {
					_isDateAndTime = true;
					_cssClass = "date";
				} else if (t.Equals(typeof(Network.IpAddress))) {
					_isIP = true;
					_cssClass = "ip";
				}
			}
			internal get { return _property; }
		}

		#endregion

		public GridColumn(Type t) { _itemType = t; }

		/// <summary>
		/// Read and parse object value for this column
		/// </summary>
		public void Read(ILinkable o) {
			string tipText = _tipText;
			_value = _property.GetValue(o, null);

			if (_value != null) {
				// determine the type of column
				if (this.IsEnum) {
					_htmlValue = _value.ToString().FixSpacing();
				} else if (!string.IsNullOrEmpty(_format)) {
					if (_value is DateTime) {
						DateTime t = (DateTime)_value;
						if (t == DateTime.MaxValue || t == DateTime.MinValue) {
							_htmlValue = _emptyCell;
						} else {
							_htmlValue = ((DateTime)_value).ToString(_format);
						}
					} else {
						_htmlValue = string.Format("{0:" + _format + "}", _value);
					}
				} else {
					if (_isDateAndTime && _group) {
						DateTime t = (DateTime)_value;
						_htmlValue = t.ToString("h:mm:ss tt", _offset);
						_heading = t.ToString("dddd, MMMM d, yyyy", _offset);
						return;
					} else if (_isIP) {
						_htmlValue = ((Network.IpAddress)_value).DetailLink;
					} else if (_value is Indicator) {
						_htmlValue = ((Indicator)_value).Name;
					} else {
						_htmlValue = _value.ToString();
					}
				}

				if (_maxLength > 0 && _htmlValue.Length > _maxLength) {
					tipText = _htmlValue;
					_htmlValue = _htmlValue.Substring(0, _maxLength) + "...";
				}
				if (_tipTextProperty != null) {
					tipText = _tipTextProperty.GetValue(o, null).ToString();
				}
				if (_isDetailLink) { _url = o.DetailUrl; }

				// add markup for link or tip
				if (!string.IsNullOrEmpty(_url)) {
					// text should be item ID if this is a detail link column
					string url = string.Format(_url, _htmlValue);
					_htmlValue = string.Format("<a href=\"{0}\" title=\"{1}\">{2}</a>", url, tipText, _htmlValue);
				} else if (!string.IsNullOrEmpty(tipText)) {
					_htmlValue = string.Format("<acronym title=\"{0}\">{1}</acronym>", tipText, _htmlValue);
				}
			} else {
				// null value--may happen for errors that occur in separate thread
				_htmlValue = _emptyCell;
			}
			_heading = _htmlValue;
		}

		#region IEquatable

		public bool Equals(GridColumn other) {
			return _property.Name.Equals(other.Property.Name, Idaho.Format.IgnoreCase);
		}

		#endregion

	}
}
