using System;
using System.Collections.Generic;
using System.Reflection;

using Deveel.Data.Services;

namespace Deveel.Data.Security {
	public static class SecurityRequirementExtensions {
		public static void CheckRequirements(this IContext context) {
			var registry = context.Scope.Resolve<IRequirementCollection>();
			if (registry == null)
				return;

			foreach (var requirement in registry) {
				var reqType = requirement.GetType();
				var handlerType = typeof(IRequirementHandler<>).MakeGenericType(reqType);

				var handlers = context.Scope.ResolveAll(handlerType);
				foreach (var handler in handlers) {
					HandleRequirement(context, handlerType, handler, reqType, requirement);
				}
			}
		}

		private static void HandleRequirement(IContext context, Type handlerType, object handler, Type reqType, IRequirement requirement) {
			var method = handlerType.GetRuntimeMethod("HandleRequirement", new[] {typeof(IContext), reqType});
			if (method == null)
				throw new InvalidOperationException();

			try {
				method.Invoke(handler, new object[] {context, requirement});
			} catch (TargetInvocationException e) {
				throw e.InnerException;
			}
		}
	}
}