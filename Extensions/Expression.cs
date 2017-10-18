using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Idaho {
	public static class ExpressionExtension {
		/*
		public static PropertyInfo AsProperty(this Expression<Func<T, U>> e) {
			if (e.Body.NodeType == ExpressionType.MemberAccess) {
				var x = e.Body as MemberExpression;
				if (x != null) { return (PropertyInfo)x.Member; }
			}
			return null;
		}
		*/
	}
}
