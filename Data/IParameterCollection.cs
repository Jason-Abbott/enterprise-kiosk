using System;
using System.Collections.Generic;
using System.Data;

namespace Idaho.Data {
	/// <summary>
	/// Define members for parameter collection
	/// </summary>
	/// <typeparam name="T">Parameter data type</typeparam>
	public interface IParameterCollection<P, T> where P : System.Data.Common.DbParameter,
		IDbDataParameter, IDataParameter, new() {

		P this[string name] { get; }
		int AddReturn();
		void Reset();
		bool Reusable { set; }
		
		/// <summary>
		/// Do not create a parameter object for null values
		/// </summary>
		bool IgnoreNull { set; }

		/// <summary>
		/// Do not create a parameter object for zero
		/// </summary>
		bool IgnoreZero { set; }

		/// <summary>
		/// Insert DbNull for empty strings
		/// </summary>
		bool NullifyEmptyStrings { set; }

		void Add(string name, object value, T type, ParameterDirection direction);
		void Add(string name, object value, T type, Nullable<bool> ignoreNull);
		void Add(string name, object value, T type);
		void Add(string name, object value);
		void Add(string name, T type, ParameterDirection direction);
		void Add(P p);

		string ToStatement();
	}
}
