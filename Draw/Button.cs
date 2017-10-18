using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Web;

namespace Idaho.Draw {
	public class Button : GraphicBase, ITextDrawer {

		private string _textureFile = string.Empty;
		private bool _shade = true;
		private Size _contentSize = Size.Empty;		// size of arrow and text
		private int _borderWidth = 1;
		private int _highlightWidth = 2;
		private Image _image;
		private Styles _style;
		// text
		private string _text = string.Empty;
		private Size _textSize = Size.Empty;		// size of rendered text
		private int _textLeft = 0;
		private Font _textFont;
		private string _fontName = string.Empty;
		private Brush _textBrush;
		private Brush _activeTextBrush;
		private System.Drawing.FontStyle _fontStyle = System.Drawing.FontStyle.Bold;
		private System.Drawing.Text.TextRenderingHint _antiAlias =
			System.Drawing.Text.TextRenderingHint.AntiAlias;
		private int _textHeightPadding = -1;
		private int _textWidthPadding = -1;
		// arrow
		private Size _arrowSize = Size.Empty;		// size of arrow, if any
		private ShowArrow _arrow = ShowArrow.None;
		private const int _arrowEdgePadding = 8;
		private Pair<int> _arrowLeftEdge = new Pair<int>(0, 0);
		// template
		private int _templateEndPadding = -1;
		private string _templateFile = string.Empty;

		[Flags]
		public enum ShowArrow {
			RightSide = Right | Up | Down,
			LeftOrRight = Left | Right,
			None = 0,
			Right = 1,
			Left = 2,
			Up = 4,
			Down = 8
		}

		[Flags]
		public enum Styles {
			Graphic = Rounded | Pattern | Template,
			Rounded = 1,
			Pattern = 2,
			Template = 4,
			Html = 8
		}

		#region Properties

		public ShowArrow Arrow { set { _arrow = value; } }
		public FontStyle FontStyle { set { _fontStyle = value; } }
		public Styles Style { set { _style = value; } }
		public string Text { set { _text = value; } }
		public string FontName { set { _fontName = value; } }

		/// <summary>
		/// Pixel space to leave above and below text
		/// </summary>
		public int TextHeightPadding { set { _textHeightPadding = value; } }

		/// <summary>
		/// Pixel space to leave left and right of text
		/// </summary>
		public int TextWidthPadding { set { _textWidthPadding = value; } }

		/// <summary>
		/// Pixels length to clip off each end of the template graphic
		/// </summary>
		public int TemplateEndPadding { set { _templateEndPadding = value; } }

		/// <summary>
		/// Image file to be repeated across button as a texture
		/// </summary>
		public string TextureFile {
			set { _textureFile = value; }
			internal get { return _textureFile; }
		}
		/// <summary>
		/// Image file to be used as basis for button
		/// </summary>
		public string TemplateFile {
			set { _templateFile = value; }
			internal get { return _templateFile; }
		}

		public Font TextFont {
			get {
				if (_textFont == null) {
					_textFont = new Font(_fontName, this.Height - _textHeightPadding,
						_fontStyle, GraphicsUnit.Pixel);
				}
				return _textFont;
			}
		}
		public Brush TextBrush {
			get {
				if (_textBrush == null) { _textBrush = new SolidBrush(this.Color.Text);	}
				return _textBrush;
			}
		}

		/// <summary>
		/// Style for text that is selected or the mouse is over
		/// </summary>
		public Brush ActiveTextBrush {
			get {
				if (_activeTextBrush == null) {
					_activeTextBrush = new SolidBrush(this.Color.ActiveText);
				}
				return _activeTextBrush;
			}
		}

		#endregion

		#region Constructors

		public Button() { }
		public Button(string text) { _text = text; }
		public Button(string text, Styles style) { _text = text; _style = style; }

		#endregion

		/// <summary>
		/// Get file name for this graphic
		/// </summary>
		protected override string GenerateFileName() {
			StringBuilder fileName = new StringBuilder();
			string type = string.Empty;
			if (string.IsNullOrEmpty(_templateFile)) {
				type = "default";
			} else {
				int slash = _templateFile.LastIndexOf('\\') + 1;
				type = _templateFile.Substring(slash, _templateFile.LastIndexOf('.') - slash);
			}
			fileName.AppendFormat("btn_{0}_{1}_{2}",
				Data.File.SafeFileName(_text), _arrow, Data.File.SafeFileName(type));
			if (this.Width != 0) { fileName.AppendFormat("-{0}w", this.Width); }
			fileName.Append(".");
			fileName.Append(this.Extension);
			return fileName.ToString();
		}

