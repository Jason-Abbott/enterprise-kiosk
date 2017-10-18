using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Web.UI;

namespace Idaho.Web.Controls {
	public class Line : Image {

		private Point _start = new Point(0, 0);
		private Point _end = new Point(0, 0);
		private int _width = 1;
		private Color _color;
		private int _alpha = 100;
		private bool _preRendered = false;

		#region Properties

		public Point Start { set { _start = value; } }
		public Point End { set { _end = value; } }
		public int StartX { set { _start.X = value; } }
		public int StartY { set { _start.Y = value; } }
		public int EndX { set { _end.X = value; } }
		public int EndY { set { _end.Y = value; } }

		/// <summary>
		/// Line width (total graphic width is computed)
		/// </summary>
		public new int Width { set { _width = value; } }
		public string Color { set { _color = ColorTranslator.FromHtml(value); } }
		public int Alpha { set { _alpha = value; } }

		#endregion

		/// <summary>
		/// Generate image tag for line
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			base.Width = Math.Abs(_end.X - _start.X);
			base.Height = Math.Abs(_end.Y - _start.Y);

			if (_alpha < 100) {
				_color = Draw.Utility.AdjustOpacity(_color, _alpha);
				this.Transparency = true;
			}
			string cacheKey = string.Format("line{0}{1}{2}",
				base.Width, base.Height, _color.ToArgb());

			if (!this.TagInCache(cacheKey)) {
				// move coordinates to origin (0,0)
				int lessX = (_start.X < _end.X) ? _start.X : _end.X;
				int lessY = (_start.Y < _end.Y) ? _start.Y : _end.Y;
				_start.X -= lessX;
				_end.X -= lessX;
				_start.Y -= lessY;
				_end.Y -= lessY;

				Idaho.Draw.Line draw = new Idaho.Draw.Line(_start, _end);

				draw.Width = base.Width;
				draw.Height = base.Height;
				// use border pen to draw line
				draw.BorderWidth = _width;
				draw.Color.Border = _color;
				this.Src = draw.Url;
			}
			// attributes that aren't cached
			this.AlternateText = "";
			this.Generated = true;
			_preRendered = true;
			base.OnPreRender(e);
		}

		public override void RenderControl(HtmlTextWriter writer) {
			if (!_preRendered) { this.OnPreRender(new EventArgs()); }
			base.RenderControl(writer);
		}
	}
}
