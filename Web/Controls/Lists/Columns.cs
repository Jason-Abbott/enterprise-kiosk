using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Display items in columns
	/// </summary>
	public class Columns : HtmlControl {
		private bool _balance = true;
		private int _length = -1;
		private int _count = 2;
		private List<string> _items = null;

		#region Properties

		protected List<string> Items { set { _items = value; } }

		/// <summary>
		/// Put an equal number of items in each column
		/// </summary>
		public bool Balance { set { _balance = value; } get { return _balance; } }

		/// <summary>
		/// Set to force a specific number of items in each column
		/// </summary>
		public int Length { set { _length = value; } get { return _length; } }

		/// <summary>
		/// How many columns should the items be rendered into
		/// </summary>
		public int ColumnCount { set { _count = value; } get { return _count; } }

		#endregion

		public delegate string ToString<T>(T e);

		/// <summary>
		/// Bind collection to columns
		/// </summary>
		/// <param name="list">Collection to be bound</param>
		/// <param name="toString">Delegate that converts item to string</param>
		public void DataBind<T>(List<T> list, ToString<T> toString) {
			_items = new List<string>();
			foreach (T e in list) { _items.Add(toString(e)); }
		}

		/// <summary>
		/// Compute any needed changes to length or count
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e) {
			if (_items == null || _items.Count == 0) {
				this.Visible = false;
			} else {
				// can't have more columns than items
				if (_items.Count < _count) { _count = _items.Count; }
				int computedLength = (int)(_items.Count / _count);
				if (_items.Count % ColumnCount > 0) { computedLength++; }
				_length = computedLength;
				// check if last column would be empty
				if (_items.Count % _length == 0) {
					_count = (int)(_items.Count / _length);
				}
				this.CssClass = "columns";
				base.OnLoad(e);
			}
		}

		/// <summary>
		/// Render items within control
		/// </summary>
		protected override void Render(HtmlTextWriter writer) {
			int row = 1;
			int column = 1;
			int x = 1;
			string width = ((100 / _count) - 2).ToString();
			// start all columns
			writer.Write("<div");
			this.RenderAttributes(writer);
			// start first column
			writer.Write("><div style=\"width: {0}%;\">", width);

			foreach (string s in _items) {
				writer.Write("<span>{0}</span>", s);
				if (row % _length == 0) {
					writer.Write("</div>");
					if (x < _items.Count) {
						writer.Write("<div style=\"width: {0}%;", width);
						if (x + _length >= _items.Count) {
							// disable border on last column
							writer.Write("border-right: none;");
						}
						writer.Write("\">");
					}
					row = 1;
					column++;
				} else {
					row++;
				}
				x++;
			}
			// end last column and all columns
			writer.Write("</div>");
		}
	}
}
