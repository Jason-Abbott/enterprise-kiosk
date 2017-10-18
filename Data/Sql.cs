using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Idaho.Data {
	/// <summary>
	/// Dim ConnStr as string = "Data Source=.\SQLExpress; Initial Catalog=; Integrated Security=true; AttachDBFileName=" & Application.StartupPath & "\MyDb.mdf "
	/// </summary>
	/// <example>http://www.code-magazine.com/article.aspx?quickid=0605061&page=2</example>
	public class Sql : Relational<SqlParameter, SqlDbType> {

		private SqlParameterCollection _parameters;

		#region Static Fields

		private static string _defaultConnectionString = string.Empty;
		private static bool _inferParameters = false;
		public static string DefaultConnectionString { set { _defaultConnectionString = value; } }
		
		/// <summary>
		/// Should the command object infer and cache command parameters
		/// </summary>
		public static bool InferParameters { set { _inferParameters = value; } }

		#endregion

		#region Properties

		public override IParameterCollection<SqlParameter, SqlDbType> Parameters {
			get {
				if (_parameters == null) {
					_parameters = new SqlParameterCollection(base.Command as SqlCommand, _inferParameters);
				}
				return _parameters as SqlParameterCollection;
			}
		}

		/// <remarks>
		/// Override so parameter cache can be initialized per procedure
		/// </remarks>
		public override string ProcedureName {
			set {
				base.ProcedureName = value;
				/*
				if (_inferParameters) {
					// prepare parameters as soon as procedure name is supplied
					if (_parameters == null) {
						_parameters = new SqlParameterCollection(base.Command as SqlCommand);
					}
				}
				*/
			}
		}
	   
		#endregion
	   
		#region Constructors

		public Sql() : this(string.Empty) { } 
		public Sql(string connectionString)
			: base(new SqlConnection(), new SqlCommand(), new SqlDataAdapter()) {

			connectionString = Utility.NoNull<string>(connectionString, _defaultConnectionString);
			Assert.NoNull(connectionString, "NoDbConnection");
			this.Connect(connectionString);
		}
		public Sql(string procedureName, object[] values) : this(string.Empty) {
			this.ProcedureName = procedureName;
			_parameters.Add(values);
		}
		public Sql(string procedureName, object value)
			: this(procedureName, new object[] { value }) { }
	   
		#endregion

		#region GetReader

		/// <summary>
		/// Get a reader object
		/// </summary>
		/// <remarks>
		/// Must leave connection open or the reader can't call .Finish()
		/// but set CommandBehavior to automatically close when reader is closed.
		/// </remarks>
		public new SqlReader GetReader(bool log) {
			CommandBehavior behavior = (base.ReuseConnection) ?
				CommandBehavior.Default : CommandBehavior.CloseConnection;
			base.Command.Connection = base.Connection;
			try {
				return new SqlReader(base.Command.ExecuteReader(behavior));
			} catch (System.Exception ex) {
				this.Finish(true);
				if (log) { this.LogError(ex); } else { throw ex; }
			}
			return null;
		}
		public new SqlReader GetReader() {
			return this.GetReader(false);
		}
		public new SqlReader GetReader(string sqlStatement) {
			this.Query = sqlStatement;
			return this.GetReader();
		}

		#endregion

		private void LogError(System.Exception ex) {
			if (_parameters != null) {
				Idaho.Exception.Log(ex, Exception.Types.Database, _parameters.ToStatement());
			} else {
				Idaho.Exception.Log(ex, Exception.Types.Database);
			}
		}

	}
}