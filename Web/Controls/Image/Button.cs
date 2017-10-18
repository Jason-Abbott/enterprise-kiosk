using Idaho;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web.UI;
using System.Configuration;

namespace Idaho.Web.Controls {
	public class Button : Idaho.Web.Controls.Image {

		private string _text = string.Empty;
		private string _fontName = string.Empty;
		private string _resourceKey = string.Empty;
		private string _configurationKey = string.Empty;
		private string _url = string.Empty;
		private string _templateFile = string.Empty;
		private string _textureFile = string.Empty;
		private int _templateEndPadding = -1;
		private int _textEndPadding = -1;
		private int _textHeightPadding = -1;
		private bool _forceRegeneration = false;
		private KeyCode _clickKey = 0;
		private Color _textColor;
		private Color _activeTextColor;
		private int _activeTextOpacity = -1;
		private int _textOpacity = -1;
		private Color _borderColor;
		private Idaho.Draw.Button.Styles _style = 0;
		private Page.Actions _action = Page.Actions.None;
		private static Dictionary<string, Idaho.Draw.Button.ShowArrow> _arrowKeys;

		#region Static Fields

		private static int _defaultHeight = 0;
		private static string _defaultFontName = string.Empty;
		private static Color _defaultHighlightColor;
		private static Color _defaultActiveTextColor;
		private static Color _defaultTextColor;
		private static Color _defaultBorderColor;
		private static int _defaultActiveTextOpacity = 100;
		private static int _defaultTextHeightPadding = 0;
		private static int _defaultTextWidthPadding = 0;
		private static int _defaultTextOpacity = 100;
		private static string _defaultTemplateFile = string.Empty;
		private static int _defaultTemplateEndPadding = 0;

		public static int DefaultHeight { set { _defaultHeight = value; } }
		public static string DefaultTemplateFile { set { _defaultTemplateFile = value; } }
		public static Color DefaultHighlightColor { set { _defaultHighlightColor = value; } }
		public static Color DefaultActiveTextColor { set { _defaultActiveTextColor = value; } }
		public static Color DefaultTextColor { set { _defaultTextColor = value; } }
		public static Color DefaultBorderColor { set { _defaultBorderColor = value; } }
		public static string DefaultFontName { set { _defaultFontName = value; } }
		public static int DefaultActiveTextOpacity {
			set { _defaultActiveTextOpacity = Utility.Constrain(value, 0, 100); }
		}
		public static int DefaultTextOpacity {
			set { _defaultTextOpacity = Utility.Constrain(value, 0, 100); }
		}
		/// <summary>
		/// Pixel space to leave above and below text
		/// </summary>
		public static int DefaultTextHeightPadding { set { _defaultTextHeightPadding = value; } }

		/// <summary>
		/// Pixel space to leave left and right of text
		/// </summary>
		public static int DefaultTextWidthPadding { set { _defaultTextWidthPadding = value; } }

		/// <summary>
		/// Pixels length to clip off each end of the template graphic
		/// </summary>
		public static int DefaultTemplateEndPadding { set { _defaultTemplateEndPadding = value; } }

		#endregion

		#region Properties

		/// <summary>
		/// Should the ENTER key cause the click action to fire
		/// </summary>
		public bool ActivateWithEnterKey {
			get { return (_clickKey == KeyCode.Enter); }
			set { _clickKey = (value) ? KeyCode.Enter : 0; }
		}
		/// <summary>
		/// Fire the click event with then given key is pressed
		/// </summary>
		/// <remarks>
		/// This is supported by rendering an EcmaScript event handler for this
		/// button ID
		/// </remarks>
		public KeyCode ActivateWithKey {
			get { return _clickKey; } set { _clickKey = value; }
		}

		public Idaho.Draw.Button.Styles GraphicStyle { set { _style = value; } }

		public string FontName { set { _fontName = value; } }

		public int TextEndPadding { set { _textEndPadding = value; } }
		public int TextHeightPadding { set { _textHeightPadding = value; } }

		/// <summary>
		/// Link targeted by button click
		/// </summary>
		public string Url { set { _url = value; } }
		public int TemplateEndPadding { set { _templateEndPadding = value; } }
		public string TemplateFile { set { _templateFile = value; } }

