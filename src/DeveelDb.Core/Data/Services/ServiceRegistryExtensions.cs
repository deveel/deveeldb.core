using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Deveel.Data.Services {
	public static class ServiceRegistryExtensions {
		public static void Register(this IServiceRegistry registry, Type serviceType, Type implementationType, object serviceKey) {
			var registration = new ServiceRegistration(serviceType, implementationType);
			registration.ServiceKey = serviceKey;
			registry.Register(registration);
		}

		public static void Register(this IServiceRegistry registry, Type serviceType) {
			Register(registry, serviceType, null);
		}

		public static void Register(this IServiceRegistry registry, Type serviceType, object serviceKey) {
			if (serviceType == null)
				throw new ArgumentNullException("serviceType");

			if (serviceType.GetTypeInfo().IsValueType)
				throw new ArgumentException(String.Format("The service type '{0}' to register is not a class.", serviceType));

			var interfaces = serviceType.GetTypeInfo().ImplementedInterfaces;
			foreach (var interfaceType in interfaces) {
				registry.Register(interfaceType, serviceType, serviceKey);
			}

			registry.Register(serviceType, serviceType, serviceKey);
		}

		public static void Register<TService, TImplementation>(this IServiceRegistry registry, object serviceKey)
			where TImplementation : class, TService {
			registry.Register(typeof(TService), typeof(TImplementation), serviceKey);
		}

		public static void Register<TService, TImplementation>(this IServiceRegistry registry)
			where TImplementation : class, TService {
			registry.Register<TService, TImplementation>(null);
		}

		public static void Register<TService>(this IServiceRegistry registry, object serviceKey)
			where TService : class {
			registry.Register(typeof(TService), serviceKey);
		}

		public static void Register<TService>(this IServiceRegistry registry)
			where TService : class {
			registry.Register<TService>(null);
		}

		public static void RegisterInstance(this IServiceRegistry registry, Type serviceType, object instance) {
			RegisterInstance(registry, serviceType, instance, null);
		}

		public static void RegisterInstance(this IServiceRegistry registry, Type serviceType, object instance, object serviceKey) {
			if (serviceType == null)
				throw new ArgumentNullException("serviceType");
			if (instance == null)
				throw new ArgumentNullException("instance");

			var implementationType = instance.GetType();
			var registration = new ServiceRegistration(serviceType, implementationType);
			registration.Instance = instance;
			registration.ServiceKey = serviceKey;
			registry.Register(registration);
		}

		public static void RegisterInstance<TService>(this IServiceRegistry registry, TService instance) where TService : class {
			RegisterInstance<TService>(registry, instance, null);
		}

		public static void RegisterInstance<TService>(this IServiceRegistry registry, TService instance, object serviceKey)
			where TService : class {
			var interfaces = typeof(TService).GetTypeInfo().ImplementedInterfaces;
			foreach (var interfaceType in interfaces) {
				registry.RegisterInstance(interfaceType, instance, serviceKey);
			}

			registry.RegisterInstance(typeof(TService), instance, serviceKey);
		}

		public static bool Unregister(this IServiceRegistry registry, Type serviceType) {
			return registry.Unregister(serviceType, null);
		}

		public static bool Unregister<TService>(this IServiceRegistry registry, object serviceKey) {
			return registry.Unregister(typeof(TService), serviceKey);
		}

		public static bool Unregister<TService>(this IServiceRegistry registry) {
			return registry.Unregister<TService>(null);
		}

		public static void Replace(this IServiceRegistry registry, Type serviceType, Type implementationType) {
			registry.Replace(serviceType, implementationType, null);
		}

		public static void Replace(this IServiceRegistry registry, Type serviceType, Type implementationType, object serviceKey) {
			if (registry.Unregister(serviceType, serviceKey))
				registry.Register(serviceType, implementationType, serviceKey);
		}

		public static void Replace<TService, TImplementation>(this IServiceRegistry registry)
			where TImplementation : class, TService {
			registry.Replace<TService, TImplementation>(null);
		}

		public static void Replace<TService, TImplementation>(this IServiceRegistry registry, object serviceKey)
			where TImplementation : class, TService {
			registry.Replace(typeof(TService), typeof(TImplementation), serviceKey);
		}

		public static bool IsRegistered(this IServiceRegistry registry, Type serviceType) {
			return registry.IsRegistered(serviceType, null);
		}

		public static bool IsRegistered<T>(this IServiceRegistry registry, object serviceKey) {
			return registry.IsRegistered(typeof(T), serviceKey);
		}

		public static bool IsRegistered<T>(this IServiceRegistry registry) {
			return registry.IsRegistered<T>(null);
		}

	}
}