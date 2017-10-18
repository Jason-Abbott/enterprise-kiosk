using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Idaho.Draw {
	/// <summary>
	/// A simple line
	/// </summary>
	public class Line : GraphicBase {

		private Point _start = new Point();
		private Point _end = new Point();
		private double _length = -1;
		private bool _doNotResize = false;

		#region Properties

		public Point Start {
			get { return _start; }
			set { this.StartX = value.X; this.StartY = value.Y; }
		}
		public Point End {
			get { return _end; }
			set { this.EndX = value.X; this.EndY = value.Y; }
		}

		/// <summary>
		/// Update coordinates to maintain the original length
		/// </summary>
		public bool DoNotResize { set { _doNotResize = value; } }

		public int StartX {
			get { return _start.X; }
			set {
				int change = value - _start.X;
				_start.X = value;
				if (_doNotResize) { _end.X += change; }
			}
		}
		public int EndX {
			get { return _end.X; }
			set {
				int change = value - _end.X;
				_end.X = value;
				if (_doNotResize) { _start.X += change; }
			}
		}
		public int StartY {
			get { return _start.Y; }
			set {
				int change = value - _start.Y;
				_start.Y = value;
				if (_doNotResize) { _end.Y += change; }
			}
		}
		public int EndY {
			get { return _end.Y; }
			set {
				int change = value - _end.Y;
				_end.Y = value;
				if (_doNotResize) { _start.Y += change; }
			}
		}

		public int Length {
			set { _length = value; }
			get {
				double width = Math.Abs(_end.X - _start.X);
				double height = Math.Abs(_end.Y - _start.Y);
				if (width == 0) {
					_length = height;
				} else if (height == 0) {
					_length = width;
				} else {
					_length = Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));
				}
				return (int)_length;
			}
		}

		#endregion

		private void UpdatePoint(int value, ref int p1, ref int p2) {
			int change = value - p1 ;
			p1 = value;
			if (_doNotResize) { p2 += change; }
		}

		public Line(Point start, Point end) {
			_start = start;
			_end = end;
			// tickle the property to update value
			int l = this.Length;
		}
		public Line(Point start, Point end, bool doNotResize) : this(start, end) {
			_doNotResize = doNotResize;
		}
		public Line() { }

		/// <summary>
		/// Create name for line graphic
		/// </summary>
		protected override string GenerateFileName() {
			StringBuilder fileName = new StringBuilder();
			fileName.AppendFormat("line_{0}-{1}to{2}-{3}_{4}.{5}",
				_start.X, _start.Y, _end.X, _end.Y,
				this.Color.Border.Name, this.Extension);
			return fileName.ToString();
		}
		/// <summary>
		/// Render the line
		/// </summary>
		internal override void Create() {
			this.Graphic.DrawLine(this.BorderPen, _start, _end);
		}
	}
}
