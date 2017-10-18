using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Idaho.Draw.Diagram {
	/// <summary>
	/// Main drawing surface
	/// </summary>
	public class Surface : Panel {
		private List<Connector> _connectors = null;
		private Connector.Styles _lineStyle = Connector.Styles.Direct;

		#region Properites

		/// <summary>
		/// Style of connector lines
		/// </summary>
		public Connector.Styles LineStyle { set { _lineStyle = value; } }

		public List<Entity> this[string id] {
			get {
				List<Entity> matches = new List<Entity>();
				if (!string.IsNullOrEmpty(id)) {
					foreach (Entity e in this.Descendants) {
						if (e.ID.Equals(id)) { matches.Add(e); }
					}
				}
				return matches;
			}
		}
		public List<Connector> Connectors { get { return _connectors; } }
		

		#endregion

		public Surface(Size size) : base(string.Empty, null, new Rectangle(new Point(0, 0), size)) { }
		public Surface(int width, int height) : this(new Size(width, height)) { }

		/// <summary>
		/// Does this surface already have the given connection
		/// </summary>
		internal bool HasConnection(Connector c) {
			if (_connectors != null && c != null) {
				foreach (Connector r in _connectors) {
					if (r.Equals(c)) { return true; }
				}
			}
			return false;
		}

		/// <summary>
		/// Add a connection (line) between two entities
		/// </summary>
		public void AddConnection(Entity from, Entity to) {
			if (from != null && to != null) {
				Connector c = new Connector(this, from, to);
				if (!this.HasConnection(c)) {
					c.LineStyle = _lineStyle;
					if (_connectors == null) { _connectors = new List<Connector>(); }
					_connectors.Add(c);
				}
			}
		}
		
		/// <summary>
		/// Add a connection (line) between the two entities with the given labels
		/// </summary>
		public void AddConnection(string fromID, string toID) {
			List<Entity> from = this[fromID];
			List<Entity> to = this[toID];

			foreach (Entity f in from) {
				foreach (Entity t in to) { this.AddConnection(f, t); }
			}
		}
		public void AddConnection(string fromID, int toID) {
			this.AddConnection(fromID, toID.ToString());
		}
		public void AddConnection(string fromID, Guid toID) {
			this.AddConnection(fromID, toID.ToString());
		}
		public void AddConnection(int fromID, int toID) {
			this.AddConnection(fromID.ToString(), toID.ToString());
		}
		public void AddConnection(int fromID, string toID) {
			this.AddConnection(fromID.ToString(), toID);
		}
		public void AddConnection(int fromID, Guid toID) {
			this.AddConnection(fromID.ToString(), toID.ToString());
		}
		public void AddConnection(Guid fromID, Guid toID) {
			this.AddConnection(fromID.ToString(), toID.ToString());
		}
		public void AddConnection(Guid fromID, string toID) {
			this.AddConnection(fromID.ToString(), toID);
		}
		public void AddConnection(Guid fromID, int toID) {
			this.AddConnection(fromID.ToString(), toID.ToString());
		}

		#region Events

		public void PreRender() {
			this.CalculateWidths();
			this.CalculateHeights();
			this.CalculatePositions();

			if (_connectors != null) {
				foreach (Connector c in _connectors) { c.CalculateCoordinates(); }
			}
		}

		#endregion

	}
}
