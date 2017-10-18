using Idaho.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Idaho {
    /// <summary>
    /// Map an individual object field to a database field (column)
    /// </summary>
	/// <typeparam name="T">The type of entity being mapped</typeparam>
	/// <typeparam name="U">The type of field being mapped</typeparam>
    public class FieldMap<T> where T: Entity, new() {
        private bool _identifier = false;
        private Action<T, ReaderBase> _assignFromDatabase = null;
		private Func<T, object> _getFromObject = null;
        private string _objectField = string.Empty;
        private string _dataField = string.Empty;
		private SqlDbType _dataType = 0;

        #region Properties

		/// <summary>
		/// Is this field the record identifier
		/// </summary>
		public bool IsIdentifier { set { _identifier = value; } get { return _identifier; } }

		/// <summary>
		/// Name of the object field
		/// </summary>
        public string ObjectField { get { return _objectField; } set { _objectField = value; } }

		/// <summary>
		/// Name of the database field
		/// </summary>
        public string DataField { get { return _dataField; } set { _dataField = value; } }

        #endregion

		/// <summary>
		/// Create mapping between an object and database record
		/// </summary>
		/// <param name="fromDb">Delegate to read and assign value from database</param>
		/// <param name="fromObject">Delegate to return value from object</param>
		/// <param name="t">The type of value</param>
        internal FieldMap(Action<T, ReaderBase> fromDb, Func<T, object> fromObject, Type t) {
            _assignFromDatabase = fromDb;
			_getFromObject = fromObject;

			if (t.Equals(typeof(string))) {
				_dataType = SqlDbType.VarChar;
			} else if (t.Equals(typeof(int))) {
				_dataType = SqlDbType.Int;
			} else if (t.Equals(typeof(Guid))) {
				_dataType = SqlDbType.UniqueIdentifier;
			} else if (t.Equals(typeof(DateTime))) {
				_dataType = SqlDbType.DateTime;
			} else if (t.Equals(typeof(long))) {
				_dataType = SqlDbType.BigInt;
			}
        }
		internal void Assign(T e, ReaderBase r) {
			_assignFromDatabase(e, r);
		}
		
		/// <summary>
		/// Create SQL parameter for this field mapping
		/// </summary>
		/// <param name="sql">A SQL connection</param>
		/// <param name="e">Entity being mapped</param>
		internal void ToParameter(Sql sql, T e) {
			sql.Parameters.Add(_dataField, _getFromObject(e), _dataType);
		}

    }
}
