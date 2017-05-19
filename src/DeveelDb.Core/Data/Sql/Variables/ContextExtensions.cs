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

using Deveel.Data.Services;

namespace Deveel.Data.Sql.Variables {
	public static class ContextExtensions {
		public static Variable ResolveVariable(this IContext context, string name, bool ignoreCase) {
			var current = context;
			while (current != null) {
				var resolvers = current.GetVariableResolvers();
				foreach (var resolver in resolvers) {
					var variable = resolver.ResolveVariable(name, ignoreCase);
					if (variable != null)
						return variable;
				}

				current = current.ParentContext;
			}

			return null;
		}

		public static TManager GetVariableManager<TManager>(this IContext context)
			where TManager : class, IVariableManager {
			return context.GetObjectManager<TManager>(DbObjectType.Variable);
		}

		public static VariableManager GetVariableManager(this IContext context) {
			return context.GetVariableManager<VariableManager>();
		}

		public static IEnumerable<IVariableResolver> GetVariableResolvers(this IContext context) {
			return context.Scope.ResolveAll<IVariableResolver>();
		}

		public static SqlType ResolveVariableType(this IContext context, string name, bool ignoreCase) {
			var resolvers = context.GetVariableResolvers();
			if (resolvers == null)
				return null;

			return resolvers.Select(resolver => resolver.ResolveVariableType(name, ignoreCase))
				.FirstOrDefault(type => type != null);
		}
	}
}