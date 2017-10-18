using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Text;

namespace Idaho.Configuration {
	public class FontElement : ConfigurationElement {

		[ConfigurationProperty("name", DefaultValue = "Tahoma", IsRequired = true)]
		[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
		public string Name {
			get { return this["name"] as string; }
			set { this["name"] = value; }
		}

		[ConfigurationProperty("size", DefaultValue = 12, IsRequired = false)]
		[IntegerValidator(ExcludeRange = false, MinValue = 1, MaxValue = 200)]
		public int Size {
			get { return (int)this["size"]; }
			set { this["size"] = value; }
		}

		[ConfigurationProperty("endPadding", DefaultValue = 0, IsRequired = false)]
		[IntegerValidator(ExcludeRange = false, MinValue = 0, MaxValue = 50)]
		public int EndPadding {
			get { return (int)this["endPadding"]; }
			set { this["endPadding"] = value; }
		}

		[ConfigurationProperty("padHeight", DefaultValue = "0", IsRequired = false)]
		[IntegerValidator(ExcludeRange = false, MinValue = 0, MaxValue = 50)]
		public int HeightPadding {
			get { return (int)this["padHeight"]; }
			set { this["padHeight"] = value; }
		}


		[ConfigurationProperty("style", DefaultValue = "Regular", IsRequired = false)]
		[EnumValidator(typeof(System.Drawing.FontStyle))]
		private string StyleName {
			get { return this["style"] as string; }
			set { this["style"] = value; }
		}

		[ConfigurationProperty("mouseOver", IsRequired = false)]
		public MouseOverElement MouseOver {
			get { return (MouseOverElement)this["mouseOver"]; }
			set { this["mouseOver"] = value; }
		}

		public FontStyle Style {
			get {
				string s = this.StyleName;
				return (string.IsNullOrEmpty(s)) ? FontStyle.Regular : s.ToEnum<FontStyle>();
			} set {
				this.StyleName = value.ToString();
			}
		}
	}
}
