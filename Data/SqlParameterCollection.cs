using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Idaho.Data {
	/// <summary>
	/// Cache SQL parameters
	/// </summary>
	public class SqlParameterCollection : IParameterCollection<SqlParameter, SqlDbType>  {

		//private object _syncLock = new object();
		private static Dictionary<string, List<SqlParameter>> _cache
			= new Dictionary<string, List<SqlParameter>>();
		private SqlCommand _command;
		private bool _reusable = false;
		private bool _ignoreNull = true;
		private bool _ignoreZero = false;
		private bool _nullifyEmptyStrings = true;

		private SqlDbType[] _maybeEmpty = new SqlDbType[]
		  { SqlDbType.VarChar, SqlDbType.UniqueIdentifier };

		public SqlParameter this[string name] {
			get {
				if (!name.StartsWith("@")) { name = "@" + name; }
				return _command.Parameters[name];
			}
		}

		#region Constructors

		internal SqlParameterCollection(SqlCommand command) : this(command, true) { }

		/// <summary>
		/// Build a parameter collection based on the given command
		/// </summary>
		internal SqlParameterCollection(SqlCommand command, bool infer) {
			_command = command;

			if (infer) {
				string key = _command.CommandText;
				_command.Parameters.Clear();

				if (_cache.ContainsKey(key)) {
					// copy parameters to command
					List<SqlParameter> list = this.Clone(_cache[key]);
					foreach (SqlParameter p in list) {
						p.Value = null;
						_command.Parameters.Add(p);
					}
				} else {
					// build parameter cache
					lock (_cache) {
						if (!_cache.ContainsKey(key)) {
							_cache.Add(key, new List<SqlParameter>());
							// db user must be able to execute sp_procedure_params_managed '[proc]'
							SqlCommandBuilder.DeriveParameters(_command);
							foreach (SqlParameter p in _command.Parameters) { _cache[key].Add(p); }
						}
					}
				}
			}
		}

		#endregion

		#region IParameterCollection

		/// <summary>
		/// Reuse the connection for more than one query
		/// </summary>
		public bool Reusable { set { _reusable = value; } }

		/// <summary>
		/// Do not add a parameter object for null values
		/// </summary>
		public bool IgnoreNull { set { _ignoreNull = value; } }

		/// <summary>
		/// Do not add a parameter object for values of zero
		/// </summary>
		public bool IgnoreZero { set { _ignoreZero = value; } }

		/// <summary>
		/// Treat empty strings as DbNull
		/// </summary>
		public bool NullifyEmptyStrings { set { _nullifyEmptyStrings = value; } }


		public int AddReturn() {
			SqlParameter p = new SqlParameter();
			p.SqlDbType = SqlDbType.Int;
			p.Direction = ParameterDirection.ReturnValue;
			_command.Parameters.Add(p);
			return _command.Parameters.Count - 1;
		}
		/// <summary>
		/// Clear all parameters if command is not to be reused
		/// </summary>
		public void Reset() { if (!_reusable) { _command.Parameters.Clear(); } }

		public void Add(string name, object value, SqlDbType type, ParameterDirection direction) {
			if (this.IsValid(value)) {
				SqlParameter p = new SqlParameter('@' + name, type);
				if (value is DateTime) { value = (DateTime)value; }
				p.Value = value;
				p.Direction = direction;
				_command.Parameters.Add(p);
			}
		}

		public void Add(string name, object value, SqlDbType type) {
			this.Add(name, value, type, null);
		}
		/// <summary>
		/// Add parameter for query call
		/// </summary>
		/// <param name="ignoreNull">Do not create parameter object if value is null</param>
		public void Add(string name, object value, SqlDbType type, bool? ignoreNull) {
			if (this.IsValid(value, ignoreNull)) {
				SqlParameter p = new SqlParameter('@' + name, type);
				//if (value is DateTime) { value = (DateTime)value; }
				if (_nullifyEmptyStrings && value is string &&
					string.IsNullOrEmpty((string)value)) { value = null; }
				p.Value = value;
				_command.Parameters.Add(p);
			}
		}

		public void Add(string name, SqlDbType type, ParameterDirection direction) {
			SqlParameter p = new SqlParameter('@' + name, type);
			p.Direction = direction;
			_command.Parameters.Add(p);
		}

		/// <summary>
		/// Is the parameter non-null and non-zero if so required
		/// </summary>
		private bool IsValid(object value, bool? ignoreNull) {
			bool noNull = ignoreNull ?? _ignoreNull;
			if (noNull && (value == null ||
				(value is string && string.IsNullOrEmpty((string)value))) ||
				(value is DateTime && (DateTime)value == DateTime.MinValue)) {

				return false;
			}
			if (_ignoreZero) {
				if (value is int) {
					return (int)value != 0;
				} else if (value is Int16) {
					return (Int16)value != 0;
				} else if (value is Int64) {
					return (Int64)value != 0;
				}
			}
			return true;
		}
		private bool IsValid(object value) { return this.IsValid(value, null); }

		#endregion

		#region Caching

		/// <summary>
		/// Duplicate list of parameter objects
		/// </summary>
		/// <remarks>
		/// A parameter cannot be referenced by more than one command at a time.
		/// To avoid that possibility with cached parameters, they are duplicated
		/// rather than being used directly.
		/// </remarks>
		private List<SqlParameter> Clone(List<SqlParameter> list) {
			List<SqlParameter> newList = new List<SqlParameter>();
			foreach (SqlParameter p in list) {
				newList.Add(new SqlParameter(
					p.ParameterName,
					p.SqlDbType,
					p.Size,
					p.Direction,
					p.IsNullable,
					p.Precision,
					p.Scale,
					p.SourceColumn,
					p.SourceVersion,
					p.Value));
			}
			return newList;
		}

		/// <summary>
		/// Add an array of values to cached parameters
		/// </summary>
		/// <remarks>
		/// The values must be ordered to match the parameters. This approach
		/// is convenient but brittle. The parameter cache must be cleared
		/// if a procedure signature is modified.
		/// </remarks>
		public void Add(object[] values) {
			// subtract one from count to exclude return value
			int parameterCount = _command.Parameters.Count - 1;
			for (int x = 0; x < values.Length && x < parameterCount; x++) {
				this.AssignValue(_command.Parameters[x + 1], values[x]);
			}
		}

		/// <summary>
		/// Add single parameter name/value to cached parameter collection
		/// </summary>
		public void Add(string name, object value) {
			name = "@" + name;
			foreach (SqlParameter p in _command.Parameters) {
				if (p.ParameterName == name) { this.AssignValue(p, value); return; }
			}
			SqlParameterCollection.ClearCache();
			//throw new System.Exception(string.Format(
			//	"Parameter {0} was not found in procedure {1}", name, _command.CommandText));
		}

		/// <summary>
		/// Add parameter object to collection
		/// </summary>
		/// <remarks>
		/// It is not expected that this will be used but is here for
		/// completeness.
		/// </remarks>
		public void Add(SqlParameter p) {
			this.Add(p.ParameterName.TrimStart(new char[] {'@'}), p.Value);
		}

		/// <summary>
		/// Conditionally assign value to parameter
		/// </summary>
		private void AssignValue(SqlParameter p, object value) {
			if (_ignoreNull) {
				if (value == null) { return; }
				if (value is string && ((string)value).Equals(string.Empty)) { return; }
				if (value is Guid && ((Guid)value).Equals(Guid.Empty)) { return; }
			}
			p.Value = value;
		}

		public static void ClearCache() { _cache.Clear(); }

		#endregion

		/// <summary>
		/// Make a well formed SQL statement from command object
		/// </summary>
		/// <remarks>
		/// Used to generate SQL to debug by pasting into a query analyzer.
		/// </remarks>
		public string ToStatement() {
			System.Text.StringBuilder query = new System.Text.StringBuilder();
			string quote = string.Empty;

			query.Append(_command.CommandText);
			query.Append(" ");
			foreach (SqlParameter p in _command.Parameters) {
				quote = (p.SqlDbType == SqlDbType.VarChar) ? "'" : string.Empty;
				query.AppendFormat("{0}={1}{2}{1}, ", p.ParameterName, quote, p.Value);
			}
			return query.ToString().TrimEnd(new char[] { ',' });
		}

	}
}