		/// <summary>
		/// Key name to look up button configuration
		/// </summary>
		public string ConfigurationKey { set { _configurationKey = value; } }
		public string Config { set { this.ConfigurationKey = value; } }
		public string Text { set { _text = value; } }

		/// <summary>
		/// Force generation of a new button graphic
		/// </summary>
		public bool ForceNew { set { _forceRegeneration = value; } }
		//public string TextColor { set { _textColor = ColorTranslator.FromHtml(value); } }
		public Color TextColor { set { _textColor = value; } }
		public string BorderColor { set { _borderColor = ColorTranslator.FromHtml(value); } }
		//public string ActiveTextColor { set { _activeTextColor = ColorTranslator.FromHtml(value); } }
		public Color ActiveTextColor { set { _activeTextColor = value; } }
		public string Resx { set { _resourceKey = value; } }

		/// <summary>
		/// Resource key to look up button text
		/// </summary>
		public string ResourceKey { set { this.Resx = value; } }
		public int ActiveTextOpacity { set { _activeTextOpacity = Utility.Constrain(value, 0, 100); } }
		public int TextOpacity { set { _textOpacity = Utility.Constrain(value, 0, 100); } }
		public Page.Actions Action {
			set { _action = value; } get { return _action; }
		}

		#endregion

		#region Constructors

		static Button() {
			_arrowKeys = new Dictionary<string, Idaho.Draw.Button.ShowArrow>();
			_arrowKeys.Add(">", Idaho.Draw.Button.ShowArrow.Right);
			_arrowKeys.Add("<", Idaho.Draw.Button.ShowArrow.Left);
			_arrowKeys.Add("^", Idaho.Draw.Button.ShowArrow.Up);
			_arrowKeys.Add("\\/", Idaho.Draw.Button.ShowArrow.Down);
		}
		public Button() { }
		public Button(string id) { this.ID = id; }
		public Button(string id, string resx) : this(id) { _resourceKey = resx; }
		public Button(string id, string resx, Page.Actions action)
			: this(id, resx) { _action = Action; }
		public Button(string id, string resx, Page.Actions action, bool submitForm)
			: this(id, resx, action) { this.SubmitForm = submitForm; }

		#endregion

		#region Events

		/// <summary>
		/// Attach key press to click event if specified
		/// </summary>
		protected override void OnLoad(EventArgs e) {
			if (_clickKey != 0 && this.Visible) {
				this.Page.ScriptBlock = Idaho.Resource.SayFormat("Script_KeyForButton",
					this.ID, (int)_clickKey);
			}
			base.OnLoad(e);
		}

		/// <summary>
		/// Get values needed to render button
		/// </summary>
		protected override void OnPreRender(EventArgs e) {
			string cacheKey = (string.IsNullOrEmpty(_resourceKey) ? _text : _resourceKey)
				+ _configurationKey;

			if (!this.TagInCache(cacheKey) || _forceRegeneration) {
				if (!Configuration.Button.Current.ApplySettings(_configurationKey, this)) {
					// if not configurable then use defaults
					if (_activeTextOpacity == -1) { _activeTextOpacity = _defaultActiveTextOpacity; }
					if (_textOpacity == -1) { _textOpacity = _defaultTextOpacity; }
				}
				// set defaults as necessary
				if (string.IsNullOrEmpty(_fontName)) { _fontName = _defaultFontName; }
				if (this.Height <= 0) { this.Height = _defaultHeight; }
				if (string.IsNullOrEmpty(_templateFile)) { _templateFile = _defaultTemplateFile; }
				if (_templateEndPadding == -1) { _templateEndPadding = _defaultTemplateEndPadding; }
				if (_textEndPadding == -1) { _textEndPadding = _defaultTextWidthPadding; }
				if (_textHeightPadding == -1) { _textHeightPadding = _defaultTextHeightPadding; }
				if (string.IsNullOrEmpty(_fontName)) { _fontName = _defaultFontName; }
				if (string.IsNullOrEmpty(_fontName)) { _fontName = "Tahoma"; }

				Draw.Button.ShowArrow arrow = Idaho.Draw.Button.ShowArrow.None;
				Assert.NoNull(_fontName, "NullButtonFontName");
				if (_style == 0) { this.InferStyle(); }
				
				// load resource string if specified
				if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_resourceKey)) {
					string resx = string.Format("Action_{0}", _resourceKey);
					_text = Idaho.Resource.Say(resx);
					Assert.NoNull(_text, "NullResource", resx);
				}
				arrow = this.InferArrow(ref _text);
				Idaho.Draw.Button draw = new Idaho.Draw.Button(_text, _style);
				draw.Arrow = arrow;
				draw.Width = this.Width;

