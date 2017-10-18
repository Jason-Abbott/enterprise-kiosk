using System;
using System.Xml;

namespace Idaho.Web.Controls {
	public class NavigationNode : IComparable<NavigationNode> {

		private string _url = string.Empty;
		private string _title = string.Empty;
		private string[] _roles;
		private int _depth = 0;
		private string _width = string.Empty;
		private bool _newSection;
		private NavigationNodeCollection _childNodes;
		private NavigationNode _parentNode;

#region Properties

		internal string Width { get { return _width; } }
		internal bool NewSection { get { return _newSection; } }
		internal int Depth { get { return _depth; } }
		internal string Url { get { return _url; } }
		internal string Title { get { return _title; } }
		internal string[] Roles { get { return _roles; } }
		internal NavigationNodeCollection ChildNodes {
			get { return _childNodes; }
			set { _childNodes = value; }
		}
		internal NavigationNode ParentNode { get { return _parentNode; } }

#endregion

		internal NavigationNode(XmlNode node, NavigationNode parent) {
			XmlAttribute section = node.Attributes["newSection"];
			XmlAttribute url = node.Attributes["url"];
			XmlAttribute roles = node.Attributes["roles"];
			XmlAttribute title = node.Attributes["title"];
			XmlAttribute width = node.Attributes["width"];

			if (parent != null) {
				_parentNode = parent;
				_depth = parent.Depth + 1;
			} else {
				_depth = 1;
			}
			_newSection = (section != null) && bool.Parse(section.Value);
			if (url != null) { _url = url.Value.Replace("~", Utility.BasePath); }
			if (title != null) { _title = title.Value; }
			if (roles != null) { _roles = roles.Value.Split(','); }
			if (width != null) { _width = width.Value; }
		}

		public bool HasChildNodes() {
			return !(_childNodes == null || _childNodes.Count == 0);
		}

		public int CompareTo(NavigationNode other) { return _title.CompareTo(other.Title); }
	}
}