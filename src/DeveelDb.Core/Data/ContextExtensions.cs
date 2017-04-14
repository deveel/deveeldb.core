using System;
using System.Collections.Generic;

using Deveel.Data.Services;

namespace Deveel.Data {
	public static class ContextExtensions {
		public static IContext Create(this IContext context, string name) {
			return new Context(context, name);
		}

		public static void RegisterService<TService, TImplementation>(this IContext context, object key = null)
			where TImplementation : class, TService {
			context.Scope.Register<TService, TImplementation>(key);
		}

		public static void RegisterService<TImplementation>(this IContext context, object key = null)
			where TImplementation : class {
			context.Scope.Register<TImplementation>();
		}

		public static void RegisterInstance<TService>(this IContext context, object instance, object key = null) {
			context.Scope.RegisterInstance<TService>(instance);
		}

		public static TService ResolveService<TService>(this IContext context, object key = null) {
			return context.Scope.Resolve<TService>(key);
		}

		public static IEnumerable<TService> ResolveAllServices<TService>(this IContext context) {
			return context.Scope.ResolveAll<TService>();
		}
	}
}