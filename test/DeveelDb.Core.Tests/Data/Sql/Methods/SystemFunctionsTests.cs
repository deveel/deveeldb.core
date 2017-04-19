using System;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Methods {
	public class SystemFunctionsTests : IDisposable {
		private IContext context;

		public SystemFunctionsTests() {
			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());
			mock.SetupGet(x => x.ContextName)
				.Returns("test");

			context = mock.Object;

			context.RegisterService<IMethodResolver, SystemFunctionProvider>();
		}

		[Fact]
		public async void Add() {
			var function = SqlExpression.Function(new ObjectName("add"),
				new[] {new InvokeArgument(SqlObject.Integer(2)), new InvokeArgument(SqlObject.Integer(4))});

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression) result).Value;

			Assert.Equal(SqlObject.Integer(6), value);
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}