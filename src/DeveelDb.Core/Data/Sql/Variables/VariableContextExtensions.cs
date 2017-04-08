using System;
using Deveel.Data.Services;

namespace Deveel.Data.Sql.Variables {
	public static class VariableContextExtensions {
		public static Variable ResolveVariable(this IContext context, string name, bool ignoreCase) {
			var current = context;
			while (current != null) {
				var variable = ResolveVariable(current.Scope, name, ignoreCase);
				if (variable != null)
					return variable;

				current = current.ParentContext;
			}

			return null;
		}

		private static Variable ResolveVariable(IScope scope, string name, bool ignoreCase) {
			var resolver = scope.Resolve<IVariableResolver>();
			if (resolver == null)
				return null;

			return resolver.ResolveVariable(name, ignoreCase);
		}

		public static VariableManager ResolveVariableManager(this IContext context) {
			var current = context;
			while (current != null) {
				var manager = current.Scope.Resolve<VariableManager>();
				if (manager != null)
					return manager;

				current = current.ParentContext;
			}

			return null;
		}
	}
}