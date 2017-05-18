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

namespace Deveel.Data.Sql {
	public static class ScopeExtensions {
		public static void AddStringSearch<T>(this IScope scope)
			where T : class, ISqlStringSearch {
			scope.Register<ISqlStringSearch, T>();
		}

		public static ISqlStringSearch GetStringSearch(this IScope scope) {
			return scope.Resolve<ISqlStringSearch>();
		}

		public static void AddReferenceResolver<TResolver>(this IScope scope)
			where TResolver : class, IReferenceResolver {
			scope.Unregister<IReferenceResolver>();
			scope.Register<IReferenceResolver, TResolver>();
			scope.Register<TResolver>();
		}

		public static void AddReferenceResolver<TResolver>(this IScope scope, TResolver resolver)
			where TResolver : class, IReferenceResolver {
			scope.Unregister<IReferenceResolver>();
			scope.RegisterInstance<IReferenceResolver>(resolver);
		}

		public static void AddGroupResolver<TResolver>(this IScope scope, TResolver resolver)
			where TResolver : class, IGroupResolver {
			scope.Unregister<IGroupResolver>();
			scope.RegisterInstance<IGroupResolver>(resolver);
			scope.Register<TResolver>();
		}
	}
}