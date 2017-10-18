using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Render sequence of images in a table
	/// </summary>
	public class ImageStrip : HtmlContainerControl, IImage {

		private List<FileInfo> _images = new List<FileInfo>();
		private List<string> _clickAction = new List<string>();
		private int _height;
		private int _width;
		private bool _resize;
		private float _sharpenIntensity;
		private int _sharpenRadius;

		#region Properties

		public List<string> OnClick { get { return _clickAction; } set { _clickAction = value; } }
		public List<FileInfo> Images { get { return _images; } set { _images = value; } }
		public int Height { set { _height = value; } get { return _height; } }
		public int Width { set { _width = value; } get { return _width; } }
		public bool Resize { set { _resize = value; } get { return _resize; } }
		public float SharpenIntensity {
			set { _sharpenIntensity = value; }
			get { return _sharpenIntensity; }
		}
		public int SharpenRadius {
			set { _sharpenRadius = value; }
			get { return _sharpenRadius; }
		}

		#endregion

		public ImageStrip() : base("table") { }

		protected override void OnLoad(EventArgs e) {
			if (_images.Count == 0) {
				this.Visible = false;
			} else {
				HtmlTableRow row = new HtmlTableRow();
				HtmlTableCell cell;
				Image image;
				int x = 1;

				foreach (FileInfo f in _images) {
					cell = new HtmlTableCell();
					image = new Image(this.Page);
					image.ID = string.Format("{0}_{1}", this.ID, x);
					image.File = f;
					image.Width = _width;
					image.Height = _height;
					image.Resize = _resize;
					image.OnClick = _clickAction[x - 1];
					image.SharpenIntensity = _sharpenIntensity;
					image.SharpenRadius = _sharpenRadius;
					x++;

					cell.Controls.Add(image);
					row.Controls.Add(cell);
				}
				this.Controls.Add(row);
			}
			base.OnLoad(e);
		}
	}
}
