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

using Deveel.Data.Services;

namespace Deveel.Data.Sql.Variables {
	public static class ScopeExtensions {
		public static void AddVariableManager<TManager>(this IScope scope)
			where TManager : class, IVariableManager {
			scope.Register<IVariableManager, TManager>();
			scope.AddObjectManager<TManager>(DbObjectType.Variable);
			scope.Register<TManager>();
		}

		public static void AddVariableManager(this IScope scope) {
			scope.AddVariableManager<VariableManager>();
			scope.AddVariableResolver<VariableManager>();
		}

		public static void AddVariableResolver<TResolver>(this IScope scope)
			where TResolver : class, IVariableResolver {
			scope.Register<IVariableResolver, TResolver>();
		}

		public static void AddVariableResolver<TResolver>(this IScope scope, TResolver resolver)
			where TResolver : class, IVariableResolver {
			scope.RegisterInstance(resolver);
			scope.RegisterInstance<IVariableResolver>(resolver);
		}
	}
}