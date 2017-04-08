using System;

using Xunit;

namespace Deveel.Data.Services {
	public class ScopeTests : IDisposable {
		private ServiceContainer container;

		public ScopeTests() {
			container = new ServiceContainer();
			container.Register<IService, Service1>("one");
			container.Register<IService, Service2>("two");
		}

		[Fact]
		public void OpenScope() {
			var scope = container.OpenScope("testScope");

			Assert.NotNull(scope);
		}

		[Fact]
		public void OpenScopeAndResolveOneService() {
			var scope = container.OpenScope("testScope");

			var service = scope.Resolve<IService>("one");

			Assert.NotNull(service);
			Assert.IsType<Service1>(service);
		}

		public void Dispose() {
			container.Dispose();
		}

		#region IService

		interface IService {
			
		}

		#endregion

		#region Service1

		class Service1 : IService {
			
		}

		#endregion

		#region Service2

		class Service2 : Service1 {
			
		}

		#endregion
	}
}