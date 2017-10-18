using System;
using System.Collections.Generic;
using System.Xml;

namespace Idaho.Web.Controls {
	public class NavigationNodeCollection : List<NavigationNode> {

		private int _depth = -1;

		public int Depth {
			get {
				if (_depth == -1 && this.Count > 0) { _depth = this[0].Depth; }
				return _depth;
			}
		}

		internal NavigationNodeCollection(XmlNodeList nodeList, NavigationNode parentNode) {

			NavigationNode node;

			foreach (XmlNode n in nodeList) {
				node = new NavigationNode(n, parentNode);
				//If node.Roles Is Nothing OrElse user.HasPermission(node.Roles) Then
				if (n.HasChildNodes) {
					node.ChildNodes = new NavigationNodeCollection(n.ChildNodes, node);
				}
				this.Add(node);
				//End If
			}
		}
		internal NavigationNodeCollection(XmlNodeList nodeList)
			: this(nodeList, null) {
		}
	}
}