using Idaho.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Idaho {
	/// <summary>
	/// Map object domain members to database fields
	/// </summary>
	/// <typeparam name="T">The type of entity being mapped</typeparam>
	public class ObjectMap<T> : List<FieldMap<T>> where T: Entity, new() {

		private Procedures _procedures = new Procedures();
		private bool _transactional = false;

		#region Properties

		public FieldMap<T> IdentityField {
			get { return this.Find(f => f.IsIdentifier); }
		}

		/// <summary>
		/// Stored procedures used for database interaction
		/// </summary>
		public Procedures Procedure { get { return _procedures; } }

		/// <summary>
		/// Should save and delete operations be transactional
		/// </summary>
		public bool Transactional { set { _transactional = value; } }

		#endregion

		#region Constructors

		protected ObjectMap() : this(true) { }

		protected ObjectMap(bool useDefaultFields) {
			if (useDefaultFields) {
				this.Add(e => e.ID, (e, r) => e.ID = r.GetGuid("ID"), "ID");
			}
		}

		#endregion

		#region Mappings

		/// <summary>
		/// Infer reader method and field names from expression
		/// </summary>
		/// <param name="e">Function expression that returns property to be mapped</param>
		/// <param name="f">Database field name if different from object property name</param>
		public void Add(Expression<Func<T, string>> e, string f) {
			this.Add<string>(e, (r, n) => r.GetString(n), f);
		}
		public void Add(Expression<Func<T, string>> e) {
			this.Add<string>(e, (r, n) => r.GetString(n));
		}
		public void Add(Expression<Func<T, int>> e) {
			this.Add<int>(e, (r, n) => r.GetInt32(n));
		}
		public void Add(Expression<Func<T, DateTime>> e) {
			this.Add<DateTime>(e, (r, n) => r.GetDateTime(n));
		}
		public void Add(Expression<Func<T, Guid>> e) {
			this.Add<Guid>(e, (r, n) => r.GetGuid(n));
		}
		public void Add(Expression<Func<T, long>> e) {
			this.Add<long>(e, (r, n) => r.GetInt64(n));
		}
		public void Add(Expression<Func<T, short>> e) {
			this.Add<short>(e, (r, n) => r.GetInt16(n));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="U">Data type of the mapped field</typeparam>
		/// <param name="e">Function that returns object property</param>
		/// <param name="r">Function that returns database field value</param>
		private void Add<U>(Expression<Func<T, U>> e, Func<ReaderBase, string, U> r) {
			var p = this.GetProperty(e);
			Func<T, U> getFromObject = e.Compile();
			Action<T, ReaderBase> assignFromDb =
				(o, reader) => p.SetValue(o, r(reader, p.Name), null);

			this.Add<U>(getFromObject, assignFromDb, p.Name);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="U">Data type of the mapped field</typeparam>
		/// <param name="e">Function that returns object property</param>
		/// <param name="r">Function that returns database field value</param>
		/// <param name="f">Database field name</param>
		private void Add<U>(Expression<Func<T, U>> e, Func<ReaderBase, string, U> r, string f) {
			var p = this.GetProperty(e);
			Func<T, U> getFromObject = e.Compile();
			Action<T, ReaderBase> assignFromDb =
				(o, reader) => p.SetValue(o, r(reader, f), null);

			this.Add<U>(getFromObject, assignFromDb, p.Name, f);
		}

		/// <summary>
		/// Create a mapping between object and database fields
		/// </summary>
		/// <param name="a">Function that assigns database value to object property</param>
		/// <param name="f">Object and database field name (must be the same)</param>
		public void Add<U>(Func<T, U> getFromObject, Action<T, ReaderBase> assignFromDb, string f) {
			this.Add<U>(getFromObject, assignFromDb, f, f);
        }

		/// <summary>
		/// Create a mapping between object and database fields
		/// </summary>
		/// <param name="assignFromDb">Delegate that assigns database value to object property</param>
		public void Add<U>(Func<T, U> getFromObject, Action<T, ReaderBase> assignFromDb,
			string objectField, string dataField) {

			FieldMap<T> m = new FieldMap<T>(assignFromDb, getFromObject as Func<T, object>, typeof(U)) {
				DataField = dataField,
				ObjectField = objectField
			};
            this.Add(m);
        }

		/// <summary>
		/// Get property information from an expression that returns that property
		/// </summary>
		/// <typeparam name="U">Datatype of the field being mapped</typeparam>
		/// <param name="e">Expression of a function returning the object field</param>
		private PropertyInfo GetProperty<U>(Expression<Func<T, U>> e) {
			if (e.Body.NodeType == ExpressionType.MemberAccess) {
				var x = e.Body as MemberExpression;
				if (x != null) { return (PropertyInfo)x.Member; }
			}
			return null;
		}


		/*
		public static PropertyInfo GetProperty<T>(Expression<Func<T, object>> expression) {
			var isExpressionOfDynamicComponent = expression.ToString().Contains("get_Item");

			if (isExpressionOfDynamicComponent)
				return GetDynamicComponentProperty(expression);

			var memberExpression = getMemberExpression(expression);

			return (PropertyInfo)memberExpression.Member;
		}

		private static PropertyInfo GetDynamicComponentProperty<MODEL>(Expression<Func<MODEL, object>> expression) {
			Type desiredConversionType = null;
			MethodCallExpression methodCallExpression = null;
			var nextOperand = expression.Body;

			while (nextOperand != null) {
				if (nextOperand.NodeType == ExpressionType.Call) {
					methodCallExpression = nextOperand as MethodCallExpression;
					break;
				}

				if (nextOperand.NodeType != ExpressionType.Convert)
					throw new ArgumentException("Expression not supported", "expression");

				var unaryExpression = (UnaryExpression)nextOperand;
				desiredConversionType = unaryExpression.Type;
				nextOperand = unaryExpression.Operand;
			}

			var constExpression = methodCallExpression.Arguments[0] as ConstantExpression;

			return new DummyPropertyInfo((string)constExpression.Value, desiredConversionType);
		}


		private static MemberExpression getMemberExpression<MODEL, T>(Expression<Func<MODEL, T>> expression) {
			return getMemberExpression(expression, true);
		}

		private static MemberExpression getMemberExpression<MODEL, T>(Expression<Func<MODEL, T>> expression, bool enforceCheck) {
			MemberExpression memberExpression = null;
			if (expression.Body.NodeType == ExpressionType.Convert) {
				var body = (UnaryExpression)expression.Body;
				memberExpression = body.Operand as MemberExpression;
			} else if (expression.Body.NodeType == ExpressionType.MemberAccess) {
				memberExpression = expression.Body as MemberExpression;
			}

			if (enforceCheck && memberExpression == null) {
				throw new ArgumentException("Not a member access", "member");
			}

			return memberExpression;
		}

		*/
/*
		protected void Map<R>(Func<T, R> action) {
			var m = action.Method;
			var b = m.GetMethodBody();
			//b.

			
		}
*/
        /*
		protected void Map(string name, Action<T, string> action) {
			_maps.Add((e, r) => action(e, r.GetString(name)));
		}
		protected void Map(string name, Action<T, Guid> action) {
			_maps.Add((e, r) => action(e, r.GetGuid(name)));
		}
		protected void Map(string name, Action<T, int> action) {
			_maps.Add((e, r) => action(e, r.GetInt32(name)));
		}
		protected void Map(string name, Action<T, DateTime> action) {
			_maps.Add((e, r) => action(e, r.GetDateTime(name)));
		}
         */

		#endregion

		/// <summary>
		/// Load collection of entities
		/// </summary>
		/// <typeparam name="U">Collection of T entities</typeparam>
		public virtual U Load<U>() where U: EntityCollection<T>, new() {
			U all = new U();
			T i = null;
			Sql sql = new Sql() { ProcedureName = _procedures.LoadAll };
			SqlReader reader = sql.GetReader(true);

			while (reader.Read()) {
				i = new T();
				this.ForEach(m => m.Assign(i, reader));
				all.Add(i);
			}
			sql.Finish();
			return all;
		}

		/// <summary>
		/// Load a particular entity
		/// </summary>
		public virtual T Load() {
			T i = null;
			Sql sql = new Sql() { ProcedureName = _procedures.LoadOne };
			SqlReader reader = sql.GetReader(true);

			if (reader.Read()) {
				i = new T();
				this.ForEach(m => m.Assign(i, reader));
			}
			sql.Finish();
			return i;
		}

		/// <summary>
		/// Save the entity to a data store
		/// </summary>
		public virtual bool Save(T e) { return this.Save(e, _transactional); }

		/// <summary>
		/// Save the entity to a data store
		/// </summary>
		public virtual bool Save(T e, bool transactional) {
			Sql sql = new Sql() { ProcedureName = _procedures.Save };

			this.ForEach(m => m.ToParameter(sql, e));

			if (transactional) { e.BeginEdit(); }

			if (sql.ExecuteOnly(true) > -1) {
				if (transactional) { e.EndEdit(); }
				return true;
			} else {
				if (transactional) { e.CancelEdit(); }
				return false;
			}
		}

		/// <summary>
		/// Delete the entity from the data store
		/// </summary>
		public virtual bool Delete(T e) {
			Sql sql = new Sql() { ProcedureName = _procedures.Delete };
			this.IdentityField.ToParameter(sql, e);
			return sql.ExecuteOnly(true) > -1;
		}

		public class Procedures {
			public string LoadAll { get; set; }
			public string LoadOne { get; set; }
			public string Save { get; set; }
			public string Delete { get; set; }
		}
	}
}
