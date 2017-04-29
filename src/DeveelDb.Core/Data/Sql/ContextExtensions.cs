using System;

using Deveel.Data.Services;

namespace Deveel.Data.Sql {
	public static class ContextExtensions {
		public static IGroupResolver GetGroupResolver(this IContext context) {
			return context.Scope.Resolve<IGroupResolver>();
		}

		public static IReferenceResolver GetReferenceResolver(this IContext context) {
			return context.Scope.Resolve<IReferenceResolver>();
		}
	}
}