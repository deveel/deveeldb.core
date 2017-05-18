using System;
using System.Collections.Generic;

using Deveel.Data.Services;

using Moq;

using Xunit;

namespace Deveel.Data.Diagnostics {
	public class ContextEventRegistryTests : IDisposable {
		private IContext context;
		private List<IEvent> events;

		public ContextEventRegistryTests() {
			events = new List<IEvent>();

			var mock = new Mock<IContext>();
			mock.As<IEventSource>();

			var container = new ServiceContainer();
			container.RegisterInstance<IList<IEvent>>(events);
			container.Register<IEventRegistry<MessageEvent>, MessageRegistry>();

			mock.SetupGet(x => x.Scope)
				.Returns(container);

			context = mock.Object;
		}

		[Fact]
		public void LogMessage() {
			context.LogMessage(101, MessageLevel.Debug, "test debug");

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);

			var e = events[0];

			Assert.IsType<MessageEvent>(e);
			Assert.NotNull(e.EventSource);
			Assert.Equal(context as IEventSource, e.EventSource);
			Assert.Equal(101, e.EventId);
			Assert.Equal(MessageLevel.Debug, ((MessageEvent)e).Level);
			Assert.Equal("test debug", ((MessageEvent)e).Text);
			Assert.Null(((MessageEvent)e).Error);
		}

		[Fact]
		public void Debug() {
			context.Debug(101, "test debug");

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);

			var e = events[0];

			Assert.IsType<MessageEvent>(e);
			Assert.NotNull(e.EventSource);
			Assert.Equal(context as IEventSource, e.EventSource);
			Assert.Equal(101, e.EventId);
			Assert.Equal(MessageLevel.Debug, ((MessageEvent)e).Level);
			Assert.Equal("test debug", ((MessageEvent)e).Text);
			Assert.Null(((MessageEvent)e).Error);
		}

		[Fact]
		public void Trace() {
			context.Trace(103, "test trace");

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);

			var e = events[0];

			Assert.IsType<MessageEvent>(e);
			Assert.NotNull(e.EventSource);
			Assert.Equal(context as IEventSource, e.EventSource);
			Assert.Equal(103, e.EventId);
			Assert.Equal(MessageLevel.Trace, ((MessageEvent)e).Level);
			Assert.Equal("test trace", ((MessageEvent)e).Text);
			Assert.Null(((MessageEvent)e).Error);
		}

		[Fact]
		public void Info() {
			context.Information(201, "test info");

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);

			var e = events[0];

			Assert.IsType<MessageEvent>(e);
			Assert.NotNull(e.EventSource);
			Assert.Equal(context as IEventSource, e.EventSource);
			Assert.Equal(201, e.EventId);
			Assert.Equal(MessageLevel.Information, ((MessageEvent)e).Level);
			Assert.Equal("test info", ((MessageEvent)e).Text);
			Assert.Null(((MessageEvent)e).Error);
		}

		[Fact]
		public void Error() {
			context.Error(-501, "test error", new Exception());

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);

			var e = events[0];

			Assert.IsType<MessageEvent>(e);
			Assert.NotNull(e.EventSource);
			Assert.Equal(context as IEventSource, e.EventSource);
			Assert.Equal(-501, e.EventId);
			Assert.Equal(MessageLevel.Error, ((MessageEvent)e).Level);
			Assert.Equal("test error", ((MessageEvent)e).Text);
			Assert.NotNull(((MessageEvent)e).Error);
		}

		[Fact]
		public void Fatal() {
			context.Fatal(-7001, "fatal", new Exception());

			Assert.NotEmpty(events);
			Assert.Equal(1, events.Count);

			var e = events[0];

			Assert.IsType<MessageEvent>(e);
			Assert.NotNull(e.EventSource);
			Assert.Equal(context as IEventSource, e.EventSource);
			Assert.Equal(-7001, e.EventId);
			Assert.Equal(MessageLevel.Fatal, ((MessageEvent)e).Level);
			Assert.Equal("fatal", ((MessageEvent)e).Text);
			Assert.NotNull(((MessageEvent)e).Error);
		}

		public void Dispose() {
			context.Dispose();
		}

		#region MessageRegistry

		class MessageRegistry : IEventRegistry<MessageEvent> {
			private readonly IList<IEvent> events;

			public MessageRegistry(IList<IEvent> events) {
				this.events = events;
			}

			public void Register(MessageEvent @event) {
				events.Add(@event);
			}

			public Type EventType => typeof(MessageEvent);

			void IEventRegistry.Register(IEvent @event) {
				Register((MessageEvent)@event);
			}
		}

		#endregion
	}
}