		internal override void Create() {
			switch (_style) {
				case Styles.Pattern:
					Assert.NoNull(this.Height, "NullButtonHeight");
					Assert.NoNull(_textureFile, "NullButtonTextureFile");
					_antiAlias = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
					_fontStyle = System.Drawing.FontStyle.Regular;
					_image = System.Drawing.Image.FromFile(_textureFile);
					_textHeightPadding = 3;
					_textWidthPadding = 6;
					this.AdjustSizeForText();
					this.CreateFromPattern();
					break;

				case Styles.Rounded:
					_textHeightPadding = 2;
					_textWidthPadding = -4;
					this.AdjustSizeForText();
					this.CreateRounded();
					break;

				case Styles.Template:
					Assert.NoNull(_templateFile, "NullButtonTemplateFile");
					_image = Image.FromFile(_templateFile);
					this.Height = _image.Height;
					this.AdjustSizeForText();
					this.CreateFromTemplate();
					break;
			}
		}

		#region Pattern

		/// <summary>
		/// Create button graphic with repeating background pattern
		/// </summary>
		private void CreateFromPattern() {
			Rectangle outline = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
			TextureBrush pattern = new TextureBrush(_image, WrapMode.Tile);
			this.Color.Highlight = Draw.Utility.AdjustOpacity(this.Color.Highlight, 75);
			this.Graphic.FillRectangle(pattern, outline);
			pattern.Dispose();
			this.Graphic.DrawString(_text, this.TextFont, this.TextBrush, _textWidthPadding, -1);
			if (_shade) { this.Shade(outline); }
			this.Graphic.DrawRectangle(new Pen(this.Color.Border, _borderWidth), outline);
			this.ResetForNextImage();
			this.Graphic.DrawRectangle(new Pen(this.Color.Highlight, _highlightWidth), outline);
		}

		#endregion

		#region Rounded

		/// <summary>
		/// Create a rounded button background
		/// </summary>
		private void CreateRounded() {
			this.DrawRounded(this.ForegroundBrush);
			this.ResetForNextImage();
			this.DrawRounded(new SolidBrush(this.Color.Highlight));
		}

