using System;
using System.Threading.Tasks;

using Deveel.Data.Services;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Methods {
	public class MethodRegistryTests : IDisposable {
		private IContext context;
		private SqlMethodRegistry registry;

		public MethodRegistryTests() {
			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());

			context = mock.Object;

			registry = new SqlMethodRegistry();
		}

		[Fact]
		public void RegisterAndResolveFunction() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			info.SetFunctionBody(ctx => {
				var a = ctx.Value("a");
				return Task.FromResult(a.Multiply(SqlObject.BigInt(2)));
			});

			registry.RegisterFunction(info);

			var invoke = new Invoke(name);
			invoke.Arguments.Add(new InvokeArgument("a", SqlObject.Integer(11)));

			var function = (registry as IMethodResolver).ResolveMethod(context, invoke);

			Assert.NotNull(function);
		}

		public void Dispose() {
			registry.Dispose();
			context.Dispose();
		}
	}
}