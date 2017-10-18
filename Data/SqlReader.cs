using Idaho.Network;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;

namespace Idaho.Data {
	public class SqlReader : ReaderBase, IDataReader {

		public SqlReader(IDataReader reader) : base((SqlDataReader)reader) { }

		/// <summary>
		/// SQL Boolean fields are structured differently, it seems
		/// </summary>
		public override bool GetBoolean(string name, bool ifNull) {
			SqlBoolean b = ((SqlDataReader)this.DataReader).GetSqlBoolean(this.GetOrdinal(name));
			return (b.IsNull) ? ifNull : b.IsTrue;
		}
	}
}