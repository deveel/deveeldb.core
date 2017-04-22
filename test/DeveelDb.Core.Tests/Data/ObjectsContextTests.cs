using System;

using Deveel.Data.Configuration;
using Deveel.Data.Services;

using Moq;

using Xunit;

namespace Deveel.Data {
	public class ObjectsContextTests : IDisposable {
		private IContext context;

		public ObjectsContextTests() {
			var config = new Configuration.Configuration();
			config.SetValue("currentSchema", "test");

			var mock = new Mock<IContext>();
			mock.As<IConfigurationScope>().SetupGet(x => x.Configuration).Returns(config);
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());

			context = mock.Object;
		}

		[Theory]
		[InlineData("table1", "test.table1")]
		[InlineData("test.tab1", "test.tab1")]
		public void QualifyName(string name, string expected) {
			var result = context.QualifyName(ObjectName.Parse(name));

			var exp = ObjectName.Parse(expected);
			Assert.Equal(exp, result);
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}