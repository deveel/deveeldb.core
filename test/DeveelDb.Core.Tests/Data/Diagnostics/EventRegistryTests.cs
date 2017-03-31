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


		[Fact]
		public void RegisterMessage() {
			registry.RegisterMessage(new EnvironmentEventSource(), -233, MessageLevel.Information, "info");

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);
			Assert.IsType<MessageEvent>(events[0]);

			var message = (MessageEvent) events[0];
			Assert.Equal(-233, message.EventId);
			Assert.Equal(MessageLevel.Information, message.Level);
			Assert.Equal("info", message.Text);
			Assert.Null(message.Error);
		}

		[Fact]
		public void RegisterTrace() {
			registry.Trace(new EnvironmentEventSource(), 9033, "trace");

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);
			Assert.IsType<MessageEvent>(events[0]);

			var message = (MessageEvent) events[0];
			Assert.Equal(9033, message.EventId);
			Assert.Equal(MessageLevel.Trace, message.Level);
			Assert.Equal("trace", message.Text);
			Assert.Null(message.Error);
		}

		[Fact]
		public void RegisterWarning() {
			registry.Warning(new EnvironmentEventSource(), 9035, "warn");

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);
			Assert.IsType<MessageEvent>(events[0]);

			var message = (MessageEvent) events[0];
			Assert.Equal(9035, message.EventId);
			Assert.Equal(MessageLevel.Warning, message.Level);
			Assert.Equal("warn", message.Text);
			Assert.Null(message.Error);
		}

		[Fact]
		public void RegisterWarningWithError() {
			registry.Warning(new EnvironmentEventSource(), 3344, "warn", new Exception());

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);
			Assert.IsType<MessageEvent>(events[0]);

			var message = (MessageEvent) events[0];
			Assert.Equal(3344, message.EventId);
			Assert.Equal(MessageLevel.Warning, message.Level);
			Assert.Equal("warn", message.Text);
			Assert.NotNull(message.Error);
		}


		[Fact]
		public void RegisterDebug() {
			registry.Debug(new EnvironmentEventSource(), 1002, "dbg");

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);
			Assert.IsType<MessageEvent>(events[0]);

			var message = (MessageEvent) events[0];
			Assert.Equal(1002, message.EventId);
			Assert.Equal(MessageLevel.Debug, message.Level);
			Assert.Equal("dbg", message.Text);
			Assert.Null(message.Error);
		}

		[Fact]
		public void RegisterInfo() {
			registry.Information(new EnvironmentEventSource(), 0334, "info");

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);
			Assert.IsType<MessageEvent>(events[0]);

			var message = (MessageEvent) events[0];
			Assert.Equal(0334, message.EventId);
			Assert.Equal(MessageLevel.Information, message.Level);
			Assert.Equal("info", message.Text);
			Assert.Null(message.Error);
		}
	}
}