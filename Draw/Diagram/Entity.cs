using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Idaho.Draw.Diagram {
	/// <summary>
	/// Basic members of a diagram entity
	/// </summary>
	public abstract class Entity : IEquatable<Entity> {
		private string _id = string.Empty;
		private Rectangle _coordinates;
		private Entity _container = null;
		private string _label = string.Empty;
		private int _margin = 0;
		private int _borderWidth = 1;
		private int _labelHeight = 30;
		private int _labelLines = 1;
		private event EventHandler<EventArgs<Rectangle>> _subscribers;
		private int _layer = 0;
		private string _styleClass = string.Empty;
		private string _labelSuffix = string.Empty;
		private string _typeName = string.Empty;
		private List<Entity> _connectedTo = null;
		private List<Entity> _connectedFrom = null;
		private bool _handleEvents = false;

		internal static Single Shrinkage = 0.9f;
		internal static Single CharacterWidthRatio = 0.5f;

		public enum Orientations { Horizontal, Vertical }
		public enum Edges { Top, Right, Buttom, Left }
		public enum HorizontalAlignments { Left, Center, Right }
		public enum VerticalAlignments { Top, Middle, Bottom }

		#region Properties

		/// <summary>
		/// Should event handling be rendered for this entity
		/// </summary>
		public bool HandleEvents { get { return _handleEvents; } set { _handleEvents = value; } }

		/// <summary>
		/// Something displayed after the label, such as a footnote
		/// </summary>
		public string LabelSuffix { get { return _labelSuffix; } set { _labelSuffix = value; } }

		/// <summary>
		/// Entities this one is connected to
		/// </summary>
		internal List<Entity> ConnectedTo {
			get {
				if (_connectedTo == null) { _connectedTo = new List<Entity>(); }
				return _connectedTo;
			}
		}

		/// <summary>
		/// Entities with connections to this one
		/// </summary>
		internal List<Entity> ConnectedFrom {
			get {
				if (_connectedFrom == null) { _connectedFrom = new List<Entity>(); }
				return _connectedFrom;
			}
		}

		public string ID { get { return _id; } protected set { _id = value; } }

		/// <summary>
		/// Name for this type of entity such as "User" or "Function"
		/// </summary>
		public string TypeName { get { return _typeName; } set { _typeName = value; } }
		
		/// <summary>
		/// Identify styling that should be applied (e.g. CSS class for HTML)
		/// </summary>
		public string StyleClass { set { _styleClass = value; } get { return _styleClass; } }

		/// <summary>
		/// The height of the label in pixels
		/// </summary>
		public int LabelHeight { set { _labelHeight = value; } get { return _labelHeight; } }

		/// <summary>
		/// Lines of text in the label
		/// </summary>
		public int LabelLines { set { _labelLines = value; } get { return _labelLines; } }

		/// <summary>
		/// Font size of the label text in pixels
		/// </summary>
		public int LabelFontSize {
			get { return (int)(_labelHeight / _labelLines) - 2; }
		}

		/// <summary>
		/// Width of border in pixels (0 for none)
		/// </summary>
		public int BorderWidth { get { return _borderWidth; } set { _borderWidth = value; } }
		
		/// <summary>
		/// The layer (z-index) on which this entity should be rendered
		/// </summary>
		public int Layer { get { return _layer; } set { _layer = value; } }

		/// <summary>
		/// Raise an event when coordinates change
		/// </summary>
		public event EventHandler<EventArgs<Rectangle>> ChangeEvent {
			add { _subscribers += value; }
			remove { _subscribers -= value; }
		}

		/// <summary>
		/// Pixel space around inside of this container
		/// </summary>
		public int Margin {
			get { return (this is Surface) ? 0 : _margin; }
			set { _margin = value; }
		}
		/// <summary>
		/// Calculated margin for child entities
		/// </summary>
		protected int ChildMargin { get { return (int)(_margin * Shrinkage); } }

		/// <summary>
		/// Text to display with entity
		/// </summary>
		public string Label { get { return _label; } set { _label = value; } }

		/// <summary>
		/// Label text with HTML break tags for new lines
		/// </summary>
		public string HtmlLabel {
			get {
				if (!string.IsNullOrEmpty(_label)) {
					return _label.Replace(Environment.NewLine, "<br/>");
				} else {
					return _label;
				}
			}
		}

		/// <summary>
		/// Height in pixels
		/// </summary>
		public int Height {
			get { return _coordinates.Height; }
			set { _coordinates.Height = value; this.Changed(); }
		}
		/// <summary>
		/// Width in pixels (updates label height if set)
		/// </summary>
		public int Width {
			get { return _coordinates.Width; }
			set {
				_coordinates.Width = value;
				this.UpdateLabelHeight();
				this.Changed();
			}
		}
		public int Bottom { get { return _coordinates.Bottom; } }
		public int Top { get { return _coordinates.Top; } set { _coordinates.Y = value; } }
		public int Left { get { return _coordinates.Left; } set { _coordinates.X = value; } }
		public int Right { get { return _coordinates.Right; } }

		public Point TopLeft {
			get { return _coordinates.Location; } set { _coordinates.Location = value; }
		}
		public Point BottomRight {
			get { return new Point(this.Left + this.Width, this.Top + this.Height);	}
			set {
				_coordinates.Width = value.X - this.Left;
				_coordinates.Height = value.Y - this.Top;
			}
		}

		/// <summary>
		/// Coordinates relative to the containing entity
		/// </summary>
		private Rectangle Coordinates { get { return _coordinates; } }
		public Entity Container { get { return _container; } }

		/// <summary>
		/// Cordinates relative to the diagram surface
		/// </summary>
		public Rectangle SurfaceCoordinates {
			get {
				int x = _coordinates.X + _borderWidth;
				int y = _coordinates.Y + _borderWidth;
				int max = 20;
				int count = 0;	// safety counter
				Entity container = this.Container;
				
				// surface container is null so loop until that level is reached
				while (count < max && container != null) {
					x += container.Coordinates.X + _borderWidth;
					y += container.Coordinates.Y;
					container = container.Container;
					count++;
				}
				return new Rectangle(x, y, this.Width, this.Height);
			}
		}

		#endregion

		protected Entity(string id, Entity container) : this(id, container, new Rectangle()) { }
		protected Entity(string id, Entity container, Rectangle coordinates) {
			_id = id;
			_container = container;
			_coordinates = coordinates;
		}

		#region Events

		/// <summary>
		/// Called when the coordinates change
		/// </summary>
		private void Changed() {
			if (_subscribers != null) {
				_subscribers(this, new EventArgs<Rectangle>(_coordinates, EventType.NewValue));
			}
		}

		#endregion

		/// <summary>
		/// Compute mid-point relative to surface for given edge
		/// </summary>
		public Point MidPoint(Edges edge) {
			Rectangle c = this.SurfaceCoordinates;
			int halfHeight = (int)((c.Height - (_borderWidth * 2)) / 2);
			int halfWidth = (int)((c.Width - (_borderWidth * 2)) / 2);

			switch (edge) {
				case Edges.Buttom:
					return new Point(c.Left + halfWidth, c.Bottom);
				case Edges.Left:
					return new Point(c.Left, c.Top + halfHeight);
				case Edges.Right:
					return new Point(c.Right, c.Top + halfHeight);
				case Edges.Top:
					return new Point(c.Left + halfWidth, c.Top);
				default:
					// the middle of the entity
					return new Point(c.Left + halfWidth, c.Top + halfWidth);
			}
		}

		/// <summary>
		/// Update height of label container to fit text
		/// </summary>
		internal void UpdateLabelHeight() {
			if (!string.IsNullOrEmpty(_label) && this.Width > 0) {
				int availableWidth = this.Width;

				// panels have extra padding around their label
				if (this is Panel) { availableWidth -= 20; }

				// maximum characters that can fit for font size and width ratio
				int maxCharacters = (int)(availableWidth / (CharacterWidthRatio * this.LabelFontSize));
				
				if (_label.Length > maxCharacters) {
					int lines = 1;
					_label = Format.Wrap(_label, maxCharacters, out lines);
					_labelLines = lines;
					_labelHeight *= lines;
					if (this is Item && ((Item)this).FitLabel) {
						this.Height = _labelHeight;
					}
				}
			}
		}

		/// <summary>
		/// Create a connection to the target item
		/// </summary>
		public void ConnectTo(Item target) {
			int count = 0;
			int limit = 20;
			Entity container = this.Container;

			while (count < limit && !(container is Surface)) {
				container = container.Container;
			}
			if (!(container is Surface)) {
				throw new System.Exception("Unable to find diagram surface for \"" + this.Label + "\" item");
			}
			((Surface)container).AddConnection(this, target);
		}

		public bool Equals(Entity other) {
			return _id == other.ID
				&& ((_container == null && other.Container == null)
				|| (_container != null && other.Container != null
				&& _container.Equals(other.Container)));
		}

		/// <summary>
		/// Build an ID string list from the entity collection
		/// </summary>
		/// <param name="format">String format for entity ID</param>
		internal static string IdList(List<Entity> list, char delimiter, string format) {
			string output = string.Empty;

			if (list != null) {
				StringBuilder sb = new StringBuilder();
				foreach (Entity e in list) {
					sb.AppendFormat(format, e.ID);
					sb.Append(delimiter);
				}
				output = sb.ToString();
				if (output.Length > 0) { output = output.TrimEnd(new char[] { delimiter }); }
			}
			return output;
		}
		internal static string IdList(List<Entity> list) { return IdList(list, ',', "'{0}'"); }
	}
}
