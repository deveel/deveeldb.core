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
	public static class ScopeExtensions {
		public static void AddMethodRegistry<TRegistry>(this IScope scope)
			where TRegistry : SqlMethodRegistry {
			scope.Register<IMethodResolver, TRegistry>();
			scope.Register<SqlMethodRegistry, TRegistry>();
			scope.Register<TRegistry>();
		}

		public static void AddMethodRegistry<TRegistry>(this IScope scope, TRegistry registry)
			where TRegistry : SqlMethodRegistry {
			scope.RegisterInstance<IMethodResolver>(registry);
			scope.RegisterInstance(registry);
		}
	}
}