				// set image properties according to button style
				switch (_style) {
					case Idaho.Draw.Button.Styles.Template:
						draw.TemplateFile = _templateFile;
						draw.TemplateEndPadding = _templateEndPadding;
						break;
				}
				draw.TextWidthPadding = _textEndPadding;
				draw.TextHeightPadding = _textHeightPadding;
				draw.Color.Text = Utility.NoNull<Color>(_textColor, _defaultTextColor);
				draw.Color.ActiveText = Utility.NoNull<Color>(_activeTextColor, _defaultActiveTextColor);

				if (_activeTextOpacity < 100) {
					draw.Color.ActiveText = 
						Idaho.Draw.Utility.AdjustOpacity(draw.Color.ActiveText, _activeTextOpacity);
				}
				if (_textOpacity < 100) {
					draw.Color.Text =
						Idaho.Draw.Utility.AdjustOpacity(draw.Color.Text, _textOpacity);
				}
				if (_style == Draw.Button.Styles.Rounded) {
					draw.Color.Highlight = Utility.NoNull<Color>(_activeTextColor, _defaultHighlightColor);
					draw.Color.Border = Utility.NoNull<Color>(_borderColor, _defaultBorderColor);
				}
				draw.ForceRegeneration = _forceRegeneration;
				draw.FontName = _fontName;
				this.Transparency = true;
				this.Src = draw.Url;
				this.Height = draw.Height;
				// height computed after .Url method runs
			}
			// attributes that aren't cached
			if (!string.IsNullOrEmpty(_url)) {
				int action = (_action != Page.Actions.None) ? (int)_action : 0;
				this.OnClick = Idaho.Resource.SayFormat("Script_Redirect", _url, action);
			} else if (_action != Page.Actions.None) {
				// call script to update hidden field with action
				this.OnClick = Idaho.Resource.SayFormat("Script_FormAction", (int)_action);
			}
			this.Generated = true;
			this.CssClass = "button";
			this.RollOver = Idaho.Resource.Say("Script_ButtonMouseOver");
			if (string.IsNullOrEmpty(this.AlternateText)) {
				this.AlternateText = _text;
				this.Attributes.Add("title", _text);
			}
			base.OnPreRender(e);
		}

		#endregion

		/// <summary>
		/// Infer arrow type and update button text
		/// </summary>
		private Idaho.Draw.Button.ShowArrow InferArrow(ref string text) {
			Idaho.Draw.Button.ShowArrow arrow = Idaho.Draw.Button.ShowArrow.None;
			foreach (string key in _arrowKeys.Keys) {
				if (text.Contains(key)) {
					text = text.Replace(key, "").Trim();
					arrow |= _arrowKeys[key];
				}
			}
			return arrow;
		}

		/// <summary>
		/// Infer button style
		/// </summary>
		private void InferStyle() {
			if (!string.IsNullOrEmpty(_templateFile)) {
				_style = Draw.Button.Styles.Template;
			} else if (!string.IsNullOrEmpty(_textureFile)) {
				_style = Draw.Button.Styles.Pattern;
			} else {
				_style = Draw.Button.Styles.Rounded;
			}
		}
		/// <summary>
		/// Action and IDs to post with form when button is clicked
		/// </summary>
		public void PostOnClick(Page.Actions a, Guid id, Guid subID) {
			this.OnClick = string.Format("Page.Post({0}, {1}, {2})",
				(int)a, this.ScriptGuid(id), this.ScriptGuid(subID));
		}
		private string ScriptGuid(Guid id) {
			return (id == null || id.Equals(Guid.Empty)) ?
				EcmaScript.Null : "'" + id.ToString() + "'";
		}
	}
}