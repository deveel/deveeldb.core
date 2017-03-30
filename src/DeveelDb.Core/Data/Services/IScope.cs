using System;
using System.Collections;

namespace Deveel.Data.Services {
    /// <summary>
    /// Provides an isolated scope of the components for the system
    /// registered during the build
    /// </summary>
	public interface IScope : IDisposable {
		/// <summary>
		/// Opens a child scope of this scope
		/// </summary>
		/// <param name="name">The name of the child scope</param>
		/// <returns>
		/// Returns an instance of <see cref="IScope"/> that is inheriting
		/// the service definitions and instances from the parent scope
		/// </returns>
		IScope OpenScope(string name);

		/// <summary>
		/// Resolves an instance of the service of the given type
		/// contained in this scope
		/// </summary>
		/// <param name="serviceType">The type of the service to resolve</param>
		/// <param name="serviceKey">An optional key for the service to be resolved,
		/// to discriminate between two services of the same type.</param>
		/// <returns>
		/// Returns an instance of the service for the given <paramref name="serviceType"/>
		/// that was registered at build.
		/// </returns>
		/// <seealso cref="IServiceRegistry.Register"/>
		/// <exception cref="ServiceResolutionException">Thrown if an error occurred
		/// while resolving the service within this scope</exception>
		object Resolve(Type serviceType, object serviceKey);

		/// <summary>
		/// Resolves all the instances of services of the given type 
		/// contained in this scope
		/// </summary>
		/// <param name="serviceType">The type of the service instances to be resolved</param>
		/// <returns>
		/// Returns an enumeration of instances of all the services of the given
		/// <paramref name="serviceType"/> contained in this scope
		/// </returns>
		/// <seealso cref="IServiceRegistry.Register"/>
		IEnumerable ResolveAll(Type serviceType);
	}
}
