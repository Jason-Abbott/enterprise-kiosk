using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Idaho.Draw.Diagram {
	public class Panel : Entity {
		private List<Panel> _panels = null;
		private List<Item> _items = null;
		private List<Entity> _children = null;
		private List<Entity> _descendants = null;
		private int _gutter = 0;
		private int _shrink = 0;
		private Orientations _orientation = Orientations.Horizontal;
		private bool _equalizeWidth = true;
		private bool _equalizeHeight = false;
		private bool _overrideSizeToEqualize = false;
		private HorizontalAlignments _horizontalAlignment = HorizontalAlignments.Left;
		private VerticalAlignments _verticalAlignment = VerticalAlignments.Top;
		private string _childTypeName = string.Empty;

		#region Properties

		public List<Panel> Panels { get { return _panels; } }
		public List<Item> Items { get { return _items; } }

		/// <summary>
		/// The amount by which panels should be shrunk relative to items
		/// </summary>
		/// <remarks>
		/// Some rendering systems, like HTML, add extra size to the element used to depict
		/// a panel. To equalize, it must be assigned a slightly shrunk width.
		/// </remarks>
		[Obsolete()]
		public int Shrink { set { _shrink = value; } }

		/// <summary>
		/// Type name that should be applied to all child entities
		/// </summary>
		public string ChildTypeName { set { _childTypeName = value; } }

		/// <summary>
		/// If width or height equalization is specified, should existing entity dimensions
		/// be overriden
		/// </summary>
		public bool OverrideSizeToEqualize {
			set { _overrideSizeToEqualize = value; } get { return _overrideSizeToEqualize; }
		}

		/// <summary>
		/// Vertical alignment of chld items
		/// </summary>
		public VerticalAlignments VerticalAlignment {
			get { return _verticalAlignment; }
			set { _verticalAlignment = value; }
		}
		/// <summary>
		/// Horizontal alignment of child items
		/// </summary>
		public HorizontalAlignments HorizontalAlignment {
			get { return _horizontalAlignment; }
			set { _horizontalAlignment = value; }
		}

		/// <summary>
		/// Child entities
		/// </summary>
		public List<Entity> Children {
			get {
				if (_children == null) {
					_children = new List<Entity>();
					if (_panels != null) { foreach (Panel p in _panels) { _children.Add(p); } }
					if (_items != null) { foreach (Item i in _items) { _children.Add(i); } }
				}
				return _children;
			}
		}
		/// <summary>
		/// Child entites and all their children
		/// </summary>
		internal List<Entity> Descendants {
			get {
				if (_descendants == null) {
					_descendants = new List<Entity>();
					if (_panels != null) {
						foreach (Panel p in _panels) {
							if (!_descendants.Contains(p)) { _descendants.Add(p); }
							foreach (Entity e in p.Descendants) {
								if (!_descendants.Contains(e)) { _descendants.Add(e); }
							}
						}
					}
					if (_items != null) {
						foreach (Item i in _items) { if (!_descendants.Contains(i)) { _descendants.Add(i); } }
					}
				}
				return _descendants;
			}
		}

		/// <summary>
		/// Pixel space between child items
		/// </summary>
		public int Gutter { get { return _gutter; } set { _gutter = value; } }

		/// <summary>
		/// Arrangement of child items
		/// </summary>
		public Orientations Orientation { set { _orientation = value; } get { return _orientation; } }
		
		public new int Width {
			get { return base.Width; }
			set {
				base.Width = value;

				if (base.Width > 0) {
					if (_orientation == Orientations.Vertical && _equalizeWidth) {
						// when width is updated, set all child items to match
						int childWidth = this.Width - (this.Margin * 2);
						if (_panels != null) {
							foreach (Panel p in _panels) { p.Width = childWidth - _shrink; }
						}
						if (_items != null) {
							foreach (Item i in _items) { i.Width = childWidth; }
						}
					}
				}
			}
		}

		/// <summary>
		/// Should all child items be made equal in height to the tallest
		/// </summary>
		public bool EqualizeHeight { set { _equalizeHeight = value; } get { return _equalizeHeight; } }

		/// <summary>
		/// Should all child items be made equal in width to the widest
		/// </summary>
		public bool EqualizeWidth { set { _equalizeWidth = value; } get { return _equalizeWidth; } }

		#endregion

		private Panel(string id, Entity container) : base(id, container) { }
		public Panel(string id, Entity container, Rectangle coordinates)
			: base(id, container, coordinates) { }
		public Panel(string id, Entity container, Orientations orientation) : base(id, container) {
			_orientation = orientation;
		}

		#region Size and Position

		/// <summary>
		/// Calculate width for this and all child entities
		/// </summary>
		internal void CalculateWidths() {
			int width = 0;
			int totalChildWidth = 0;
			int childrenWithWidth = 0;
			int childCount = this.Children.Count;
			int totalMargin = 2 * this.Margin;
			int totalGutter = (childCount - 1) * _gutter;

			// all descendant widths should be calculated first
			if (_panels != null) { foreach (Panel p in _panels) { p.CalculateWidths(); } }

			// compute total width for subsequent calculations
			foreach (Entity e in this.Children) {
				if (_orientation == Orientations.Vertical) {
					// set total width to greatest child width if vertical
					if (e.Width > width) { width = e.Width; }
				} else {
					// width is sum of child widths
					width += e.Width;
				}
				if (e.Width > 0) { childrenWithWidth++; totalChildWidth += e.Width; }
			}

			// compute child width
			if (_equalizeWidth) {
				if (this.Width > 0) {
					// equalize within parent width
					int availableWidth = this.Width - (totalMargin + totalGutter);

					if (_orientation == Orientations.Horizontal) {
						// horizontal: divide width among children
						if (_overrideSizeToEqualize) {
							// set to zero to allow recomputation
							totalChildWidth = 0;
						} else {
							// do not change child width if already specified
							if (childCount == childrenWithWidth) {
								// if all children have a width then adjust gutter
								totalGutter = this.Width - (totalChildWidth + totalMargin);
								if (childCount > 1) {
									_gutter = (int)(totalGutter / (childCount - 1));
								} else {
									_gutter = totalGutter;
								}
							} else {
								// adjust children without width to fill available space
								availableWidth -= totalChildWidth;
								childCount -= childrenWithWidth;
							}
						}
						width = (int)(availableWidth / childCount);
					} else {
						// vertical: set child width to fit within total available
						width = availableWidth;
					}
				}
				if (width > 0) {
					// treat panels separately from items because panel width cascades
					if (_panels != null) {
						foreach (Panel p in _panels) {
							if (_overrideSizeToEqualize || p.Width <= 0) { p.Width = width - _shrink; }
						}
					}
					if (_items != null) {
						foreach (Item i in _items) {
							if (_overrideSizeToEqualize || i.Width <= 0) { i.Width = width; }
						}
					}
				}
			} else {
				// if not equalizing, child width is already summarized in width variable
				totalChildWidth = 0;
			}
			if (this.Width <= 0 && width > 0) {
				this.Width = width + totalChildWidth + totalMargin + totalGutter;
			}
		}

		/// <summary>
		/// Calculate height for this and all child entities
		/// </summary>
		internal void CalculateHeights() {
			int height = 0;
			int totalChildHeight = 0;
			int childrenWithHeight = 0;
			int childCount = this.Children.Count;
			int totalMargin = 2 * this.Margin;
			int totalGutter = (childCount - 1) * _gutter;

			if (this.Height <= 0 && childCount == 0) { this.Height = this.LabelHeight + 10; return; }

			// all descendant heights should be calculated first
			if (_panels != null) { foreach (Panel p in _panels) { p.CalculateHeights(); } }

			// compute total height for subsequent calculations
			foreach (Entity e in this.Children) {
				if (_orientation == Orientations.Vertical) {
					// height is sum of child height
					height += e.Height;
				} else {
					// set total height to greatest child height if horizontal
					if (e.Height > height) { height = e.Height; }
				}
				if (e.Height > 0) { childrenWithHeight++; totalChildHeight += e.Height; }
			}

			// compute child height
			if (_equalizeHeight) {
				if (this.Height > 0) {
					// equalize within parent height
					int totalHeight = ((this.Height - totalMargin) - totalGutter) - this.LabelHeight;

					if (_orientation == Orientations.Vertical) {
						// divide child heights to fit
						if (!_overrideSizeToEqualize) {
							// do not change child height if already specified
							totalHeight -= totalChildHeight;
							childCount -= childrenWithHeight;
						} else {
							totalChildHeight = 0;
						}
						height = (int)(totalHeight / childCount);
					} else {
						// set child heights to fill available height
						height = totalHeight;
					}
				}
				if (height > 0) {
					foreach (Entity e in this.Children) {
						if (_overrideSizeToEqualize || e.Height <= 0) { e.Height = height; }
					}
				}
			} else {
				totalChildHeight = 0;
			}
			if (this.Height <= 0 && height > 0) {
				this.Height = height + totalMargin + totalGutter + this.LabelHeight;
			}
		}

		/// <summary>
		/// Calculate position (X,Y) for this and all child entities
		/// </summary>
		internal void CalculatePositions() {
			// all descendant positions should be calculated first
			if (_panels != null) { foreach (Panel p in _panels) { p.CalculatePositions(); } }

			Point topLeft = new Point(this.Margin, this.Margin + this.LabelHeight);
			foreach (Entity e in this.Children) {
				e.Left = topLeft.X;
				e.Top = topLeft.Y;
				if (_orientation == Orientations.Vertical) {
					topLeft.Y += e.Height + _gutter;
				} else {
					topLeft.X += e.Width + _gutter;
				}
			}
		}

		#endregion

		/// <summary>
		/// Create a child panel
		/// </summary>
		/// <param name="orientation">Orientation of items within the created panel</param>
		public Panel CreatePanel(string id, string label, Orientations orientation) {
			Assert.NoNull(label, "NullDiagramItemLabel");
			if (_panels == null) { _panels = new List<Panel>(); }
			Panel p = new Panel(id, this, orientation);
			p.Label = label;
			p.TypeName = _childTypeName;
			p.Margin = this.ChildMargin;
			p.LabelHeight = (int)(this.LabelHeight * Shrinkage);
			p.Gutter = _gutter;
			_panels.Add(p);
			_children = null;
			return p;
		}
		public Panel CreatePanel(Guid id, string label, Orientations orientation) {
			return this.CreatePanel(id.ToString(), label, orientation);
		}
		public Panel CreatePanel(int id, string label, Orientations orientation) {
			return this.CreatePanel(id.ToString(), label, orientation);
		}
		public Panel CreatePanel(string label, Orientations orientation) {
			return this.CreatePanel(string.Empty, label, orientation);
		}
		/// <summary>
		/// Create a child item
		/// </summary>
		public void AddItem(string id, string label, string labelSuffix, int height, bool fitLabel, string style) {
			Assert.NoNull(label, "NullDiagramItemLabel");
			Assert.NoNull(height, "NullDiagramItemHeight");
			if (_items == null) { _items = new List<Item>(); }
			Item i = new Item(id, this, 0, height);
			i.Label = label;
			i.LabelSuffix = labelSuffix;
			i.TypeName = _childTypeName;
			i.StyleClass = style;
			i.FitLabel = fitLabel;
			i.LabelHeight = (int)(this.LabelHeight * Shrinkage);
			_items.Add(i);
			_children = null;
		}
		public void AddItem(string label, int height) {
			this.AddItem(string.Empty, label, height);
		}
		public void AddItem(string id, string label, int height) {
			this.AddItem(string.Empty, label, string.Empty, height, false, string.Empty);
		}
		public void AddItem(string label, int height, string style) {
			this.AddItem(string.Empty, label, string.Empty, height, false, style);
		}
		public void AddItem(string label) {
			this.AddItem(label, string.Empty);
		}
		public void AddItem(string label, string style) {
			this.AddItem(string.Empty, label, style);
		}
		public void AddItem(string id, string label, string style) {
			this.AddItem(id, label, string.Empty, style);
		}
		public void AddItem(Guid id, string label, string style) {
			this.AddItem(id.ToString(), label, style);
		}
		public void AddItem(int id, string label, string style) {
			this.AddItem(id.ToString(), label, style);
		}
		public void AddItem(string id, string label, string suffix, string style) {
			int height = (int)(this.LabelHeight * Shrinkage);
			this.AddItem(id, label, suffix, height, true, style);
		}
	}
}
