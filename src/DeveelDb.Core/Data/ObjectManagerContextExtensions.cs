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

namespace Deveel.Data {
	public static class ObjectManagerContextExtensions {
		public static void RegisterObjectManager<TManager>(this IContext context, DbObjectType objectType) 
			where TManager : class, IDbObjectManager {
			context.Scope.Register<IDbObjectManager, TManager>(objectType);
			context.Scope.Register<TManager>(objectType);
		}

		public static TManager ResolveObjectManager<TManager>(this IContext context, DbObjectType objectType)
			where TManager : IDbObjectManager {
			var current = context;
			while (current != null) {
				if (current.Scope.IsRegistered<IDbObjectManager>(objectType))
					return (TManager) current.Scope.Resolve<IDbObjectManager>(objectType);

				current = current.ParentContext;
			}

			return default(TManager);
		}
	}
}