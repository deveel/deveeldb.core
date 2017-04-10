using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Configuration;

namespace Deveel.Data.Sql.Methods {
	public class SqlMethodRegistry : IMethodResolver, IDisposable {
		private bool initialized;
		private Dictionary<MethodSignature, SqlMethod> methods;
		private Dictionary<ObjectName, ObjectName> ignoreCaseResolver;

		public SqlMethodRegistry() {
			methods = new Dictionary<MethodSignature, SqlMethod>();
			ignoreCaseResolver = new Dictionary<ObjectName, ObjectName>(ObjectNameComparer.IgnoreCase);
		}

		~SqlMethodRegistry() {
			Dispose(false);
		}

		private void EnsureInitialized() {
			if (!initialized) {
				Initialize();

				PostInitialize();
			}

			initialized = true;
		}

		protected virtual void Initialize() {
			
		}

		private void PostInitialize() {
			foreach (var method in methods) {
				ignoreCaseResolver[method.Key.Name] = method.Key.Name;
			}
		}

		public void Register(SqlMethod method) {
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			var methodInfo = method.MethodInfo;
			var types = methodInfo.Parameters.Select(x => x.ParameterType).ToArray();
			var key = new MethodSignature(methodInfo.MethodName, types);

			methods[key] = method;
			ignoreCaseResolver[methodInfo.MethodName] = methodInfo.MethodName;

			initialized = false;
		}

		public void RegisterFunction(SqlFunctionInfo functionInfo) {
			Register(new SqlFunction(functionInfo));
		}

		SqlMethod IMethodResolver.ResolveMethod(IContext context, Invoke invoke) {
			EnsureInitialized();

			var ignoreCase = context.GetValue("ignoreCase", true);

			var types = invoke.Arguments.Select(x => x.Value.GetSqlType(context)).ToArray();
			var normName = invoke.MethodName;
			if (ignoreCase) {
				if (!ignoreCaseResolver.TryGetValue(invoke.MethodName, out normName)) {
					return null;
				}
			}

			SqlMethod method;
			if (!methods.TryGetValue(new MethodSignature(normName, types), out method))
				return null;

			return method;
		}

		#region MethodSignature

		class MethodSignature : IEquatable<MethodSignature> {
			public MethodSignature(ObjectName name, SqlType[] types) {
				Name = name;
				Types = types;
			}

			public ObjectName Name { get;  }

			public SqlType[] Types { get; }

			public bool Equals(MethodSignature other) {
				if (!Name.Equals(other.Name, true))
					return false;

				if (Types.Length != other.Types.Length)
					return false;

				for (int i = 0; i < Types.Length; i++) {
					var thisType = Types[i];
					var otherType = other.Types[i];
					if (!thisType.IsComparable(otherType))
						return false;
				}

				return true;
			}

			public override bool Equals(object obj) {
				return Equals((MethodSignature) obj);
			}

			public override int GetHashCode() {
				var code = Name.GetHashCode(true);

				foreach (var type in Types) {
					code += type.GetHashCode();
				}

				return code;
			}
		}

		#endregion

		#region MethodSignatureComparer

		class MethodSignatureComparer : IEqualityComparer<MethodSignature> {
			public bool Equals(MethodSignature x, MethodSignature y) {
				return x.Equals(y);
			}

			public int GetHashCode(MethodSignature obj) {
				return obj.GetHashCode();
			}
		}

		#endregion

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (methods != null)
					methods.Clear();
				if (ignoreCaseResolver != null)
					ignoreCaseResolver.Clear();
			}

			ignoreCaseResolver = null;
			methods = null;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}