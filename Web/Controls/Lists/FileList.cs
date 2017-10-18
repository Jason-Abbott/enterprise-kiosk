using System;
using System.IO;
using System.Web;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Render a drop-down list of file names matching filter in given path
	/// </summary>
	public class FileList : Idaho.Web.Controls.SelectList {

		private string _path = string.Empty;
		// bitmask of File.Types
		private Int32 _filter = 0;

#region Properties

		public Int32 Filter { set { _filter = value; } }
		public string Folder { get { return _path; } set { _path = value; } }
		public new string TopSelection {
			get {
				string selection = base.Selection;
				if (selection == "0") { selection = string.Empty; }
				return selection;
			}
		}

#endregion

		private void FileList_Load(object sender, System.EventArgs e) {
			//Me.RegisterValidation(Validation.Types.Select, "HDV File")
		}

		protected override void Render(System.Web.UI.HtmlTextWriter writer) {
			if (_path == string.Empty) {
				throw new NullReferenceException("Must supply path");
			}

			DirectoryInfo folder = new DirectoryInfo(
				string.Format("{0}{1}", HttpRuntime.AppDomainAppPath, _path));

			FileInfo[] files = folder.GetFiles();
			//Array.Sort(files, New AMP.Compare.File.Name)

			this.RenderLabel(writer);
			this.RenderBeginTag(writer);
			foreach (FileInfo f in files) {
				if (_filter == 0) {
					//OrElse HasBits(AMP.File.InferType(f.Name), _filter) Then
					writer.Write("<option value=\"");
					writer.Write(f.Name);
					writer.Write("\"");
					this.RenderSelected(f.Name, writer);
					writer.Write(">");
					writer.Write(f.Name);
					writer.Write("</option>");
				}
			}
			this.RenderEndTag(writer);
		}
	}
}
