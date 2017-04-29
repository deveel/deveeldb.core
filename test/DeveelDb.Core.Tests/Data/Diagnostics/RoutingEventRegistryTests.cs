using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Deveel.Data.Services;

using Moq;

using Xunit;

namespace Deveel.Data.Diagnostics {
	public class RoutingEventRegistryTests : IDisposable {
		private List<MessageEvent> routedEvents;
		private AutoResetEvent eventSignal;
		private IContext context;

		public RoutingEventRegistryTests() {
			routedEvents = new List<MessageEvent>();
			eventSignal = new AutoResetEvent(false);

			var scope = new ServiceContainer();
			scope.AddRoutingEventRegistry<MessageEvent>();
			scope.AddEventRouter<MessageEvent>(e => {
				routedEvents.Add(e);
				eventSignal.Set();
			});

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(scope);
			mock.As<IEventSource>();
			mock.Setup(x => x.Dispose())
				.Callback(() => scope.Dispose());

			context = mock.Object;

			scope.RegisterInstance<IContext>(context);
		}

		[Fact]
		public void RouteErrorMessage() {
			context.Error(-1, "test error", null);

			eventSignal.WaitOne(300);

			Assert.NotEmpty(routedEvents);
			Assert.Equal(1, routedEvents.Count);

			var message = routedEvents[0];
			Assert.Equal(MessageLevel.Error, message.Level);
			Assert.Null(message.Error);
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}

