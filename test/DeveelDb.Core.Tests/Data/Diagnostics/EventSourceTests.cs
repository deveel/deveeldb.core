using System;
using System.Collections.Generic;

using Moq;

using Xunit;
using Xunit.Abstractions;

namespace Deveel.Data.Diagnostics {
	public class EventSourceTests {
		private ITestOutputHelper output;

		public EventSourceTests(ITestOutputHelper output) {
			this.output = output;
		}

		[Fact]
		public void CreateEnvSource() {
			IEventSource source = new EnvironmentEventSource();

			Assert.NotNull(source.Metadata);
			Assert.NotEmpty(source.Metadata);
			Assert.Null(source.ParentSource);

			foreach (var pair in source.Metadata) {
				output.WriteLine("{0} = {1}", pair.Key, pair.Value);
			}
		}

		[Fact]
		public void CreateMockedEventSource() {
			var mock = new Mock<IEventSource>();
			mock.SetupGet(x => x.Metadata)
				.Returns(new[] {new KeyValuePair<string, object>("a", 67)});

			var source = mock.Object;

			var value = source.GetValue<int>("a");

			Assert.Equal(67, value);
		}

		[Fact]
		public void CreateEmptyEvent() {
			var @event = new Event(new EnvironmentEventSource(), -1);

			Assert.NotNull(@event.EventSource);
			Assert.IsType<EnvironmentEventSource>(@event.EventSource);
			Assert.Equal(-1, @event.EventId);
		}
	}
}