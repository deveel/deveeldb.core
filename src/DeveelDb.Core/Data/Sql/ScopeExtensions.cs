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
			scope.Register<IReferenceResolver, TResolver>();
			scope.Register<TResolver>();
		}

		public static void AddReferenceResolver<TResolver>(this IScope scope, TResolver resolver)
			where TResolver : class, IReferenceResolver {
			scope.RegisterInstance<IReferenceResolver>(resolver);
		}
	}
}