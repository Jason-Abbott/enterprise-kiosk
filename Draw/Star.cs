using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;

namespace Idaho.Draw {
	public class Star : GraphicBase {

		private float _rating;
		private int _stars = 5;
		private int _points = 5;
		private double _sharpness;
		private const float _minPixelWidth = 0.3F;

		#region Properties

		public int Stars { set { _stars = value; } }
		public float Rating { set { _rating = value; } }
		public int Points { set { _points = value; } }
		public double Sharpness { set { _sharpness = value; } }

		#endregion

		public Star() { }
		public Star(int stars) { _stars = stars; }
		public Star(int stars, float rating) { _stars = stars; _rating = rating; }
		public Star(int stars, float rating, int points, double sharpness) {
			_stars = stars;
			_rating = rating;
			_points = points;
			_sharpness = sharpness;
		}

		/// <summary>
		/// Create name for star graphic
		/// </summary>
		protected override string GenerateFileName() {
			StringBuilder fileName = new StringBuilder();
			if (_stars > 1) {
				fileName.AppendFormat("rating_{0}-", _rating);
			} else {
				fileName.AppendFormat("star_{0}-{1}", _points, _sharpness);
			}
			if (!this.Transparent) {
				fileName.AppendFormat("{0},", this.Color.BackGround.Name);
			}
			fileName.AppendFormat("{0},", this.Color.Border.Name);
			fileName.AppendFormat("{0}-", this.Color.ForeGround.Name);
			fileName.Append(this.Height);
			fileName.Append(".");
			fileName.Append(this.Extension);
			return fileName.ToString();
		}

		internal override void Create() {
			this.SetPathFromPoints(this.BuildStarPoints());
			if (_stars > 1) { this.Multiple(); } else { this.Single(); }
		}

		/// <summary>
		/// Create graphic of multiple stars
		/// </summary>
		/// <remarks>May be used to represent rating compared to five stars.</remarks>
		private void Multiple() {
			float remainingRating = _rating;
			Matrix move = new Matrix(1, 0, 0, 1, this.Height, 0);

			// position and draw remaining stars
			for (int x = 0; x <= _stars; x++) {
				if (x > 0) {
					this.Path.Transform(move);
					this.GradientBrush.TranslateTransform(this.Height, 0);
				}
				if (remainingRating > 0) {
					if (remainingRating < 1) {
						// make clipping region to show partial star
						float partWidth = (float)(x + remainingRating) * this.Height + 1;
						this.Graphic.Clip = new Region(
							new RectangleF(new PointF(0, 0), new SizeF(partWidth, this.Height)));
					}
					// draw the outline
					this.Graphic.FillPolygon(this.GradientBrush, this.Path.PathPoints);
				}
				this.Graphic.ResetClip();
				this.Graphic.DrawPolygon(this.BorderPen, this.Path.PathPoints);
				remainingRating -= 1;
			}
		}

		/// <summary>
		/// Create a single star
		/// </summary>
		private void Single() {
			this.Graphic.FillPolygon(this.GradientBrush, this.Path.PathPoints);
			this.Graphic.DrawPolygon(this.BorderPen, this.Path.PathPoints);
		}

		/// <summary>
		/// Create array of points representing star
		/// </summary>
		private PointF[] BuildStarPoints() {
			double bleed = 0;

			if (this.BorderWidth > 1) {
				// need to reduce star radius to accomodate border width
				// compute based on triangle formed in unit circle (radius = 1)
				double legEdge;
				double crossMultiple;
				double sineOfPointAngle;
				double redux;

				// law of cosines
				legEdge = Math.Sqrt((Math.Pow(_sharpness, 2) + 1) - (2 * _sharpness * Math.Cos(Math.PI / _points)));
				// law of sines
				crossMultiple = _sharpness * Math.Sin(Math.PI / _points);
				sineOfPointAngle = crossMultiple / legEdge;
				// height of point formed at vertex by pen width
				bleed = ((this.BorderWidth - 1) * legEdge) / (2 * crossMultiple);

				// bleed is mathematically correct but pixel fractions don't display for
				// excessively sharp points, so reduce bleed point to minimum pixel width
				redux = (_minPixelWidth / 2) / Math.Tan(Math.Asin(sineOfPointAngle));

				bleed -= redux;
				if (bleed < 0) { bleed = 0; }
			}
			float radius = (float)((this.Height / 2) - bleed);
			float innerRadius = (float)(radius - radius * _sharpness);
			//List<PointF> vertices;
			//PointF[] vertices;
			PointF[] vertices = new PointF[_points * 2 - 1];
			float factor;
			//double cosine;
			//double sine;

			for (int x = 1; x <= vertices.Length; x++) {
				// alternating inner and outer points
				factor = (int)(x % 2 == 0 ? innerRadius : radius);
				// compute coordinates on unit circle; pi/2 radians = 90 degree start position
				vertices[x - 1].X = (float)Math.Cos((x * Math.PI / _points) + (Math.PI / 2));
				vertices[x - 1].Y = (float)Math.Sin((x * Math.PI / _points) + (Math.PI / 2));
				// multiply size to match specified radius
				vertices[x - 1].X *= factor;
				vertices[x - 1].Y *= factor;
				// reposition so star center is in center of image
				vertices[x - 1].X += (float)this.Height / 2;
				vertices[x - 1].Y += (float)this.Height / 2;
			}
			return vertices;
		}

		//Function BuildStarPoints2(ByVal points As Integer, ByVal sharpness As Double) As PointF()
		// ' from http://www.bobpowell.net/pgb.htm
		// Dim vertices(points - 1) As PointF
		// Dim inner As Boolean = False
		// Dim point As Integer = 0
		// Dim angle As Single

		// Do Until angle <= Math.PI * 2
		// Dim length As Single = 50 + (80 * CInt(IIf(inner, 0, 1)))
		// vertices(point) = New PointF(CSng(Math.Cos(angle - Math.PI / 2) * length), _
		// CSng(Math.Sin(angle - Math.PI / 2) * length))
		// angle += CSng(Math.PI / 5)
		// inner = Not inner
		// point += point
		// Loop
		// vertices(point) = vertices(0)

		// Return vertices
		//End Function
	}
}