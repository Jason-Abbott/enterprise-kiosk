using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Text;

namespace Idaho.Configuration {
	public class ColorElement : ConfigurationElement {

		[ConfigurationProperty("color", DefaultValue = "FFF", IsRequired = true)]
		[PatternValidator("HexColor")]
        private String ColorHex {
            get { return this["color"] as string; } set { this["color"] = value; }
		}

		[ConfigurationProperty("opacity", DefaultValue = "100", IsRequired = false)]
        [IntegerValidator(ExcludeRange = false, MinValue = 0, MaxValue = 100)]
        private int Opacity {
            get { return (int)this["opacity"]; } set { this["color"] = value; }
        }

		public Color Color {
			get {
				string h = this.ColorHex;
				int o = this.Opacity;

				if (string.IsNullOrEmpty(h)) {
					return Color.Empty;
				} else {
					if (!h.StartsWith("#")) { h = "#" + h; }
					Color c = ColorTranslator.FromHtml(h);
					return Draw.Utility.AdjustOpacity(c, this.Opacity);
				}
			}
			set {
				this.ColorHex = value.ToString();
			}
		}
	}
}
