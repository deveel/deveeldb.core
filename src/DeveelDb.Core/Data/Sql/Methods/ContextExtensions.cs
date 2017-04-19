using System;
using System.Linq;

namespace Deveel.Data.Sql.Methods {
	public static class ContextExtensions {
		public static bool IsSystemFunction(this IContext context, ObjectName name, params InvokeArgument[] arguments) {
			var invoke = new Invoke(name, arguments);
			var resolvers = context.ResolveAllServices<IMethodResolver>();
			foreach (var resolver in resolvers) {
				var method = resolver.ResolveMethod(context, invoke);
				if (method != null && method.IsFunction && method.IsSystem)
					return true;
			}

			return false;
		}
	}
}