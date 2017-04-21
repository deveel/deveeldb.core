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

using Deveel.Data.Services;

namespace Deveel.Data.Sql.Methods {
	public class SqlMethodRegistry : IMethodResolver, IDisposable {
		private bool initialized;
		private ServiceContainer container;

		public SqlMethodRegistry() {
			container = new ServiceContainer();

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

		public void Register(SqlMethod method, params ObjectName[] aliases) {
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			var methodName = method.MethodInfo.MethodName.ToUpper();
			container.RegisterInstance<SqlMethod>(method, methodName);

			foreach (var alias in aliases) {
				container.RegisterInstance<SqlMethod>(method, alias.ToUpper());
			}

			initialized = false;
		}

		public void Register<TMethod>(SqlMethodInfo methodInfo, params ObjectName[] aliases)
			where TMethod : SqlMethod {
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo));

			var methodName = methodInfo.MethodName.ToUpper();
			container.Register<SqlMethod, TMethod>(methodName);

			foreach (var alias in aliases) {
				container.Register<SqlMethod, TMethod>(alias.ToUpper());
			}

			initialized = false;
		}

		SqlMethod IMethodResolver.ResolveMethod(IContext context, Invoke invoke) {
			EnsureInitialized();

			var methodName = invoke.MethodName.ToUpper();
			return container.Resolve<SqlMethod>(methodName);			
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
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