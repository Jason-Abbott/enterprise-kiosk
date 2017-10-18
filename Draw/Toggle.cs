using System;
using System.Drawing;
using System.Text;

namespace Idaho.Draw {
	public class Toggle : GraphicBase {

		private States _state;
		private Styles _style = Styles.PlusMinus;
		public enum States { Open, Closed }
		public enum Styles { PlusMinus, Arrow }
		public States State { set { _state = value; } }
		public Styles Style { set { _style = value; } }

		/// <summary>
		/// Build filename for this graphic
		/// </summary>
		protected override string GenerateFileName() {
			StringBuilder fileName = new StringBuilder();
			fileName.Append("toggle_");
			if (!this.Transparent) {
				fileName.Append(this.Color.BackGround.Name);
				fileName.Append(",");
			}
			fileName.Append(this.Color.ForeGround.Name);
			if (this.Height != 0) { fileName.AppendFormat("-{0}", this.Height); }
			fileName.Append((_state == States.Open ? "-." : "+."));
			fileName.Append(this.Extension);
			return fileName.ToString();
		}
		internal override void Create() {
			switch (_style) {
				case Styles.PlusMinus: this.PlusMinus(); break;
				case Styles.Arrow: throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Create graphics for both on and off state of toggle image
		/// </summary>
		private void PlusMinus() {
			this.BorderWidth = 1;
			PointF point1;
			PointF point2;

			this.Graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
			point1 = new PointF(3, (float)this.Height / 2);
			point2 = new PointF(this.Height - 4, (float)this.Height / 2);

			// draw minus
			if (!this.Transparent) { this.Graphic.Clear(this.Color.BackGround); }
			this.Graphic.DrawRectangle(this.BorderPen, 0, 0, this.Height - 1, this.Height - 1);
			this.Graphic.DrawLine(this.BorderPen, point1, point2);
			this.AbsolutePath = this.AbsolutePath.Replace("+.", "-.");
			this.Save(false);

			// add vertical line to form "+"
			point1 = new PointF((float)this.Height / 2, 3);
			point2 = new PointF((float)this.Height / 2, this.Height - 4);
			this.Graphic.DrawLine(this.BorderPen, point1, point2);
			this.AbsolutePath = this.AbsolutePath.Replace("-.", "+.");
		}

		private void Arrow() {

		}
	}
}