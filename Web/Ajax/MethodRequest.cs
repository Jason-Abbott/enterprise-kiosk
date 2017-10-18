using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace Idaho.Web.Ajax {
	/// <summary>
	/// Build the response for an AJAX method request
	/// </summary>
	public class MethodRequest : Request {

		internal MethodRequest(Response r) : base(r) { }
		private static Dictionary<Type, ConstructorInfo> _constructorCache =
			new Dictionary<Type, ConstructorInfo>();

		/// <summary>
		/// Execute method with given properties and return result
		/// </summary>
		internal override void Process() {
			string typeName = base["type"];
			Type hostType;

			// get method type and validate
			if (Handlers.AjaxHandler.KnownType.ContainsKey(typeName)) {
				hostType = Handlers.AjaxHandler.KnownType[typeName];
			} else {
				hostType = System.Type.GetType(typeName);
			}
			if (hostType == null) {
				this.Response.AddError("Type \"{0}\" could not found", typeName);
				return;
			}
			this.InvokeMethod(hostType);
		}

		/// <summary>
		/// Invoke method on given type
		/// </summary>
		internal protected void InvokeMethod(Type hostType) {
			string methodName = base["method"];
			string result = string.Empty;

			if (string.IsNullOrEmpty(methodName)) {
				this.Response.AddError("No method specified for \"{0}\"", hostType.Name);
				this.Response.Abort();
				return;
			}
			MethodInfo methodInfo = WebInvokable.GetMethod(hostType, methodName);

			if (methodInfo == null) {
				this.Response.AddError("Invokable method \"{0}()\" not found in type \"{1}\"", methodName, hostType.Name);
				this.Response.Abort();
				return;
			}

			// convert parameter types
			string value;
			ParameterInfo[] info = methodInfo.GetParameters();
			List<object> arguments = new List<object>();

			try {
				foreach (ParameterInfo p in info) {
					value = this.Parameters[p.Name];
					arguments.Add(EcmaScript.ToType(value, p.ParameterType, this.Context));
				}
			} catch (System.Exception ex) {
				Idaho.Exception.Log(ex);
				this.Response.Error(ex);
				return;
			}

			try {
				object instance = methodInfo.IsStatic ? null : this.CreateInstance(hostType);
				result = methodInfo.Invoke(instance, arguments.ToArray()).ToJSON();
			} catch (System.Exception ex) {
				Idaho.Exception.Log(ex);
				this.Response.Error(ex);
			} finally {
				this.Response.Complete(result, false);
			}
		}

		/// <summary>
		/// Create an instance of the given type
		/// </summary>
		private object CreateInstance(Type type) {
			ConstructorInfo construct = null;
			object[] parameters = null;
			int count = -1;

			if (_constructorCache.ContainsKey(type) && _constructorCache[type] != null) {
				construct = _constructorCache[type];
				parameters = this.InferParameters(construct);
			} else {
				ConstructorInfo[] constructors = type.GetConstructors();

				foreach (ConstructorInfo i in constructors) {
					// search for a constructor with the most parameters values
					// we can infer (to create most fully formed object possible)
					parameters = this.InferParameters(i);
					if (parameters != null && parameters.Length > count) {
						construct = i;
						count = parameters.Length;
					}
				}
				_constructorCache.Add(type, construct);
			}
			return (construct == null) ? null : construct.Invoke(parameters);
		}

		/// <summary>
		/// Infer constructor parameter values
		/// </summary>
		/// <remarks>
		/// If unable to infer a parameter, return null. Note that this will need
		/// to be evaluated differently from a constructor with no parameters,
		/// which will be an empty list rather than null.
		/// </remarks>
		private object[] InferParameters(ConstructorInfo i) {
			List<object> parameters = new List<object>();
			ParameterInfo[] info = i.GetParameters();
			object value = null;

			foreach (ParameterInfo p in info) {
				value = EcmaScript.ToType(string.Empty, p.ParameterType, this.Context);
				if (value != null) {
					parameters.Add(value);
				} else {
					return null;
				}
			}
			return parameters.ToArray();
		}
	}
}
