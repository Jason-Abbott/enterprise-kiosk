using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Web;
using System.Web.UI;
using web = System.Web.UI.WebControls;

namespace Idaho.Draw {
	public class TextGraphic : GraphicBase, ITextDrawer {

		private string _text;
		private float _rotate = 0;
		private string _fontName = "Tahoma";
		private int _size = 10;	// pixel height
		private TextRenderingHint _antiAlias = TextRenderingHint.AntiAlias;
		private System.Drawing.FontStyle _fontStyle = System.Drawing.FontStyle.Regular;
		private Font _textFont = null;
		private Brush _textBrush = null;

		#region Properties

		public FontStyle FontStyle { set { _fontStyle = value; } }
		public TextRenderingHint AntiAlias { set { _antiAlias = value; } }
		public string Text { set { _text = value; } }
		public string FontName { set { _fontName = value; } }
		
		/// <summary>
		/// Number of degrees to rotate the rendered text
		/// </summary>
		public float Rotate { set { _rotate = value; } }
		public new int Size { set { _size = value; } }

		public Font TextFont {
			get {
				if (_textFont == null) {
					_textFont = new Font(_fontName, _size,
						_fontStyle, GraphicsUnit.Pixel);
				}
				return _textFont;
			}
		}
		public Brush TextBrush {
			get {
				if (_textBrush == null) { _textBrush = new SolidBrush(this.Color.Text); }
				return _textBrush;
			}
		}

		#endregion

		/// <summary>
		/// Draw basic text
		/// </summary>
		internal override void Create() {
			this.AdjustSizeForText();
			this.Graphic.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0));
			if (_rotate != 0) {
				this.Graphic.TranslateTransform(0, this.Height);
				this.Graphic.RotateTransform(_rotate);
				//this.Graphic.TranslateTransform(this.Width/2, -this.Height);
			}
			this.Graphic.DrawString(_text, this.TextFont, this.TextBrush, 0, 0);
			this.Graphic.ResetTransform();
		}
		public void Generate() { this.Create(); }

		/// <summary>
		/// Create file name for generated graphic
		/// </summary>
		/// <returns></returns>
		protected override string GenerateFileName() {
			StringBuilder fileName = new StringBuilder();
			fileName.AppendFormat("text_{0}", _text.Replace(" ", ""));
			fileName.AppendFormat("_{0}", _fontName.Replace(" ", ""));
			fileName.AppendFormat("-{0}", _fontStyle);
			fileName.AppendFormat("_{0}", _size);
			if (_rotate != 0) { fileName.AppendFormat("_{0}", _rotate); }
			fileName.AppendFormat("_{0}", this.Color.Text.Name);
			fileName.Append(".");
			fileName.Append(this.Extension);
			return fileName.ToString();
		}

		/// <summary>
		/// Compute graphic size based on requested text
		/// </summary>
		public void AdjustSizeForText() {
			Bitmap button = new Bitmap(1, 1);
			Graphics graphic = Graphics.FromImage(button);
			Size stringSize;

			graphic.TextRenderingHint = _antiAlias;
			stringSize = graphic.MeasureString(_text, this.TextFont).ToSize();

			if (_rotate != 0) {
				int height = stringSize.Width;
				int width = stringSize.Height;

				if (90 % _rotate != 0) {
					// non-orthogonal rotation
					height = (int)Math.Abs(stringSize.Width * Math.Sin(_rotate));
					height += (int)Math.Abs(stringSize.Height * Math.Cos(_rotate));
					width = (int)Math.Abs(stringSize.Width * Math.Cos(_rotate));
					width += (int)Math.Abs(stringSize.Height * Math.Sin(_rotate));
				}
				this.Width = width;
				this.Height = height;
			} else {
				this.Width = stringSize.Width;
				this.Height = stringSize.Height;
			}
			this.Graphic.TextRenderingHint = _antiAlias;
		}
	}
}