		/// <summary>
		/// Draw rounded button graphic
		/// </summary>
		/// <param name="brush"></param>
		private void DrawRounded(Brush brush) {
			this.Graphic.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0));
			// draw left arc
			this.Graphic.FillEllipse(brush, 0, 0, this.Height - 1, this.Height - 1);
			// draw right arc
			this.Graphic.FillEllipse(brush, (this.Width - this.Height), 0, this.Height - 1, this.Height - 1);
			// draw background
			this.Graphic.FillRectangle(brush, (int)this.Height / 2 - 1, -1, (this.Width - this.Height) + 1, this.Height + 1);
			// write text
			this.Graphic.DrawString(_text, this.TextFont, this.TextBrush, _textWidthPadding, -1);
		}

		#endregion

		#region Template

		/// <summary>
		/// Create button graphic from external template file
		/// </summary>
		/// <remarks>
		/// The external graphic file used as a template should be a blank
		/// button. This function will slice the ends for use in generated
		/// buttons and use a slice from the middle of the template as a pattern
		/// for the background of the button body.
		/// </remarks>
		private void CreateFromTemplate() {
			// rectangles capture needed parts of template image source and target
			Rectangle left = new Rectangle(0, 0, _templateEndPadding, this.Height);
			Rectangle right = new Rectangle(
				this.Width - _templateEndPadding, 0, _templateEndPadding, this.Height);
			Rectangle leftSrc = new Rectangle(0, 0, _templateEndPadding, this.Height);
			Rectangle rightSrc = new Rectangle(
				_image.Width - _templateEndPadding, 0, _templateEndPadding, this.Height);
			Rectangle midSrc = new Rectangle((int)_image.Width / 2, 0, 1, this.Height);

			// normal image
			this.DrawTextOnTemplate(left, leftSrc, right, rightSrc, midSrc,
				this.TextBrush, Color.Text);
			this.ResetForNextImage();

			// reset graphic for highlighted image
			this.Graphic = null;
			this.Graphic.TextRenderingHint = _antiAlias;
			_image = System.Drawing.Image.FromFile(this.Rename(_templateFile));
			this.DrawTextOnTemplate(left, leftSrc, right, rightSrc, midSrc,
				this.ActiveTextBrush, Color.ActiveText);
		}

		/// <summary>
		/// Draw text on the button graphic
		/// </summary>
		/// <param name="left">Area to copy template left to</param>
		/// <param name="leftSrc">Area of templte to copy for left edge</param>
		/// <param name="right">Area to copy template right to</param>
		/// <param name="rightSrc">Area of template to copy for right edge</param>
		/// <param name="midSrc">Area of template to copy for repeating mid-section</param>
		/// <param name="brush">Brush to use for drawing the text</param>
		/// <param name="color">Color for potential use in drawing arrows</param>
		private void DrawTextOnTemplate(Rectangle left, Rectangle leftSrc,
			Rectangle right, Rectangle rightSrc, Rectangle midSrc, Brush brush, Color color) {

			Pair<Triangle> arrows = new Pair<Triangle>(new Triangle(this.Graphic), null);
			int midWidth = (this.Width - _templateEndPadding) - 1;
			int arrowTop = 0;
			
			// draw the image template ends onto the button bitmap
			this.Graphic.DrawImage(_image, left, leftSrc, GraphicsUnit.Pixel);
			this.Graphic.DrawImage(_image, right, rightSrc, GraphicsUnit.Pixel);

			// tile the button middle
			for (int x = _templateEndPadding; x <= midWidth; x++) {
				this.Graphic.DrawImage(_image, new Rectangle(x, 0, 1, this.Height),
					midSrc, GraphicsUnit.Pixel);
			}
			if (_arrow != ShowArrow.None) {
				arrows.First.Height = _arrowSize.Height;
				arrows.First.Width = _arrowSize.Width;
				arrows.First.Color.ForeGround = color;
				arrowTop = (this.Height - arrows.First.Height) / 2;

				if (_arrow.Contains(ShowArrow.Left, ShowArrow.Right)) {
					arrows.Second = new Triangle(this.Graphic);
					arrows.Second.Import(arrows.First);
					arrows.Second.Color.ForeGround = arrows.First.Color.ForeGround;
				}
			}
			// draw left arrow if specified
			if (_arrow.Contains(ShowArrow.Left)) {
				arrows.First.Direction = Triangle.Directions.Left;
				arrows.First.Create(_arrowLeftEdge.First, arrowTop);
			}
			// write text
			this.Graphic.DrawString(_text, _textFont, brush,
				_textLeft, (0.3f * _textHeightPadding));

			// arrows that go right of text
			if (_arrow.Contains(ShowArrow.Left, ShowArrow.Right)) {
				arrows.Second.Direction = Triangle.Directions.Right;
				arrows.Second.Create(_arrowLeftEdge.Second, arrowTop);
			} else if (_arrow.Contains(ShowArrow.Right)) {
				arrows.First.Direction = Triangle.Directions.Right;
				arrows.First.Create(_arrowLeftEdge.First, arrowTop);
			}
			if (_arrow.Contains(ShowArrow.Up)) {
				arrows.First.Direction = Triangle.Directions.Up;
				arrows.First.Create(_arrowLeftEdge.First, arrowTop - 2);
			}
			if (_arrow.Contains(ShowArrow.Down)) {
				arrows.First.Direction = Triangle.Directions.Down;
				arrows.First.Create(_arrowLeftEdge.First, arrowTop);
			}
		}

		#endregion

		/// <summary>
		/// Add a gradient to create shade effect on button
		/// </summary>
		private void Shade(Rectangle outline) {
			Color bottomColor = System.Drawing.Color.FromArgb(150, 0, 0, 0);
			Color middleColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
			Color topColor = System.Drawing.Color.FromArgb(75, 255, 255, 255);
			LinearGradientBrush gradient = new LinearGradientBrush(outline, topColor, bottomColor, 90);
			ColorBlend blend = new ColorBlend();
			blend.Positions = new float[] { 0, 0.5F, 1 };
			blend.Colors = new Color[] { topColor, middleColor, bottomColor };
			gradient.InterpolationColors = blend;
			this.Graphic.FillRectangle(gradient, outline);
			gradient.Dispose();
		}

		/// <summary>
		/// Compute button width based on text width
		/// </summary>
		public void AdjustSizeForText() {
			Bitmap button = new Bitmap(1, 1);
			Graphics graphic = Graphics.FromImage(button);
			int totalArrowWidth = 0;

			graphic.TextRenderingHint = _antiAlias;
			_textSize = graphic.MeasureString(_text, this.TextFont).ToSize();

			if (_arrow != ShowArrow.None) {
				_arrowSize.Height = this.Height - (_textHeightPadding + 4);

				if (_arrow.Contains(ShowArrow.Up | ShowArrow.Down)) {
					// vertical arrows
					_arrowSize.Width = _arrowSize.Height;
				} else {
					// horizontal arrows
					_arrowSize.Width = (int)(_arrowSize.Height / 2);
				}
				totalArrowWidth = _arrowSize.Width;
				if (_arrow.Contains(ShowArrow.Left, ShowArrow.Right)) {
					totalArrowWidth *= 2; }
			}
			_contentSize.Width = _textSize.Width + totalArrowWidth;
			_contentSize.Height = _textSize.Height + _arrowSize.Height;

			if (this.Width == 0) {
				// compute width based on rendered string length and arrow (total content)
				this.Width = _contentSize.Width + (_textWidthPadding * 2);
				_textLeft = _textWidthPadding;
			} else {
				// center text within fixed width
				_textLeft = (this.Width - (_contentSize.Width)) / 2;
			}

			// adjust positioning for arrow
			if (_arrow != ShowArrow.None) {
				if (_arrow.Contains(ShowArrow.Left)) {
					_textLeft += _arrowSize.Width;
					_arrowLeftEdge.First = _textWidthPadding - 2;

					if (_arrow.Contains(ShowArrow.Left, ShowArrow.Right)) {
						_arrowLeftEdge.Second = _textLeft + _textSize.Width + (int)(0.07 * _textSize.Height);
					}
				} else {
					// these arrow types are to the right of any text
					_arrowLeftEdge.First = _textLeft + _textSize.Width + (int)(0.07 * _textSize.Height);
				}
			}
			this.Graphic.TextRenderingHint = _antiAlias;
		}
	}
}