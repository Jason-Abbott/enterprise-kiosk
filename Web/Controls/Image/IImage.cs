using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Web.Controls {
	public interface IImage {
		int Height { set; get; }
		int Width { set; get; }
		bool Resize { set; get; }
		float SharpenIntensity { set; get; }
		int SharpenRadius { set; get; }
	}
}
