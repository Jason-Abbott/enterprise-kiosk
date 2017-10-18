using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Idaho.Web {
	/// <summary>
	/// List of client resource references having common path and extension
	/// </summary>
	public class ClientResourceList : UniqueStringList {
		
		private string _path = string.Empty;
		private string _extension = string.Empty;
		private string _parentPath = string.Empty;
		private string _theme = string.Empty;

		#region Properties

		/// <summary>
		/// Set theme subfolder name
		/// </summary>
		internal string Theme { set { _theme = value; } }

		/// <summary>
		/// Root path of the sub-application
		/// </summary>
		private string ParentPath {
			get {
				if (_parentPath == string.Empty) {
					string path = HttpContext.Current.Request.Path;
					_parentPath = path.Substring(1, path.LastIndexOf('/'));
				}
				return _parentPath;
			}
		}

		#endregion

		public ClientResourceList(string path, string extension) {
			_path = path; _extension = extension;
		}
		private ClientResourceList() { }

		/// <summary>
		/// Add a reference to an embedded resource
		/// </summary>
		/// <param name="key">Name only, without extension, of the resource file</param>
		public void AddResource(string key) {
			base.Add(this.ResourceURL(key));
		}
		public void InsertResource(int index, string key) {
			base.Insert(index, this.ResourceURL(key));
		}
		public new void Add(string name) {
			base.Add(this.DefaultURL(name, false, string.Empty));
		}
		
		/// <summary>
		/// Add a client resource
		/// </summary>
		/// <param name="name">The name only (no extension) of file resource</param>
		/// <param name="subFolder">Sub-folder beneath the default for this type of resource</param>
		public void Add(string name, string subFolder) {
			base.Add(this.DefaultURL(name, false, subFolder));
		}
		/// <summary>
		/// Add a client resource
		/// </summary>
		/// <param name="name">The name only (no extension) of file resource</param>
		/// <param name="useSubPath">If this is a sub-application, should the sub-application
		/// path be preprended</param>
		public void Add(string name, bool useParentPath) {
			base.Add(this.DefaultURL(name, useParentPath, string.Empty));
		}
		public void Add(string name, bool useParentPath, string subFolder) {
			base.Add(this.DefaultURL(name, useParentPath, subFolder));
		}
		
		public new void Insert(int index, string name) {
			base.Insert(index, this.DefaultURL(name, false, string.Empty));
		}
		public void InsertFirst(string name, bool useParentPath) {
			base.Insert(0, this.DefaultURL(name, useParentPath, string.Empty));
		}
		public void InsertFirst(string name) { this.InsertFirst(name, false); }

		/// <summary>
		/// Convert the list to a literal control based on the given string pattern
		/// </summary>
		/// <param name="linkPattern">String format pattern for resource link</param>
		/// <returns></returns>
		public LiteralControl ToControl(string linkPattern) {
			StringBuilder html = new StringBuilder();
			foreach (string s in this) { html.AppendFormat(linkPattern, s);	}
			return new LiteralControl(html.ToString());
		}
		private string DefaultURL(string name, bool useParentPath, string subFolder) {
			string parentPath = (useParentPath) ? this.ParentPath : "";
			if (!string.IsNullOrEmpty(_theme)) { subFolder = _theme; }
			subFolder += "/";
			if (subFolder != "/") { subFolder = "/" + subFolder; }
			return string.Format("{0}{1}{2}{3}.{4}", parentPath, _path, subFolder, name, _extension);
		}
		private string ResourceURL(string name) {
			return Handlers.Resource.GetURL(string.Format("{0}.{1}.{2}", _path, name, _extension));
		}
	}
}
