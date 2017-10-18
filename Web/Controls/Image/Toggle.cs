using System;
using System.Drawing;
using System.Web.UI;
using System.Configuration;

namespace Idaho.Web.Controls {
	public class Toggle : Idaho.Web.Controls.Image {

		private Color _color;
		private Color _backGroundColor;
		private Draw.Toggle.States _initialState = Draw.Toggle.States.Open;
		private string _nodeID = string.Empty;
		private bool _suppressClick = false;
		private string _click = string.Empty;

		#region Static Fields

		private static int _defaultHeight = 0;
		private static Color _defaultColor;

		public static int DefaultHeight { set { _defaultHeight = value; } }
		public static Color DefaultColor { set { _defaultColor = value; } }

		#endregion

		#region Properties

		public bool SuppressClick { set { _suppressClick = value; } }
		public string NodeID { set { _nodeID = value; } }
		public Draw.Toggle.States InitialState { set { _initialState = value; } }
		public string Color { set { _color = ColorTranslator.FromHtml(value); } }
		public string BackGround { set { _backGroundColor = ColorTranslator.FromHtml(value); } }

		#endregion

		/// <summary>
		/// Forces control to render
		/// </summary>
		/// <param name="nodeID"></param>
		public void RenderControl(HtmlTextWriter writer, string nodeID) {
			if (nodeID != string.Empty) { _nodeID = nodeID; }
			this.OnPreRender(null);
			base.Render(writer);
		}
		public new void RenderControl(HtmlTextWriter writer) {
			this.RenderControl(writer, null);
		}

		protected override void OnPreRender(EventArgs e) {
			string cacheKey = string.Format("toggle{0}{1}", this.Height, _color.ToString());

			if (!this.TagInCache(cacheKey)) {
				Idaho.Draw.Toggle draw = new Idaho.Draw.Toggle();
		
				this.Transparency = true;
				draw.Height = Utility.NoNull<int>(this.Height, _defaultHeight);
				draw.Width = draw.Height;
				draw.Color.BackGround = _backGroundColor;
				draw.Color.ForeGround = Utility.NoNull<Color>(_color, _defaultColor);
				draw.Color.Border = draw.Color.ForeGround;
				draw.State = _initialState;
				this.Src = draw.Url;
				this.Generated = true;
			}

			// attributes that aren't cached
			if ((!_suppressClick) && this.OnClick == string.Empty) {
				this.OnClick = string.Format("WebPart.Toggle(this, '{0}');", _nodeID);
			}
			this.Generated = true;
			this.AlternateText = "";
			base.OnPreRender(e);
		}
	}
}