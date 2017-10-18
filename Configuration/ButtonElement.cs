using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web;

namespace Idaho.Configuration {
	/// <summary>
	/// Set button properties
	/// </summary>
	public class ButtonElement : ConfigurationElement {

		/// <summary>
		/// Arbitrary name to serve as key
		/// </summary>
		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public string Name { get { return this["name"] as string; } }

		/// <summary>
		/// Is this the default button configuration
		/// </summary>
		[ConfigurationProperty("isDefault")]
		public bool IsDefault { get { return (bool)this["isDefault"]; } }

		/// <summary>
		/// Font used to render button text
		/// </summary>
		[ConfigurationProperty("font", IsRequired = false)]
		public FontElement Font { get { return (FontElement)this["font"]; } }

		/// <summary>
		/// Enumerated button style name
		/// </summary>
		[ConfigurationProperty("style", IsRequired = true)]
		//[EnumValidator(typeof(Idaho.Draw.Button.Styles))]
		private string StyleName { get { return this["style"] as string; } }

		/// <summary>
		/// Pixel height of the button
		/// </summary>
		[ConfigurationProperty("height", IsRequired = true)]
		//[IntegerValidator(MinValue = 5, MaxValue = 200)]
		public int Height { get { return (int)this["height"]; } set { this["height"] = value; } }

		/// <summary>
		/// Pixel width of the button
		/// </summary>
		/// <remarks>This only applies to certain button types.</remarks>
		[ConfigurationProperty("width", IsRequired = false)]
		//[IntegerValidator(ExcludeRange = false, MinValue = 20, MaxValue = 600)]
		public int Width { get { return (int)this["width"]; } set { this["width"] = value; } }

		/// <summary>
		/// Button template if that's the specified style
		/// </summary>
		[ConfigurationProperty("template", IsRequired = false)]
		public TemplateElement Template { get { return (TemplateElement)this["template"]; } }

		/// <summary>
		/// Enumerated button style
		/// </summary>
		public Idaho.Draw.Button.Styles Style {
			get { return this.StyleName.ToEnum<Idaho.Draw.Button.Styles>(); }
		}

		/// <summary>
		/// Settings for button template if that's the specified style
		/// </summary>
		public class TemplateElement : ConfigurationElement {
			[ConfigurationProperty("path", IsRequired = true)]
			[UriValidator(UriKind.RelativeOrAbsolute)]
			public string Path {
				get {
					string path = this["path"] as string;
					if (!string.IsNullOrEmpty(path)) {
						path = HttpContext.Current.Server.MapPath(path);
					}
					return path;
				}
				set { this["path"] = value; }
			}

			[ConfigurationProperty("endPadding", IsRequired = true)]
			[IntegerValidator(ExcludeRange = false, MinValue = 0, MaxValue = 200)]
			public int EndPadding {
				get { return (int)this["endPadding"]; }
				set { this["endPadding"] = value; }
			}
		}
	}
}
