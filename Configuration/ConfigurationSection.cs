using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Idaho.Configuration {
	// http://www.codeproject.com/KB/dotnet/mysteriesofconfiguration2.aspx?display=PrintAll
	// http://olondono.blogspot.com/2008/01/advanced-net-configuration-techniques.html
	// http://forums.msdn.microsoft.com/en-US/clr/thread/6faf9c70-162c-499b-8d0c-0b1f19c7a24a/
	// http://www.devnewsgroups.net/group/microsoft.public.dotnet.framework/topic38369.aspx
	// http://www.devnewsgroups.net/group/microsoft.public.dotnet.framework/topic38702.aspx
	[Obsolete]
	public class Section : System.Configuration.ConfigurationSection {

		protected TimeSpan TimeSpanFromSetting(string key) {
			int seconds;
			if (int.TryParse(this[key] as string, out seconds)) {
				return TimeSpan.FromSeconds(seconds);
			} else {
				return TimeSpan.Zero;
			}
		}
	}
}