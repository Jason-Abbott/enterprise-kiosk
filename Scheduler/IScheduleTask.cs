using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho.Scheduler {
	public interface ITask {
		bool Execute();
	}
}
