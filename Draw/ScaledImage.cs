using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Web;

namespace Idaho.Draw {
	public class ScaledImage : GraphicBase {

		private FileInfo _file;
		private int _sharpenRadius = 0;
		private float _sharpenIntensity = 1;
		private string _rotate = string.Empty;

		internal int SharpenRadius { set { _sharpenRadius = value; } }
		internal float SharpenIntensity { set { _sharpenIntensity = value; } }
		internal string Rotate { set { _rotate = value; } }

		#region Constructor

		public ScaledImage(FileInfo path, int width, int height) {
			_file = path;
			base.Width = width;
			base.Height = height;
		}
		public ScaledImage(string path, int width, int height) {
			path = string.Format("{0}{1}",
				System.Web.HttpRuntime.AppDomainAppPath, path.Replace('/', '\\'));
			_file = new FileInfo(path);
			base.Width = width;
			base.Height = height;
		}

		#endregion

		/// <summary>
		/// Get file name for this graphic
		/// </summary>
		protected override string GenerateFileName() {
			StringBuilder fileName = new StringBuilder();
			fileName.Append(_file.Name.Substring(0, _file.Name.IndexOf('.')).Replace(" ", "_"));
			fileName.Append("_");
			if (base.Width > 0) { fileName.AppendFormat("{0}w", base.Width); }
			if (base.Height > 0) { fileName.AppendFormat("{0}h", base.Height); }
			fileName.Append(".");
			fileName.Append(base.Extension);
			return fileName.ToString();
		}

		internal override void Create() {
			base.Image = Idaho.Draw.Utility.Resize(
				_file, base.Width, base.Height, _sharpenRadius, _sharpenIntensity, _rotate);
		}
	}
}
