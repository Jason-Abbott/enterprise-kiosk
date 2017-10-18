using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;

namespace Idaho.Draw {
	public class Corner : GraphicBase {

		private bool _hasBorder = false;
		private Positions _position = 0;
		private int _shadowWidth = 0;
		private int _extendWidth = 0;
		private int _extendHeight = 0;
		private int _radius = 0;

		#region Properties

		public Positions Position { set { _position = value; } }
		public enum Positions { TopLeft = 1, TopRight, BottomRight, BottomLeft }
		public int ShadowWidth { set { _shadowWidth = value; } }
	
		/// <summary>
		/// Arc radius in pixels
		/// </summary>
		public int Radius { set { _radius = value; } get { return _radius; } }
		
		/// <summary>
		/// Amount in pixels to extend the arc horizontally
		/// </summary>
		public int ExtendWidth {
			set { _extendWidth = value;	this.Width += _extendWidth;	}
		}

		/// <summary>
		/// Amount in pixels to extend the arc vertically
		/// </summary>
		public int ExtendHeight {
			set {
				_extendHeight = value; this.Height += _extendHeight; }
		}

		#endregion

		/// <summary>
		/// Create a filename for this graphic
		/// </summary>
		/// <returns></returns>
		protected override string GenerateFileName() {
			_hasBorder = (!this.Color.ForeGround.Equals(this.Color.Border));
			StringBuilder fileName = new StringBuilder();
			
			fileName.Append("corner_");
			switch (_position) {
				case Positions.TopLeft: fileName.Append("tl-");	break;
				case Positions.TopRight: fileName.Append("tr-"); break;
				case Positions.BottomRight: fileName.Append("br-"); break;
				case Positions.BottomLeft: fileName.Append("bl-"); break;
			}
			if (!this.Transparent) {
				fileName.AppendFormat("{0},", this.Color.BackGround.Name);
			}
			if (_hasBorder) { fileName.AppendFormat("{0},", this.Color.Border.Name); }
			fileName.AppendFormat("{0}", this.Color.ForeGround.Name);
			if (_shadowWidth > 0) { fileName.AppendFormat(",{0}", this.Color.Shadow.Name); }
			fileName.AppendFormat("-{0}by{1}", this.Width, this.Height);			
			if (_extendWidth > 0) {
				fileName.AppendFormat("-{0}wider", _extendWidth);
			} else if (_extendHeight > 0) {
				fileName.AppendFormat("-{0}taller", _extendHeight);
			}
			fileName.Append(".");
			fileName.Append(this.Extension);
			return fileName.ToString();
		}

