using Idaho.Network;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Idaho.Data {
	public class Reader : ReaderBase, IDataReader {
		public Reader(IDataReader reader) : base(reader) { }
	}
}