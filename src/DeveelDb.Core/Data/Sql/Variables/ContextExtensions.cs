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
				if (current is IVariableScope) {
					var scope = (IVariableScope) current;
					var variable = scope.Variables.ResolveVariable(name, ignoreCase);
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

		public static IEnumerable<IVariableResolver> GetVariableResolvers(this IContext context) {
			return context.Scope.ResolveAll<IVariableResolver>();
		}

		public static SqlType ResolveVariableType(this IContext context, string name, bool ignoreCase) {
			var current = context;
			while (current != null) {
				if (current is IVariableScope) {
					var scope = (IVariableScope)current;
					var type = scope.Variables.ResolveVariableType(name, ignoreCase);
					if (type != null)
						return type;
				}

				current = current.ParentContext;
			}

			return null;
		}
	}
}