		/// <summary>
		/// Create the corner graphic
		/// </summary>
		internal override void Create() {
			Rectangle arc = new Rectangle(_shadowWidth, _shadowWidth, 0, 0);
			Rectangle box = new Rectangle(0, 0, 0, 0);

			arc.Width = (2 * _radius) - 1;
			arc.Height = arc.Width;

			// graphic box to extend the radius graphic
			if (_extendHeight > 0) {
				box.Width = arc.Width;
				box.Height = _extendHeight;
			} else if (_extendWidth > 0) {
				box.Width = _extendWidth;
				box.Height = arc.Height;
			}

			// coordinates are for upper left of a bounding rectangle for
			// a complete ellipse even if only 90° is visible
			switch (_position) {
				case Positions.TopLeft:
					if (_extendHeight > 0) {
						box.X = arc.X;
						box.Y = arc.Y + _radius;
					} else if (_extendWidth > 0) {
						box.X = arc.X + _radius;
						box.Y = arc.Y;
					}
					break;
				case Positions.TopRight:
					arc.X = -_radius;

					if (_extendHeight > 0) {
						box.Y = arc.Y + _radius;
						box.X = arc.X;
					} else if (_extendWidth > 0) {
						box.Y = _shadowWidth;
					}
					break;
				case Positions.BottomRight:
					arc.X = -_radius;
					arc.Y = -_radius;

					if (_extendHeight > 0) {
						arc.Y += box.Height;
					} else if (_extendWidth > 0) {
						arc.X += box.Width;
					}
					break;
				case Positions.BottomLeft:
					arc.Y = -_radius;

					if (_extendHeight > 0) {
						box.X = _shadowWidth;
						arc.Y += box.Height;
					} else if (_extendWidth > 0) {
						box.X = arc.X + arc.Width;
					}
					break;
			}

			if (_hasBorder) {
				int holdBack = (int)(this.BorderWidth / 3);
				arc.Y += (_position == Positions.BottomLeft ||
					_position == Positions.BottomRight) ? -holdBack : holdBack;

				arc.X += (_position == Positions.TopRight ||
					_position == Positions.BottomRight) ? -holdBack : holdBack;
			}

			// draw shadow
			if (_shadowWidth > 0) {
				GraphicsPath exclusions = new GraphicsPath();
				Rectangle r = new Rectangle();
				Rectangle clone = new Rectangle();

				clone.X = arc.X;
				clone.Y = arc.Y;
				clone.Width = arc.Width;
				clone.Height = arc.Height;
				clone.Inflate(-1, -1);

				if (_extendHeight > 0) {
					r.Y = box.Y;
					r.Width = this.Width;
					r.Height = box.Height;
				} else if (_extendWidth > 0) {
					r.X = box.X;
					r.Width = box.Width;
					r.Height = this.Height;
				}
				exclusions.AddRectangle(r);
				exclusions.AddEllipse(clone);

				this.DrawShadow(arc, Shapes.Ellipse, exclusions);
				
				// box shadow
				if (_extendHeight > 0 || _extendWidth > 0) {
					exclusions.Reset();
					r.X = 0;
					r.Y = 0;
					
					if (_extendHeight > 0) {
						r.Height = this.Height - box.Height;
						r.Y = (_position == Positions.TopLeft ||
							_position == Positions.TopRight) ? 0 : box.Height;
					} else if (_extendWidth > 0) {
						r.Width = this.Width - box.Width;
						r.X = (_position == Positions.TopLeft ||
							_position == Positions.BottomLeft) ? 0 : box.Width;
					}
					exclusions.AddRectangle(r);

					this.DrawShadow(box, Shapes.Rectangle, exclusions);
				}
			}
			// draw arc
			this.Graphic.FillEllipse(this.ForegroundBrush, arc);
			
			if (_hasBorder) {
				this.Graphic.DrawArc(this.BorderPen, arc, 0, 360);
			}
			// draw extension box
			if (_extendHeight > 0 || _extendWidth > 0) {
				this.Graphic.FillRectangle(this.ForegroundBrush, box);
			}
		}

		/// <summary>
		/// Draw a shadow for the given shape
		/// </summary>
		private void DrawShadow(Rectangle shape, Shapes type, GraphicsPath exclusions) {
			Rectangle shadow = new Rectangle();

			shadow.X = shape.X - _shadowWidth;
			shadow.Y = shape.Y - _shadowWidth;
			shadow.Width = shape.Width + (2 * _shadowWidth) + 1;
			shadow.Height = shadow.Width;

			GraphicsPath path = new GraphicsPath();

			switch (type) {
				case Shapes.Ellipse: path.AddEllipse(shadow); break;
				case Shapes.Rectangle: path.AddRectangle(shadow); break;
			}
			PathGradientBrush brush = new PathGradientBrush(path);
			brush.CenterColor = this.Color.Shadow;
			brush.SurroundColors = new Color[] {
				Draw.Utility.AdjustOpacity(this.Color.Shadow, 0) };

			if (exclusions != null) {
				this.Graphic.ExcludeClip(new Region(exclusions));
			}

			switch (type) {
				case Shapes.Ellipse: this.Graphic.FillEllipse(brush, shadow); break;
				case Shapes.Rectangle: this.Graphic.FillRectangle(brush, shadow); break;
			}
			this.Graphic.ResetClip();
		}
	}
}