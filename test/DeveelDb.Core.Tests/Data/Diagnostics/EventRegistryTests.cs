using System;
using System.Collections.Generic;


using Moq;

using Xunit;

namespace Deveel.Data.Diagnostics {
	public class EventRegistryTests {
		private IEventRegistry registry;
		private List<IEvent> events;

		public EventRegistryTests() {
			events = new List<IEvent>();

			var mock = new Mock<IEventRegistry>();
			mock.Setup(x => x.Register(It.IsAny<IEvent>()))
				.Callback<IEvent>(e => events.Add(e));
			mock.SetupGet(x => x.EventType)
				.Returns(typeof(Event));

			registry = mock.Object;
		}

		[Fact]
		public void RegisterCreatedEvent() {
			registry.Register(new Event(new EnvironmentEventSource(), -1));

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);
		}

		[Fact]
		public void RegisterEventToBeBuilt() {
			registry.Register<Event>(new EnvironmentEventSource(), -1);

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);
		}
	}
}