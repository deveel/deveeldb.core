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
using System.Reflection;
using System.Threading.Tasks;

using Deveel.Data.Services;

namespace Deveel.Data.Security {
	public static class SecurityRequirementExtensions {
		public static async Task CheckRequirementsAsync(this IContext context) {
			var registry = context.Scope.Resolve<IRequirementCollection>();
			if (registry == null)
				return;

			foreach (var requirement in registry) {
				var reqType = requirement.GetType();
				var handlerType = typeof(IRequirementHandler<>).MakeGenericType(reqType);

				var handlers = context.Scope.ResolveAll(handlerType);
				foreach (var handler in handlers) {
					await HandleRequirement(context, handlerType, handler, reqType, requirement);
				}
			}
		}

		private static Task HandleRequirement(IContext context, Type handlerType, object handler, Type reqType, IRequirement requirement) {
			var method = handlerType.GetRuntimeMethod("HandleRequirementAsync", new[] {typeof(IContext), reqType});
			if (method == null)
				throw new InvalidOperationException();

			try {
				return (Task) method.Invoke(handler, new object[] {context, requirement});
			} catch (TargetInvocationException e) {
				throw e.InnerException;
			}
		}
	}
}