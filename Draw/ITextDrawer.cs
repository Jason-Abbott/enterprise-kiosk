using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Idaho.Draw {
	public interface ITextDrawer {
		Font TextFont { get; }
		Brush TextBrush { get; }
		string Text { set; }
		string FontName { set; }
		FontStyle FontStyle { set; }
		void AdjustSizeForText();
	}
}
