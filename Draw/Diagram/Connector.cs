using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Idaho.Draw.Diagram {
	/// <summary>
	/// Connection (line) between two items
	/// </summary>
	public class Connector : Entity, IEquatable<Connector> {
		private Entity _from = null;
		private Entity _to = null;
		private Styles _style = Styles.Direct;
		private EndStyles _fromStyle = EndStyles.None;
		private EndStyles _toStyle = EndStyles.None;
		private Edges _fromEdge = Edges.Right;
		private Edges _toEdge = Edges.Left;
		private Point _fromPoint = new Point(0, 0);
		private Point _toPoint = new Point(0, 0);

		public enum Trends { Downward, Upward }
		public enum EndStyles { None, Arrow, Circle }
		public enum Styles {
			/// <summary>
			/// A combination of orthoginal lines to make the connection
			/// </summary>
			Orthoganol,
			/// <summary>
			/// A single straight line, usually at an angle
			/// </summary>
			Direct
		}

		#region Properties

		private Entity From { get { return _from; } }
		private Entity To { get { return _to; } }

		public Point FromPoint { get { return _fromPoint; } }
		public Point ToPoint { get { return _toPoint; } }

		/// <summary>
		/// How does the line trend from left-to-right (upward or downward)
		/// </summary>
		/// <remarks>
		/// Used to determine appropriate rendering for orthogonal style.
		/// </remarks>
		public Trends Trend {
			get {
				if (_fromPoint.X < _toPoint.X) {
					return (_fromPoint.Y < _toPoint.Y) ? Trends.Downward : Trends.Upward;
				} else {
					return (_fromPoint.Y < _toPoint.Y) ? Trends.Upward : Trends.Downward;
				}
			}
		}

		/// <summary>
		/// Style (orthogonal, direct, etc.) of the line itself
		/// </summary>
		public Styles LineStyle { set { _style = value; } get { return _style; } }

		/// <summary>
		/// Style (arrow, circle, etc.) at the source of the line
		/// </summary>
		public EndStyles FromStyle { set { _fromStyle = value; } }

		/// <summary>
		/// Style (arrow, circle, etc.) at the end of the line
		/// </summary>
		public EndStyles ToStyle { set { _toStyle = value; } }

		#endregion

		internal void CalculateCoordinates() {
			Rectangle fromRect = _from.SurfaceCoordinates;
			Rectangle toRect = _to.SurfaceCoordinates;

			// compute best connection edges
			if (fromRect.Right < toRect.Left) {
				// source is completely left of target
				_toEdge = Edges.Left;
				_fromEdge = Edges.Right;
			} else if (toRect.Right < fromRect.Left) {
				// source is completely right of target
				_toEdge = Edges.Right;
				_fromEdge = Edges.Left;
			} else if (fromRect.Bottom < toRect.Top) {
				// source is completely above target
				_toEdge = Edges.Top;
			}
			_fromPoint = _from.MidPoint(_fromEdge);
			_toPoint = _to.MidPoint(_toEdge);

			if (_fromPoint.Y < _toPoint.Y) {
				this.Top = _fromPoint.Y;
				this.Height = _toPoint.Y - _fromPoint.Y;
			} else {
				this.Top = _toPoint.Y;
				this.Height = _fromPoint.Y - _toPoint.Y;
			}
			if (_fromPoint.X < _toPoint.X) {
				this.Left = _fromPoint.X;
				this.Width = _toPoint.X - _fromPoint.X;
			} else {
				this.Left = _toPoint.X;
				this.Width = _fromPoint.X - _toPoint.X;
			}
		}

		public Connector(Surface surface, Entity from, Entity to) : base(string.Empty, surface) {
			Assert.NoNull(from, "NullDiagramEntity");
			Assert.NoNull(to, "NullDiagramEntity");
			_from = from;
			_to = to;

			this.ID = _from.ID + "-to-" + _to.ID;

			if (!_to.ConnectedFrom.Contains(_from)) { _to.ConnectedFrom.Add(_from); }
			if (!_from.ConnectedTo.Contains(_to)) { _from.ConnectedTo.Add(_to); }

			_from.HandleEvents = true;
			_to.HandleEvents = true;

			this.BorderWidth = 0;
		}

		public bool Equals(Connector other) {
			return _from.Equals(other.From) && _to.Equals(other.To);
		}
	}
}
