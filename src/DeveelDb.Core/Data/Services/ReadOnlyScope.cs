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
using System.Collections;

namespace Deveel.Data.Services {
	class ReadOnlyScope : IScope {
		private IScope scope;

		public ReadOnlyScope(IScope scope) {
			this.scope = scope;
		}

		public object GetService(Type serviceType) {
			return scope.Resolve(serviceType);
		}

		public void Dispose() {
			if (scope != null)
				scope.Dispose();

			scope = null;
		}

		public bool IsReadOnly => true;

		public void Register(ServiceRegistration registration) {
			throw new NotSupportedException("The scope is read-only");
		}

		public bool Unregister(Type serviceType, object serviceKey) {
			throw new NotSupportedException("The scope is read-only");
		}

		public bool IsRegistered(Type serviceType, object serviceKey) {
			return scope.IsRegistered(serviceType, serviceKey);
		}

		public IScope OpenScope(string name) {
			return scope.OpenScope(name);
		}

		public object Resolve(Type serviceType, object serviceKey) {
			return scope.Resolve(serviceType, serviceKey);
		}

		public IEnumerable ResolveAll(Type serviceType) {
			return scope.ResolveAll(serviceType);
		}
	}
}