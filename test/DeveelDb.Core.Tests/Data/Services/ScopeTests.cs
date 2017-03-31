using System;
using System.Linq;

using Xunit;

namespace Deveel.Data.Services {
	public class ScopeTests {
		[Theory]
		[InlineData(typeof(IService), typeof(ServiceOne), null)]
		[InlineData(typeof(IService), typeof(ServiceOne), "s1")]
		public void RegisterAndCheckService(Type serviceType, Type implementationType, object key) {
			var provider = new ServiceContainer();
			provider.Register(serviceType, implementationType, key);

			Assert.True(provider.IsRegistered(serviceType, key));
		}

		[Theory]
		[InlineData(typeof(IService), typeof(ServiceOne), null)]
		[InlineData(typeof(IService), typeof(ServiceOne), "s1")]
		public void RegisterAndResolveService(Type serviceType, Type implementationType, object key) {
			var provider = new ServiceContainer();
			provider.Register(serviceType, implementationType, key);

			var service = provider.Resolve(serviceType, key);

			Assert.NotNull(service);
			Assert.IsType(implementationType, service);
		}

		[Fact]
		public void OpenScopeAndResolveParent() {
			var provider = new ServiceContainer();
			provider.Register<IService, ServiceOne>();

			var scope = provider.OpenScope("c");

			Assert.NotNull(scope);

			var service = provider.Resolve<IService>();

			Assert.NotNull(service);
			Assert.IsType<ServiceOne>(service);
		}

		[Fact]
		public void RegisterAndUnregisterFromSameScope() {
			var provider = new ServiceContainer();
			provider.Register<IService, ServiceOne>();

			Assert.True(provider.IsRegistered<IService>());

			Assert.True(provider.Unregister<IService>());

			Assert.False(provider.IsRegistered<IService>());
			var service = provider.Resolve<IService>();

			Assert.Null(service);
		}

		[Fact]
		public void RegisterAndUnregisterFromChildScope() {
			var provider = new ServiceContainer();
			provider.Register<IService, ServiceOne>();

			Assert.True(provider.IsRegistered<IService>());

			var scope = provider.OpenScope("b");

			Assert.NotNull(scope);
			Assert.True(scope.IsRegistered<IService>());

			Assert.True(scope.Unregister<IService>());

			var service = scope.Resolve<IService>();

			Assert.Null(service);
		}

		[Fact]
		public void ResolveAll() {
			var provider = new ServiceContainer();
			provider.Register<IService, ServiceOne>();
			provider.Register<IService, ServiceTwo>();

			var services = provider.ResolveAll<IService>();

			Assert.NotNull(services);
			Assert.NotEmpty(services);
			Assert.Equal(2, services.Count());
		}

		[Fact]
		public void RegisterManyAndResolveOne() {
			var provider = new ServiceContainer();
			provider.Register<IService, ServiceOne>();
			provider.Register<IService, ServiceTwo>("two");

			var service = provider.Resolve<IService>("two");

			Assert.NotNull(service);
			Assert.IsType<ServiceTwo>(service);

			Assert.Equal("Hello!", service.Do());
		}

		[Fact]
		public void RegisterInstance() {
			var provider = new ServiceContainer();
			provider.RegisterInstance<IService>(new ServiceOne());

			var service = provider.Resolve<IService>();

			Assert.NotNull(service);
			Assert.IsType<ServiceOne>(service);
		}

		#region IService

		private interface IService {
			object Do();
		}

		#endregion

		#region ServiceOne

		class ServiceOne : IService {
			public object Do() {
				return 566;
			}
		}

		#endregion

		#region ServiceTwo

		class ServiceTwo : IService {
			public object Do() {
				return "Hello!";
			}
		}

		#endregion
	}
}