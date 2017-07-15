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

using DryIoc;

namespace Deveel.Data.Services {
	public class ServiceContainer : IScope {
		private IContainer container;
		public ServiceContainer() 
			: this(null, null, false) {
		}

		private ServiceContainer(ServiceContainer parent, string scopeName, bool readOnly) {
			if (parent != null) {
				container = parent.container.OpenScope(scopeName,
					rules => rules.WithDefaultReuseInsteadOfTransient(Reuse.InCurrentNamedScope(scopeName)));

				ScopeName = scopeName;
			} else {
				container = new Container(Rules.Default.WithTrackingDisposableTransients());
			}

			IsReadOnly = readOnly;
		}

		~ServiceContainer() {
			Dispose(false);
		}

		object IServiceProvider.GetService(Type serviceType) {
			return Resolve(serviceType, null);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				lock (this) {
					if (container != null) {
						container.Dispose();
					}
				}
			}

			lock (this) {
				container = null;
			}
		}

		private string ScopeName { get; set; }

		public bool IsReadOnly { get; }

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public IScope OpenScope(string name) {
			return new ServiceContainer(this, name, false);
		}

		public object Resolve(Type serviceType, object name) {
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			if (container == null)
				throw new InvalidOperationException("The container was not initialized.");

			lock (this) {
				try {
					return container.Resolve(serviceType, name, IfUnresolved.ReturnDefault);
				} catch (Exception ex) {
					throw new ServiceResolutionException(serviceType, "Error when resolving service", ex);
				}
			}
		}

		public IEnumerable ResolveAll(Type serviceType) {
			if (serviceType == null)
				throw new ArgumentNullException("serviceType");

			if (container == null)
				throw new InvalidOperationException("The container was not initialized.");

			lock (this) {
				try {
					return container.ResolveMany<object>(serviceType);
				} catch (NullReferenceException) {
					// this means that the container is out of sync in the dispose
					return new object[0];
				} catch (ServiceResolutionException) {
					throw;
				} catch (Exception ex) {
					throw new ServiceResolutionException(serviceType, "Error resolving all services", ex);
				}
			}
		}

		public void Register(ServiceRegistration registration) {
			if (registration == null)
				throw new ArgumentNullException(nameof(registration));

			if (container == null)
				throw new InvalidOperationException("The container was not initialized.");

			try {
				lock (this) {
					var serviceType = registration.ServiceType;
					var service = registration.Instance;
					var serviceName = registration.ServiceKey;
					var implementationType = registration.ImplementationType;

					var reuse = Reuse.Singleton;
					if (!String.IsNullOrWhiteSpace(ScopeName))
						reuse = Reuse.InCurrentNamedScope(ScopeName);
					if (!String.IsNullOrEmpty(registration.Scope))
						reuse = Reuse.InCurrentNamedScope(registration.Scope);

					if (service == null) {
						container.Register(serviceType, implementationType, serviceKey: serviceName, reuse:reuse);
					} else {
						container.RegisterInstance(serviceType, service, serviceKey: serviceName, reuse:reuse);
					}
				}
			} catch(ServiceException) {
				throw;
			} catch (Exception ex) {
				throw new ServiceException("Error when registering service.", ex);
			}
		}

		public bool Unregister(Type serviceType, object serviceName) {
			if (serviceType == null)
				throw new ArgumentNullException("serviceType");

			if (container == null)
				throw new InvalidOperationException("The container was not initialized.");

			lock (this) {
				try {
					container.Unregister(serviceType, serviceName);
					return true;
				} catch (Exception ex) {
					throw new ServiceException("Error when unregistering service", ex);
				}
			}
		}

		public bool IsRegistered(Type serviceType, object key) {
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			if (container == null)
				throw new InvalidOperationException("The container was not initialized.");

			lock (this) {
				return container.IsRegistered(serviceType, key);
			}
		}
	}
}
