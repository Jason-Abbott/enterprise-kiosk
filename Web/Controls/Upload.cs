using Idaho.Data;
using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Configuration;

namespace Idaho.Web.Controls {
	public class Upload : Idaho.Web.Controls.InputControl {

		private bool _parse = false;
		private Types _allowedTypes = Types.Any;
		private bool _uploaded = false;
		private int _maxFileKB = 0;
		private FileInfo _file;

		[Flags]
		public enum Types {
			Image = Jpeg | Gif | Png,
			Any = 0x0,
			Jpeg = 0x1,
			Gif = 0x2,
			Png = 0x4
		}

		/// <summary>
		/// The saved file, if any
		/// </summary>
		public FileInfo File { get { return _file; } }

		#region Static Fields

		private static int _defaultMaxFileKB = 0;
		private static DirectoryInfo _folder;

		public static int DefaultMaxFileSize { set { _defaultMaxFileKB = value; } }
		public static DirectoryInfo Folder { set { _folder = value; } get { return _folder; } }

		#endregion

		public Types AllowedTypes { set { _allowedTypes = value; } }
		public bool Parse { set { _parse = value; } }
		public bool Uploaded { get { return _uploaded; } }
		public int MaxFileSize { set { _maxFileKB = value; } }

		#region IPostBackDataHandler (from InputControl)

		public override bool LoadPostData(string key, NameValueCollection posted) {
			HttpPostedFile postedFile = this.Context.Request.Files[this.UniqueID];

			if ((postedFile != null) && postedFile.ContentLength > 0) {
				_uploaded = this.Save(postedFile);
			}
			return false;
		}

		#endregion

		#region Events

		protected override void OnInit(EventArgs e) {
			this.ShowLabel = true;
			this.ValidationType = Validation.Types.File;
			base.OnInit(e);
		}
		protected override void OnLoad(EventArgs e) {
			this.Page.Form.Enctype = "multipart/form-data";
			this.RegisterValidation();
			base.OnLoad(e);
		}
		protected override void OnPreRender(EventArgs e) {
			Page.RegisterRequiresPostBack(this);
			base.OnPreRender(e);
		}

		#endregion

		/// <summary>
		/// Write labeled control with validation and notes
		/// </summary>
		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			this.Attributes.Add("class", "file");
			this.RenderLabel(writer);
			writer.Write("<input type=\"file\" name=\"");
			writer.Write(this.UniqueID);
			writer.Write("\"");
			this.RenderAttributes(writer);
			writer.Write(">");
			this.RenderNote(writer);
		}

		/// <summary>
		/// Save posted file to disk and update file information field
		/// </summary>
		private bool Save(HttpPostedFile posted) {
			int maxSize = Utility.NoNull<int>(_maxFileKB, _defaultMaxFileKB);

			if (posted.ContentLength > (maxSize * 1024)) {
				this.Page.Profile.Message = Resource.SayFormat("Error_LargeFile", maxSize);
				return false;
			}
			if (_allowedTypes != Types.Any &&
				!_allowedTypes.Contains(this.InferType(posted.FileName))) {

				this.Page.Profile.Message = Resource.SayFormat("Error_FileType", posted.FileName);
				return false;
			}
			try {
				_file = new FileInfo(string.Format("{0}/{1}", _folder.FullName, posted.FileName));
				this.UpdateToAvailableName(ref _file);
				posted.SaveAs(_file.FullName);
			} catch (UnauthorizedAccessException e) {
				Idaho.Exception.Log(e, Exception.Types.FileSystem);
				_file = null;
				return false;
			} catch (System.Exception e) {
				Idaho.Exception.Log(e, Exception.Types.Unknown);
				_file = null;
				return false;
			}
			if (_file == null) {
				// unable to save file
				this.Page.Profile.Message = Resource.Say("Error_FileSave");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Infer file type from name
		/// </summary>
		private Types InferType(string filename) {
			string extension = Path.GetExtension(filename).ToLower();
			switch (extension) {
				case "jpg":
				case "jpeg":	return Types.Jpeg;
				case "gif":		return Types.Gif;
				case "png":		return Types.Png;
				default:		return Types.Any;
			}
		}

		/// <summary>
		/// Find an available file name
		/// </summary>
		private void UpdateToAvailableName(ref FileInfo file) {
			string tryName = file.Name;
			string newName = string.Empty;
			int x = 2;

			while (file.Exists) {
				newName = string.Format("{0}_{1:00}{2}", 
					Path.GetFileNameWithoutExtension(tryName), x, file.Extension);
				file = new FileInfo(file.FullName.Replace(file.Name, newName));
				x++;
			}
			return;
		}
	}
}