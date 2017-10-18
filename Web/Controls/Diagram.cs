using Idaho.Attributes;
using Idaho.Draw;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Web.Caching;
using System.Web.UI;

namespace Idaho.Web.Controls {
	/// <summary>
	/// Base class for rendering diagrams as HTML
	/// </summary>
	public abstract class Diagram : AjaxControl {
		private Draw.Diagram.Surface _surface = null;
		private int _gutter = 0;
		private int _margin = 0;
		private int _layer = 0;
		private int _width = 0;
		private int _labelHeight = 30;
		private int _connectorMidPointPercent = 0;
		private int _connectorMidPointStep = 0;
		const int _connectorPadLeftPercent = 5;
		const int _connectorPadRightPercent = 12;
		private bool _fromCache = false;
		private bool _wireDomEvents = true;
		private CacheItemPriority _cachePriority = CacheItemPriority.Normal;
		private TimeSpan _cacheDuration = TimeSpan.MinValue;
		private string _cacheKey = string.Empty;

		#region Properties

		protected TimeSpan CacheDuration { set { _cacheDuration = value; } }
		protected CacheItemPriority CachePriority { set { _cachePriority = value; } }
		protected string CacheKey {
			set { _cacheKey = value; }
			get {
				if (string.IsNullOrEmpty(_cacheKey)) {
					// if a key hasn't been set by a subclass, generate one
					StringBuilder keys = new StringBuilder();
					foreach (string k in this.Context.Request.Form.Keys) {
						keys.AppendFormat("{0}={1};", k, this.Context.Request.Form[k]);
					}
					_cacheKey = keys.ToString();
				}
				return _cacheKey;
			}
		}
		
		/// <summary>
		/// Number of minutes to cache grid data; default is zero, no caching.
		/// </summary>
		[WebBindable()]
		public int CacheMinutes {
			set { _cacheDuration = TimeSpan.FromMinutes(value); }
			get { return _cacheDuration.Minutes; }
		}

		/// <summary>
		/// The data displayed in this diagram
		/// </summary>
		protected Draw.Diagram.Surface Surface {
			set {
				_surface = value;
				if (_cacheDuration > TimeSpan.MinValue && _surface != null) {
					base.Context.Cache.Add(this.CacheKey, _surface, null,
						DateTime.Now + _cacheDuration, Cache.NoSlidingExpiration, _cachePriority, null);
				}
			}
			get {
				if (!_fromCache && _cacheDuration > TimeSpan.MinValue) {
					Draw.Diagram.Surface s = base.Context.Cache[this.CacheKey] as Draw.Diagram.Surface;
					if (s != null) {
						_surface = s;
						_fromCache = true;
					}
				}
				return _surface;
			}
		}
		protected bool HasCacheData { get { return _fromCache; } }


		/// <summary>
		/// If true, EcmaScript is rendered to handle mouse events with entity labels
		/// </summary>
		[WebBindable()]
		public bool WireDomEvents { set { _wireDomEvents = value; } get { return _wireDomEvents; } }

		/// <summary>
		/// Height of label in pixels
		/// </summary>
		[WebBindable()]
		public int LabelHeight { set { _labelHeight = value; } get { return _labelHeight; } }

		/// <summary>
		/// Space in pixels between child entities
		/// </summary>
		[WebBindable()]
		public int Gutter { set { _gutter = value; } get { return _gutter; } }

		/// <summary>
		/// The z-index at which the diagram surface should start
		/// </summary>
		[WebBindable()]
		public int zIndex { set { _layer = value; } get { return _layer; } }

		/// <summary>
		/// Space in pixels around inside edge of this container
		/// </summary>
		[WebBindable()]
		public int Margin { set { _margin = value; } get { return _margin; } }

		[WebBindable()]
		public int Width { set { _width = value; } get { return _width; } }

		#endregion

		public Diagram() { }

		#region Events

		protected override void OnInit(EventArgs e) {
			if (this.Ajax.IsRenderingContent) {
				_surface = new Draw.Diagram.Surface(new Size(_width, 0));
				_surface.LabelHeight = _labelHeight;
				_surface.EqualizeHeight = true;
				_surface.Layer = _layer;
				_surface.Margin = _margin;
				_surface.Gutter = _gutter;
			}
			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e) {
			if (this.Ajax.IsRenderState(AjaxBase.RenderStates.InPage)) {
				this.Page.ScriptFile.AddResource(EcmaScript.Resource.Diagram);
			}
			base.OnLoad(e);
		}

		protected override void OnPreRender(EventArgs e) {
			if (this.Ajax.IsRenderingContent) { _surface.PreRender(); }
			base.OnPreRender(e);
		}

		#endregion

		protected override void RenderContent(HtmlTextWriter writer) {
			if (_surface.Connectors != null) {
				_connectorMidPointStep = (int)((100f - (_connectorPadLeftPercent + _connectorPadRightPercent)) / (double)_surface.Connectors.Count);
				if (_connectorMidPointStep == 0) { _connectorMidPointStep = 1; }
			}
			_connectorMidPointPercent = _connectorPadLeftPercent;

			writer.Write("<div class=\"surface\" style=\"position: relative; ");
			writer.Write("width: {0}px; height: {1}px; z-index: {2};\">",
				_surface.Width, _surface.Height, _surface.Layer);

			if (_surface.Panels != null) {
				foreach (Draw.Diagram.Panel p in _surface.Panels) { this.RenderPanel(p, writer); }
			}
			if (_surface.Items != null) {
				foreach (Draw.Diagram.Item i in _surface.Items) { this.RenderItem(i, writer); }
			}
			if (_surface.Connectors != null) {
				foreach (Draw.Diagram.Connector c in _surface.Connectors) { this.RenderConnector(c, writer); }
			}
			writer.Write("</div>");
		}

