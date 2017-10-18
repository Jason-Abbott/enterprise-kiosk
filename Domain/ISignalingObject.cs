using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	/// <summary>
	/// For objects that have dependencies signaled through raised events
	/// </summary>
	public interface ISignalingObject {
		event EventHandler Dependency;
		void Changed();
	}
}
