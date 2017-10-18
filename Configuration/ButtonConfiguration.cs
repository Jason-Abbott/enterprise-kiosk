using Idaho.Attributes;
using Idaho.Draw;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace Idaho.Configuration {
	/// <summary>
	/// Configuration for a drawn button
	/// </summary>
	/// <see cref="Idaho.Draw.Button"/>
	public class Button : ConfigurationSection, ISelfLoad<Idaho.Web.Controls.Button> {

		internal Button() { }

		public static Button Current {
			get { return WebConfigurationManager.GetSection("button") as Button; }
		}

		/// <summary>
		/// Assemblies with resources to be used
		/// </summary>
		[ConfigurationProperty("buttons", IsRequired = true, IsDefaultCollection = true)]
		public ButtonElementCollection Buttons {
			get { return (ButtonElementCollection)this["buttons"]; }
		}

		#region ISelfLoad

		/// <summary>
		/// Load settings and apply them to object properties
		/// </summary>
		public void ApplySettings() {
			ButtonElement b = this.Buttons.Default;
			if (b != null) {
				// set static properties for defaults
				Web.Controls.Button.DefaultHeight = b.Height;
				Web.Controls.Button.DefaultTemplateFile = b.Template.Path;
				Web.Controls.Button.DefaultTemplateEndPadding = b.Template.EndPadding;
				Web.Controls.Button.DefaultTextHeightPadding = b.Font.HeightPadding;
				Web.Controls.Button.DefaultTextWidthPadding = b.Font.EndPadding;
				Web.Controls.Button.DefaultFontName = b.Font.Name;
				Web.Controls.Button.DefaultActiveTextColor = b.Font.MouseOver.On.Color;
				Web.Controls.Button.DefaultTextColor = b.Font.MouseOver.Off.Color;
			}
		}

		public bool ApplySettings(string key, Web.Controls.Button button) {
			if (!string.IsNullOrEmpty(key)) {
				ButtonElement b = this.Buttons[key];
				if (b != null) {
					button.Height = b.Height;
					button.FontName = b.Font.Name;
					button.TextColor = b.Font.MouseOver.Off.Color;
					button.ActiveTextColor = b.Font.MouseOver.On.Color;
					button.TextEndPadding = b.Font.EndPadding;
					button.TextHeightPadding = b.Font.HeightPadding;
					// config colors already have opacity applied
					button.TextOpacity = 100;
					button.ActiveTextOpacity = 100;
					// style
					button.GraphicStyle = b.Style;
					switch (b.Style) {
						case Idaho.Draw.Button.Styles.Template:
							button.TemplateFile = b.Template.Path;
							button.TemplateEndPadding = b.Template.EndPadding;
							break;
					}
					return true;
				}
			}
			return false;
		}

		#endregion

	}
}