		private void RenderPanel(Draw.Diagram.Panel p, HtmlTextWriter writer) {
			bool hasChildren = false;

			writer.Write("<fieldset id=\"{0}\"", p.ID);
			this.RenderStyle(p, "panel", writer);
			writer.Write("><legend style=\"font-size: {0}px;\">", p.LabelFontSize);
			this.RenderLabel(p, writer);
			writer.Write("</legend>");

			if (p.Items != null) {
				hasChildren = true;
				foreach (Draw.Diagram.Item i in p.Items) { this.RenderItem(i, writer); }
			}
			if (p.Panels != null) {
				hasChildren = true;
				foreach (Draw.Diagram.Panel pnl in p.Panels) { this.RenderPanel(pnl, writer); }
			}
			// fieldsets require some content to render correctly
			if (!hasChildren) { writer.Write(Symbol.NonBreakingSpace); }
			writer.Write("</fieldset>");
		}

		private void RenderItem(Draw.Diagram.Item i, HtmlTextWriter writer) {
			writer.Write("<div id=\"{0}\"", i.ID);
			this.RenderStyle(i, "item", writer);
			writer.Write(">");
			this.RenderLabel(i, writer);
			writer.Write("</div>");
		}

		/// <summary>
		/// Render connecting lines between entities
		/// </summary>
		private void RenderConnector(Draw.Diagram.Connector c, HtmlTextWriter writer) {
			switch (c.LineStyle) {
				case Draw.Diagram.Connector.Styles.Orthoganol:
					int width = (int)(c.Width * (_connectorMidPointPercent / 100f));
					writer.Write("<div id=\"{0}\"", c.ID);
					this.RenderStyle(c, "connect" + c.Trend.ToString(), writer);
					writer.Write(">");
					// left side of connector
					writer.Write("<div style=\"position: absolute; left: 0; top: 0; ");
					writer.Write("width: {0}px; ", width);
					writer.Write("height: {0}px;\" class=\"left\"></div>", c.Height);
					// right side of connector
					writer.Write("<div style=\"position: absolute; right: 0; top: 0; ");
					writer.Write("width: {0}px; ", c.Width - width);
					writer.Write("height: {0}px;\" class=\"right\"></div>", c.Height);
					writer.Write("</div>");
					_connectorMidPointPercent += _connectorMidPointStep;
					if (_connectorMidPointPercent > 100 - _connectorPadRightPercent) {
						_connectorMidPointPercent = _connectorPadLeftPercent;
					}
					break;
				case Draw.Diagram.Connector.Styles.Direct:
					Line line = new Line();
					line.Width = 1;
					line.Start = c.FromPoint;
					line.End = c.ToPoint;
					line.Color = "#fff";
					line.Style.Add("position", "absolute");
					line.Style.Add("top", c.Top + "px");
					line.Style.Add("left", c.Left + "px");
					line.Style.Add("height", c.Height + "px");
					line.Style.Add("width", c.Width + "px");
					line.RenderControl(writer);
					break;
			}
		}

		/// <summary>
		/// Render entity label
		/// </summary>
		private void RenderLabel(Draw.Diagram.Entity e, HtmlTextWriter writer) {
			if (_wireDomEvents && e.HandleEvents) {
				// render EcmaScript to handle click on this entity
				writer.Write("<a onclick=\"Diagram.OnItemClick('{0}',", e.ID);
				writer.Write("[{0}],[{1}]);\" ",
					Draw.Diagram.Entity.IdList(e.ConnectedFrom),
					Draw.Diagram.Entity.IdList(e.ConnectedTo));
				writer.Write("title=\"{0} {1}\">{2}</a>",
					e.TypeName, e.ID, e.HtmlLabel);
			} else if (!string.IsNullOrEmpty(e.ID)) {
				// show tooltip for this entity
				writer.Write("<acronym title=\"{0} {1}\">{2}</acronym>",
					e.TypeName, e.ID, e.HtmlLabel);
			} else {
				// render label alone
				writer.Write(e.HtmlLabel);
			}
			if (!string.IsNullOrEmpty(e.LabelSuffix)) {
				writer.Write("<span class=\"suffix\">");
				writer.Write(e.LabelSuffix);
				writer.Write("</span>");
			}
		}

		/// <summary>
		/// Render CSS to position the entity
		/// </summary>
		private void RenderStyle(Draw.Diagram.Entity e, string defaultCss, HtmlTextWriter writer) {
			int width = e.Width;
			int height = e.Height;
			int top = e.Top;
			int left = e.Left;
			string css = (string.IsNullOrEmpty(e.StyleClass)) ? defaultCss : e.StyleClass;

			// handles the IE box model which adds border outside given dimensions
			if (e.BorderWidth == 1) {
				width -= 2;
				height -= 2;
				top--;
				left--;
			}
			// connector is boxes within boxes so handle box model again
			if (e is Draw.Diagram.Connector) { left -= 4; top -= 3; }

			writer.Write(" class=\"{0}\" style=\"position: absolute; ", css);
			writer.Write("width: {0}px; height: {1}px; ", width, height);
			writer.Write("top: {0}px; left: {1}px; z-index: {2};", top, left, e.Layer);
			if (!(e is Draw.Diagram.Connector)) {
				if (e.BorderWidth == 0) { writer.Write(" border: none;"); }
				writer.Write(" font-size: {0}px; line-height: {0}px;", e.LabelFontSize);
			}
			writer.Write("\"");
		}
	}
}
