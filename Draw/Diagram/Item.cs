using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Idaho.Draw.Diagram {
	/// <summary>
	/// Smallest diagram object
	/// </summary>
	public class Item : Entity {
		private bool _fitLabel = true;

		/// <summary>
		/// Adjust height to fit the label text
		/// </summary>
		public bool FitLabel { set { _fitLabel = true; } get { return _fitLabel; } }

		private Item(string id, Entity container) : base(id, container) { }
		public Item(string id, Entity container, Rectangle coordinates) : base(id, container, coordinates) { }
		public Item(string id, Entity container, Size size) : this(id, container, new Rectangle(new Point(0, 0), size)) { }
		public Item(string id, Entity container, int width, int height) : this(id, container, new Size(width, height)) { }
	}
}
