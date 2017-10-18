using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	public interface IObjectRelationMap<T> where T : class {
		bool Delete();
		bool Save();
	}
}
