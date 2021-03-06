﻿// 
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
using System.Collections;

namespace Deveel.Data.Services {
    /// <summary>
    /// Provides an isolated scope of the components for the system
    /// registered during the build
    /// </summary>
	public interface IScope : IServiceProvider, IDisposable {
		/// <summary>
		/// Gets a value indicating if this scope is read-only
		/// </summary>
		/// <remarks>
		/// A read-only scope does not support the registration or
		/// unregistration of services and throws exceptions whenever
		/// users try to mutate the registrations.
		/// </remarks>
		bool IsReadOnly { get; }

	    /// <summary>
	    /// Registers a service using the specifications for resolution
	    /// </summary>
	    /// <param name="registration">The service registration resolution
	    /// information provided to the registry</param>
	    /// <exception cref="ArgumentNullException">If the provided <paramref name="registration"/>
	    /// is <c>null</c></exception>
	    /// <exception cref="ServiceException">If an error occurred while registering 
	    /// the service given</exception>
	    void Register(ServiceRegistration registration);

	    /// <summary>
	    /// Removes a registered service from the registry
	    /// </summary>
	    /// <param name="serviceType">The type of the service to be removed</param>
	    /// <param name="serviceKey">An optional key to identify the service
	    /// with the given type to be removed</param>
	    /// <returns>
	    /// Returns <c>true</c> if a service with the given type and key
	    /// was find and removed, otherwise <c>false</c> if it was not found in the
	    /// underlying registry
	    /// </returns>
	    bool Unregister(Type serviceType, object serviceKey);

	    /// <summary>
	    /// Verifies if a service of the given type with the given
	    /// key is registered.
	    /// </summary>
	    /// <param name="serviceType">The type of the service to verify.</param>
	    /// <param name="serviceKey">An optional key to identify the service of
	    /// the given type within the underlying registry</param>
	    /// <returns></returns>
	    bool IsRegistered(Type serviceType, object serviceKey);

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
		/// <seealso cref="Register"/>
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
		/// <seealso cref="Register"/>
		IEnumerable ResolveAll(Type serviceType);
	}
}
