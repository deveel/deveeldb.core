// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Configuration;
using Deveel.Data.Services;

namespace Deveel.Data.Sql.Methods {
	public class SqlMethodRegistry : IMethodResolver, IDisposable {
		private bool initialized;
		private readonly Dictionary<MethodSignature, SqlMethod> methodCache;
		private ServiceContainer container;

		public SqlMethodRegistry() {
			container = new ServiceContainer();
			methodCache = new Dictionary<MethodSignature, SqlMethod>(new MethodSignatureComparer());

			EnsureInitialized();
		}

		~SqlMethodRegistry() {
			Dispose(false);
		}

		private void EnsureInitialized() {
			if (!initialized) {
				Initialize();
			}

			initialized = true;
		}

		protected virtual void Initialize() {
			
		}

		public void Register(SqlMethod method) {
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			container.RegisterInstance<SqlMethod>(method);

			initialized = false;
		}

		public void Register<TMethod>(SqlMethodInfo methodInfo)
			where TMethod : SqlMethod {
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo));

			container.Register<SqlMethod, TMethod>();

			initialized = false;
		}

		SqlMethod IMethodResolver.ResolveMethod(IContext context, Invoke invoke) {
			EnsureInitialized();

			var types = invoke.Arguments.Select(x => x.Value.GetSqlType(context)).ToArray();
			var signature = new MethodSignature(invoke.MethodName, types);

			SqlMethod method;
			if (!methodCache.TryGetValue(signature, out method)) {
				var methods = container.ResolveAll<SqlMethod>();
				foreach (var m in methods) {
					if (m.Matches(context, invoke)) {
						method = m;
						methodCache[signature] = m;
						break;
					}
				}
			}

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
				if (methodCache != null)
					methodCache.Clear();
				if (container != null)
					container.Dispose();
			}

			container = null;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}