using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Idaho.Data {
	/// <summary>
	/// Base class for relational data connections
	/// </summary>
	/// <typeparam name="P">Type of parameter object</typeparam>
	/// <typeparam name="T">Parameter type enumeration</typeparam>
	public abstract class Relational<P, T> : IDisposable where P : System.Data.Common.DbParameter,
			IDbDataParameter, IDataParameter, new() {

		private bool _reuseConnection = false;
		private ParameterCollection _parameters = null;
		private IDbCommand _command = null;
		private IDbConnection _connection = null;
		private IDbDataAdapter _adapter = null;
	   
		#region Properties

		public virtual IParameterCollection<P, T> Parameters { get { return _parameters; } }
		protected internal IDbCommand Command { get { return _command; } }
	    protected IDbConnection Connection { get { return _connection; } set { _connection = value; } }
		protected IDbDataAdapter Adapter { get { return _adapter; } set { _adapter = value; } }
		public bool ReuseConnection { get { return _reuseConnection; } set { _reuseConnection = value; } }
		public virtual string ProcedureName {
			set {
				_command.CommandType = CommandType.StoredProcedure;
				_command.CommandText = value; // "EyeRegard.dbo." + 
			}
		}
		/// <summary>
		/// Query to run
		/// </summary>
		public string Query {
			set {
				_command.CommandType = CommandType.Text;
				_command.CommandText = value;
			}
		}
		/// <summary>
		/// How long to attempt query execution
		/// </summary>
		public TimeSpan TimeoutAfter {
			set { _command.CommandTimeout = value.Seconds; }
			get { return TimeSpan.FromSeconds(_command.CommandTimeout); }
		}
	   
		#endregion

		#region Constructors

		protected Relational(IDbConnection connection, IDbCommand command, IDbDataAdapter adapter) {
			_command = command;
			_adapter = adapter;
			_connection = connection;
		}

		/// <summary>
		/// Construct with parameter delegate
		/// </summary>
		protected Relational(IDbConnection connection, IDbCommand command, IDbDataAdapter adapter,
			ParameterCollection.SetType setType) : this(connection, command, adapter) {

			_parameters = new ParameterCollection(this, setType);
		}

		#endregion

		protected void Connect(string connectionString, bool log) {
			try {
				_connection.ConnectionString = connectionString;
				_connection.Open();
				_command.Connection = _connection;
			} catch (System.Exception ex) {
				this.Finish(true);
				if (log) { this.LogError(ex); } else { throw ex; }
			}
		}
		protected void Connect(string connectionString) {
			this.Connect(connectionString, false);
		}
	   
		#region GetDataTable
	   
		public DataTable GetDataTable() {
			DataSet ds = new DataSet();
			this.Adapter.SelectCommand = _command;
			try { this.Adapter.Fill(ds); } finally { this.Dispose(); }
			return ds.Tables[0];
		}
		public DataTable GetDataTable(string sqlStatement) {
			this.Query = sqlStatement;
			return this.GetDataTable();
		}
	   
		#endregion
	   
		#region GetDataSet
	   
		public DataSet GetDataSet() {
			DataSet dataSet = new DataSet();
			this.Adapter.SelectCommand = _command;
			try { this.Adapter.Fill(dataSet); } finally { this.Dispose(); }
			return dataSet;
		}
	   
		public DataSet GetDataSet(string sqlStatement) {
			this.Query = sqlStatement;
			return this.GetDataSet();
		}
	   
		#endregion
	   
		#region GetReader
	   
		/// <summary>
		/// Get a reader object
		/// </summary>
		/// <remarks>
		/// Must leave connection open or the reader can't call .Finish()
		/// but set CommandBehavior to automatically close when reader is closed.
		/// </remarks>
		public Reader GetReader(bool log) {
			CommandBehavior behavior = (_reuseConnection) ? CommandBehavior.Default : CommandBehavior.CloseConnection;
			_command.Connection = _connection;

			try {
				return new Reader(_command.ExecuteReader(behavior));
			} catch (System.Exception ex) {
				this.Finish(true);
				if (log) { this.LogError(ex); } else { throw ex; }
			}
			return null;
		}
		public Reader GetReader() {
			return this.GetReader(false);
		}
		public Reader GetReader(string sqlStatement) {
			this.Query = sqlStatement;
			return this.GetReader();
		}
	   
		#endregion
	   
		#region GetSingleValue
	   
		/// <summary>
		/// Get single select value
		/// </summary>
		/// <returns></returns>
		public Q GetSingleValue<Q>() {
			_command.Connection = _connection;
			object value;
			try { value = _command.ExecuteScalar(); } finally { this.Dispose(); }
			return (Q)value;
		}
		public Q GetSingleValue<Q>(string sqlStatement) {
			this.Query = sqlStatement;
			return this.GetSingleValue<Q>();
		}
	   
		#endregion
	   
		#region Exists
	   
		/// <summary>
		/// Does query have any results
		/// </summary>
		public bool Exists() {
			return this.GetDataTable().Rows.Count > 0;
		}
		public bool Exists(string sqlStatement) {
			this.Query = sqlStatement;
			return this.Exists();
		}

		#endregion
		
		#region ExecuteOnly

		/// <summary>
		/// Execute a command with no results or output/return value only
		/// </summary>
		/// <returns>Rows affected (-1 for error)</returns>
		public int ExecuteOnly() {
			return this.ExecuteOnly(false);
		}
		public int ExecuteOnly(bool log) {
			int affected = -1;

			try {
				affected = _command.ExecuteNonQuery();
				// ExecuteNonQuery() returns -1 for SELECTs or NO COUNTs
				// which we will treat as 0 letting -1 be the error condition
				if (affected < 0) { affected = 0; }
				this.Dispose();
			} catch (System.Exception ex) {
				if (log) {
					this.LogError(ex);
					// log before finishing to allow statement inference
					this.Finish();
				} else {
					this.Finish();
					throw ex;
				}
			}
			return affected;
		}
		public int ExecuteOnly(string sqlStatement) {
			this.Query = sqlStatement;
			return this.ExecuteOnly();
		}

		#endregion

		private void LogError(System.Exception ex) {
			if (_parameters != null) {
				Idaho.Exception.Log(ex, Exception.Types.Database, _parameters.ToStatement());
			} else {
				Idaho.Exception.Log(ex, Exception.Types.Database);
			}
		}

		#region GetReturn

		/// <summary>
		/// Retrieve return value from command
		/// </summary>
		/// <param name="log">Catch and log errors</param>
		public int GetReturn(bool log) {
			int index = this.Parameters.AddReturn();
			IDbDataParameter p = null;
			_command.Connection = this.Connection;

			try {
				_command.ExecuteNonQuery();
				p = _command.Parameters[index] as IDbDataParameter;
			} catch (System.Exception ex) {
				if (log) { this.LogError(ex); } else { throw ex; }
			} finally {
				this.Finish();
			}
			return (p != null) ? Convert.ToInt32(p.Value) : 0;
		}
		public int GetReturn() {
			return this.GetReturn(false);
		}
		public int GetReturn(string sqlStatement) {
			this.Query = sqlStatement;
			return this.GetReturn();
		}

		#endregion

		#region Parameters

		/// <summary>
		/// Nested collection of command parameter members
		/// </summary>
		/// <remarks>
		/// Nested in order to take advantage of common P and T
		/// </remarks>
		public class ParameterCollection : IParameterCollection<P, T> {

			private bool _ignoreNull = false;
			private bool _ignoreZero = false;
			private bool _reusable = false;
			private bool _nullifyEmptyStrings = true;
			private Idaho.Data.Relational<P, T> _db;
			public delegate void SetType(ref P parameter, T type);
			private SetType _setType;

			#region Properties

			public P this[string name] { get { return _db.Command.Parameters[name] as P; } }
			public bool IgnoreNull { set { _ignoreNull = value; } }
			public bool IgnoreZero { set { _ignoreZero = value; } }
			public bool NullifyEmptyStrings { set { _nullifyEmptyStrings = value; } }

			#endregion

			#region IParameterCollection

			public bool Reusable {
				set { _reusable = value; if (_reusable) { _db.ReuseConnection = true; } }
			}

			public int AddReturn() {
				int index = 0;
				P p = new P();
				p.Direction = ParameterDirection.ReturnValue;
				//TODO: parameter.OleDbType = OleDbType.Integer;
				p.DbType = DbType.Int32;
				index = _db.Command.Parameters.Count;
				_db.Command.Parameters.Add(p);
				return index;
			}

			public void Reset() {
				if (!_reusable) { _db.Command.Parameters.Clear(); }
			}

			#endregion
			
			/// <param name="db">Instace of DAL for reference to command object</param>
			/// <param name="setType">Delegate to set type on parameter object</param>
			/// <remarks>
			/// A delegate is needed to set the parameter type since the generic reference
			/// P doesn't have provider-specific members or type enumerations.
			/// </remarks>
			internal ParameterCollection(Idaho.Data.Relational<P, T> db, SetType setType) {
				_db = db; _setType = setType;
			}

			public void Add(P p) { _db.Command.Parameters.Add(p); }

			/// <summary>
			/// Add parameters to query
			/// </summary>
			/// <param name="name">Parameter name</param>
			/// <param name="value">Parameter value</param>
			/// <param name="type">Parameter type</param>
			/// <param name="direction">Parameter direction</param>
			public void Add(string name, object value, T type, ParameterDirection direction) {

				if (_ignoreNull && value == null) { return; }
				if (_ignoreZero && value.Equals(0)) { return; }
				if (_nullifyEmptyStrings && value is string &&
					string.IsNullOrEmpty((string)value)) { value = null; }

				P parameter = new P();
				parameter.ParameterName = name;
				parameter.Value = value;
				parameter.Direction = direction;
				_setType(ref parameter, type);
				_db.Command.Parameters.Add(parameter);
			}
			public void Add(string name, object value, T type) {
				this.Add(name, value, type, ParameterDirection.Input);
			}
			public void Add(string name, T type, ParameterDirection direction) {
				P parameter = new P();
				parameter.ParameterName = name;
				parameter.Direction = direction;
				_setType(ref parameter, type);
				_db.Command.Parameters.Add(parameter);
			}


			/// <summary>
			/// Make a well formed SQL statement from command object
			/// </summary>
			/// <remarks>
			/// Used to generate SQL to debug by pasting into a query analyzer.
			/// </remarks>
			public string ToStatement() {
				System.Text.StringBuilder query = new System.Text.StringBuilder();
				string quote = string.Empty;

				query.Append(_db.Command.CommandText);
				query.Append(" ");
				foreach (P p in _db.Command.Parameters) {
					quote = (p.DbType == DbType.String) ? "'" : string.Empty;
					query.AppendFormat("{0}={1}{2}{1}, ", p.ParameterName, quote, p.Value);
				}
				query.TrimEnd();
				return query.ToString();
			}

			#region IParameterCollection not implemented

			public void Add(string name, object value) {
				throw new System.Exception("The method or operation is not implemented.");
			}
			public void Add(string name, object value, T type, bool? allowNull) {
				throw new System.Exception("The method or operation is not implemented.");
			}

			#endregion

		}

		#endregion

		#region IDisposable

		/// <summary>
		/// Close current data objects
		/// </summary>
		public void Dispose() {
			if (this.Parameters != null) { this.Parameters.Reset(); }
			if (!_reuseConnection) {
				// free the ADO objects
				_command.Dispose();
				if (_connection.State != ConnectionState.Closed) { _connection.Close(); }
				_connection.Dispose();
			}
		}

		public void Finish(bool closeConnection) {
			if (closeConnection) {
				_reuseConnection = false;
				if (this.Parameters != null) { this.Parameters.Reusable = false; }
			}
			this.Dispose();
		}
		public void Finish() { this.Finish(true); }

		#endregion

	}
}