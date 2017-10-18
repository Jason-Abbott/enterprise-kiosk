using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web.Caching;
using System.Web.UI;
using web = System.Web.UI.WebControls;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Override this control and its Render() to create an HTML table with
	//  optional context menu.
	/// </summary>
	/// <remarks>
	/// It depends on EcmaScript files for asynhchronous
	/// behavior and context menu genaration.
	/// </remarks>
	public abstract class Grid<T> : Idaho.Web.Controls.AjaxControl where T: class, ILinkable {

		private GridColumnCollection _columns;
		private string _clickTarget = string.Empty;
		//private bool _showCheckBox = false;
		//private bool _mouseHighlights = true;
		private string _title;
		private List<T> _data;
		private string _detailPage = string.Empty;
		private CssDelegate _cssDelegate = null;
		private Dictionary<string, IComparer<T>> _sorters = null;
		private NameValue<string, Entity.SortDirections> _sort = null;
		private NameValue<string, Entity.SortDirections> _defaultSort = null;
		private CacheItemPriority _cachePriority = CacheItemPriority.Normal;
		private TimeSpan _cacheDuration = TimeSpan.MinValue;
		private string _cacheKey = string.Empty;
		private bool _showCount = false;

		/// <summary>
		/// Subclass may create delegate to add CSS styling to a row
		/// based on entity-specific criteria
		/// </summary>
		public delegate string CssDelegate(T entity);

		#region Properties

		protected TimeSpan CacheDuration { set { _cacheDuration = value; } }
		protected CacheItemPriority CachePriority { set { _cachePriority = value; } }
		protected string CacheKey {
			set { _cacheKey = value; }
			get {
				if (string.IsNullOrEmpty(_cacheKey)) {
					// if a key hasn't been set by a subclass, generate one
					StringBuilder keys = new StringBuilder();
					foreach (string k in this.Context.Request.Form.Keys) {
						if (!k.StartsWith("sort", Format.IgnoreCase)) {
							keys.AppendFormat("{0}={1};", k, this.Context.Request.Form[k]);
						}
					}
					_cacheKey = keys.ToString();
				}
				return _cacheKey;
			}
		}

		/// <summary>
		/// Specify a delegate to add styling
		/// </summary>
		protected CssDelegate StyleDelegate { set { _cssDelegate = value; } }

		/// <summary>
		/// Number of minutes to cache grid data; default is zero, no caching.
		/// </summary>
		[WebBindable()]
		public int CacheMinutes {
			set { _cacheDuration = TimeSpan.FromMinutes(value);	}
			get { return _cacheDuration.Minutes; }
		}

		/// <summary>
		/// Show record count above grid when rendering
		/// </summary>
		[WebBindable()]
		public bool ShowCount { get { return _showCount; } set { _showCount = value; } }

		/// <summary>
		/// Target page for item detail
		/// </summary>
		[WebBindable()]
		public string DetailPage { set { _detailPage = value; } get { return _detailPage; } }

		/// <summary>
		/// Default Column to sort on
		/// </summary>
		public GridColumn DefaultSorter {
			set {
				if (_defaultSort == null) { _defaultSort = new NameValue<string, Entity.SortDirections>(); }
				Assert.ContainsKey<string, IComparer<T>>(value.Title, _sorters, "Error_MissingGridSorterKey");
				_defaultSort.Name = value.Title;
			}
		}

		/// <summary>
		/// Default direction to sort
		/// </summary>
		public Entity.SortDirections DefaultSortDirection {
			set {
				if (_defaultSort == null) { _defaultSort = new NameValue<string, Entity.SortDirections>(); }
				_defaultSort.Value = value;
			}
		}

		/// <summary>
		/// Column to sort on
		/// </summary>
		[WebBindable()]
		public string SortBy {
			set {
				if (_sort == null) { _sort = new NameValue<string, Entity.SortDirections>(); }
				_sort.Name = value;
			}
			get { return (_sort == null) ? string.Empty : _sort.Name; }
		}

		/// <summary>
		/// Direction to sort
		/// </summary>
		[WebBindable()]
		public Entity.SortDirections SortDirection {
			set {
				if (_sort == null) { _sort = new NameValue<string, Entity.SortDirections>(); }
				_sort.Value = value;
			}
			get { return (_sort == null) ? 0 : _sort.Value; }
		}

		/// <summary>
		/// The data displayed in this grid
		/// </summary>
		protected List<T> Data {
			set {
				_data = value;
				if (_cacheDuration > TimeSpan.MinValue && _data != null && _data.Count > 0) {
					base.Context.Cache.Add(this.CacheKey, _data, null,
						DateTime.Now + _cacheDuration, Cache.NoSlidingExpiration, _cachePriority, null);
				}
			}
			get {
				if (_data == null && _cacheDuration > TimeSpan.MinValue) {
					_data = base.Context.Cache[this.CacheKey] as List<T>;
				}
				return _data;
			}
		}
		protected bool HasCacheData { get { return this.Data != null; } }

		/// <summary>
		/// The columns displayed in this grid
		/// </summary>
		protected GridColumnCollection Columns { get { return _columns; } }

		/// <summary>
		/// Title rendered above grid
		/// </summary>
		[WebBindable()]
		public string Title { get { return _title; } set { _title = value; } }

		/// <summary>
		/// DOM row ID is appended to this for each table row
		/// </summary>
		/// <remarks>
		/// Used to make each row clickable for more detail. The target could
		/// be a page or script function.
		/// </remarks>
		/// <example>target.aspx?id=932</example>
		public string ClickTarget { set { _clickTarget = value; } }

		#endregion

		public Grid() {
			_columns = new GridColumnCollection(typeof(T));
		}

		#region Events

		protected override void OnInit(EventArgs e) {
			this.CssClass = "grid";
			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e) {
			if (this.Ajax.IsRenderState(AjaxBase.RenderStates.InPage)) {
				this.Page.StyleSheet.Add("grid");
			}
			if (this.Ajax.IsRenderingContent) {
				if (_data != null && _data.Count > 0) {
					_columns.ComputeSpanning();
				} else {
					this.Visible = false;
				}
			}
			base.OnLoad(e);
		}

		/// <summary>
		/// Prepare to render grid control
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			if (this.Ajax.IsRenderingContent && this.Visible && _sorters != null) {
				// determine sorting
				NameValue<string, Entity.SortDirections> savedSort = null;
				if (this.Profile.GridSort.ContainsKey(this.ID)) {
					savedSort = this.Profile.GridSort[this.ID];
				}

				if (_sort != null && _sort.IsValid) {
					// use specified sort (from click and AJAX processing)
					if (this.Profile.GridSort.ContainsKey(this.ID)) {
						this.Profile.GridSort[this.ID] = _sort;
					} else {
						this.Profile.GridSort.Add(this.ID, _sort);
					}
				} else if (savedSort == null) {
					// use default
					_sort = _defaultSort;
				} else {
					// use sort option saved in profile and cookie
					_sort = savedSort;
				}

				if (_sort != null && _sort.IsValid) {
					// convert sort specifications into comparer object
					IComparer<T> comparer = _sorters[_sort.Name];
					comparer.Direction = _sort.Value;
					if (comparer != null) { _data.Sort(comparer); }
				}
			}
			base.OnPreRender(e);
		}

		#endregion

		#region Rendering

		/// <summary>
		/// Rendering start point
		/// </summary>
		protected override void RenderContent(HtmlTextWriter writer) {
			if (_showCount) {
				writer.Write("<div id=\"");
				writer.Write(this.ID);
				writer.Write("Count\" class=\"gridCount\">");
				writer.Write(_data.Count.ToString());
				writer.Write(" match");
				if (_data.Count != 1) { writer.Write("es"); }
				writer.Write("</div>");
			}
			writer.Write("<table");
			this.RenderAttributes(writer);
			writer.Write(">");
			this.RenderHeadings(writer);
			this.RenderRows(writer);
			writer.Write("</table>");
		}

		/// <summary>
		/// Render column headings
		/// </summary>
		protected void RenderHeadings(HtmlTextWriter writer) {
			bool sortLink = false;
			string arrow = string.Empty;
			string sortClass = string.Empty;
			Entity.SortDirections direction = Entity.SortDirections.Ascending;

			writer.Write("<tr class=\"heading\">");
			foreach (GridColumn g in _columns) {
				if (g.VisibleHeading) {
					arrow = string.Empty;
					sortLink = (_sorters != null && _sorters.ContainsKey(g.Title));
					writer.Write("<th");
					if (sortLink) {
						if (_sort.Name == g.Title) {
							// reverse existing sort
							direction = (_sort.Value == Entity.SortDirections.Ascending) ? Entity.SortDirections.Descending : Entity.SortDirections.Ascending;
							sortClass = "sort" + ((direction == Entity.SortDirections.Ascending) ? "Up" : "Down");
							arrow = "<div class=\"" + sortClass + "\"></div>";
							writer.Write(" class=\"");
							writer.Write(sortClass);
							writer.Write("\"");
						} else {
							direction = Entity.SortDirections.Ascending;
						}
						writer.Write(">");
						writer.Write(arrow);
						writer.Write("<a href=\"javascript:");
						writer.Write(this.ID);
						writer.Write(".Sort('");
						writer.Write(g.Title);
						writer.Write("',");
						writer.Write((int)direction);
						writer.Write(")\">");
					} else {
						writer.Write(">");
					}
					writer.Write(g.Title);
					if (sortLink) { writer.Write("</a>"); }
					writer.Write("</th>");
				}
			}
			writer.Write("</tr>");
		}

		/// <summary>
		/// Render all data rows
		/// </summary>
		protected void RenderRows(HtmlTextWriter writer) {
			int x = 1;	

			foreach (T o in _data) {
				_columns.ReadRow(o);

				if (_columns.NeedHeader) {
					writer.Write("<tr><td colspan=\"");
					writer.Write(_columns.VisibleCount);
					writer.Write("\" class=\"groupTitle\">");
					writer.Write(_columns.Heading);
					writer.Write("</td></tr>");
					x = 1;
				}
				for (int row = 1; row <= _columns.TotalRows; row++) {
					// iterate through HTML rows used to render single data row
					writer.Write("<tr class=\"");
					writer.Write(x.SayOddEven());
					if (_cssDelegate != null) { writer.Write(_cssDelegate(o)); }
					writer.Write("\">");
					foreach (GridColumn g in _columns) {
						if (g.VisibleRow && g.Row == row) {
							writer.Write("<td");
							if (!string.IsNullOrEmpty(g.Style)) {
								writer.Write(" style=\"");
								writer.Write(g.Style);
								writer.Write("\"");
							}
							if (!string.IsNullOrEmpty(g.CssClass)) {
								writer.Write(" class=\"");
								writer.Write(g.CssClass);
								if (g.StyleDelegate != null) {
									writer.Write(g.StyleDelegate(o));
								}
								writer.Write("\"");
							}
							if (g.RowSpan > 1) {
								writer.Write(" rowspan=\"");
								writer.Write(g.RowSpan);
								writer.Write("\"");
							}
							if (g.ColumnSpan > 1) {
								writer.Write(" colspan=\"");
								writer.Write(g.ColumnSpan);
								writer.Write("\"");
							}
							writer.Write(">");
							writer.Write(g.Value);
							writer.Write("</td>");
						}
					}
					writer.Write("</tr>");
				}
				x++;
			}
		}

		#endregion

		protected void AddSorter(GridColumn c, IComparer<T> s) {
			if (_sorters == null) { _sorters = new Dictionary<string, IComparer<T>>(); }
			_sorters.Add(c.Title, s);
		}
	}
}
