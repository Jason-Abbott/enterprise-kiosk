using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using color = System.Drawing.Color;

namespace Idaho.Draw {
	[Serializable]
	public class Triangle : GraphicBase {

		private Directions _direction = Directions.Right;
		public enum Directions { Up, Down, Left, Right }
		public Directions Direction { set { _direction = value; } }

		public Triangle(Directions direction) { _direction = direction; }
		public Triangle(Graphics graphic) { base.Graphic = graphic; }

		/// <summary>
		/// Create name for triangle graphic
		/// </summary>
		/// <remarks>Only supports equilateral triangles</remarks>
		protected override string GenerateFileName() {
			StringBuilder fileName = new StringBuilder();
			fileName.AppendFormat("triangle_{0}-{1}-{2}by{3}.{4}",
				_direction, this.Color.ForeGround.Name,
				this.Height, this.Width, this.Extension);
			return fileName.ToString();
		}

		/// <summary>
		/// Render the triangle
		/// </summary>
		/// <remarks>
		/// If drawing on a graphic that is larger than the triangle will be,
		/// specify top/left coordinates where triangle should appear.
		/// </remarks>
		/// <param name="x">Start X coordinate</param>
		/// <param name="y">Start Y coordinate</param>
		internal void Create(int x, int y) {
			this.SetPathFromPoints(this.BuildTrianglePoints(x, y));

			// normal image
			this.Graphic.FillPolygon(this.ForegroundBrush, this.Path.PathPoints);
			
			// reset graphic for highlighted image
			if (!this.Color.Highlight.Equals(color.Empty)) {
				this.ResetForNextImage();
				this.Graphic = null;
				this.Graphic.FillPolygon(this.HighlightBrush, this.Path.PathPoints);
			}
			//this.Graphic.DrawPolygon(this.BorderPen, this.Path.PathPoints);
		}
		internal override void Create() { this.Create(0, 0); }

		/// <summary>
		/// Compute the triangle coordinates
		/// </summary>
		/// <param name="x">Start X coordinate</param>
		/// <param name="y">Start Y coordinate</param>
		private PointF[] BuildTrianglePoints(int x, int y) {
			int top = y;
			int left = x;
			int right = left + this.Width;
			int bottom = top + this.Height;
			PointF[] vertices = new PointF[3];
			PointF topLeft = new PointF(left, top);
			PointF topRight = new PointF(right, top);
			PointF bottomRight = new PointF(right, bottom);
			PointF bottomLeft = new PointF(left, bottom);

			switch (_direction) {
				case Directions.Right:
					vertices[0] = topLeft;
					vertices[1] = new PointF(right, (top + bottom) / 2);
					vertices[2] = bottomLeft;
					break;
				case Directions.Left:
					vertices[0] = topRight;
					vertices[1] = new PointF(left, (top + bottom) / 2);
					vertices[2] = bottomRight;
					break;
				case Directions.Up:
					vertices[0] = bottomLeft;
					vertices[1] = new PointF((left + right) / 2, top);
					vertices[2] = bottomRight;
					break;
				case Directions.Down:
					vertices[0] = topLeft;
					vertices[1] = topRight;
					vertices[2] = new PointF((left + right) / 2, bottom);
					break;
			}
			return vertices;
		}
	}
}
