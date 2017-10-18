using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Text;

namespace Idaho.Configuration {
	/// <summary>
	/// Define HTML or graphic values for mouse-over
	/// </summary>
	public class MouseOverElement : ConfigurationElement {

		[ConfigurationProperty("off", IsRequired = true)]
        public ColorElement Off {
            get { return (ColorElement)this["off"]; }
		}

		[ConfigurationProperty("on", IsRequired = true)]
        public ColorElement On {
            get { return (ColorElement)this["on"]; }
		}
	}
}
