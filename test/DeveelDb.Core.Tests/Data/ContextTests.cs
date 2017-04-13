using System;

using Moq;

using Xunit;

namespace Deveel.Data {
	public class ContextTests : IDisposable {
		public ContextTests() {
			context = new Context("test");
		}

		private Context context;

		[Fact]
		public void IsEmpty() {
			Assert.Null((context as IContext).Scope);
			Assert.Null((context as IContext).ParentContext);
			Assert.NotNull((context as IContext).ContextName);
		}

		[Fact]
		public void DisposeContext() {
			context.Dispose();
		}

		public void Dispose() {
			if (context != null)
				context.Dispose();
		}
	}
}