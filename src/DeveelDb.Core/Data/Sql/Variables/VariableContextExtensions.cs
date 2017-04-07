using System;
using Deveel.Data.Services;

namespace Deveel.Data.Sql.Variables {
	public static class VariableContextExtensions {
		public static Variable ResolveVariable(this IContext context, string name) {
			var current = context;
			while (current != null) {
				var variable = ResolveVariable(current.Scope, name);
				if (variable != null)
					return variable;

				current = current.ParentContext;
			}

			return null;
		}

		private static Variable ResolveVariable(IScope scope, string name) {
			var resolver = scope.Resolve<IVariableResolver>();
			if (resolver == null)
				return null;

			return resolver.ResolveVariable(name);
		}
	}
}