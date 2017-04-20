using System;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Methods {
	public class SqlProcedureTests : IDisposable {
		private IContext context;

		public SqlProcedureTests() {
			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());
			mock.SetupGet(x => x.ContextName)
				.Returns("test");

			context = mock.Object;
		}

		[Fact]
		public static void MakeProcedureInfo() {
			var name = ObjectName.Parse("a.proc");
			var info = new SqlMethodInfo(name);
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));

			Assert.Equal(name, info.MethodName);
			Assert.NotEmpty(info.Parameters);
		}

		[Fact]
		public static void GetString() {
			var name = ObjectName.Parse("a.proc");
			var info = new SqlMethodInfo(name);
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			var function = new SqlProcedureDelegate(info, ctx => {
				var a = ctx.Value("a");
				var b = a.Multiply(SqlObject.BigInt(2));

				Console.Out.WriteLine("a * 2 = {0}", b);
			});

			var sql = $"PROCEDURE a.proc(a INTEGER)";
			Assert.Equal(sql, function.ToString());
		}

		[Fact]
		public static void MatchInvoke() {
			var name = ObjectName.Parse("a.proc");
			var info = new SqlMethodInfo(name);
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			var procedure = new SqlProcedureDelegate(info, methodContext => Task.CompletedTask);

			var invoke = new Invoke(name, new[] { new InvokeArgument(SqlObject.BigInt(11)) });

			Assert.True(procedure.Matches(null, invoke));
		}

		[Fact]
		public async Task ExecuteWithSequentialArgs() {
			var name = ObjectName.Parse("a.proc");
			var info = new SqlMethodInfo(name);
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			var procedure = new SqlProcedureDelegate(info, ctx => {
				var a = ctx.Value("a");
				var b = a.Multiply(SqlObject.BigInt(2));
				Console.Out.WriteLine("a * 2 = {0}", b);
			});

			Assert.Equal(name, info.MethodName);

			var result = await procedure.ExecuteAsync(context, SqlObject.Integer(22));

			Assert.NotNull(result);
			Assert.False(result.HasReturnedValue);
		}

		[Fact]
		public async Task ExecuteWithNamedArgs() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlMethodInfo(name);
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			var procedure = new SqlProcedureDelegate(info, ctx => {
				var a = ctx.Value("a");
				var b = a.Multiply(SqlObject.BigInt(2));
				Console.Out.WriteLine("a * 2 = {0}", b);
			});

			Assert.Equal(name, info.MethodName);

			var result = await procedure.ExecuteAsync(context, new InvokeArgument("a", SqlObject.Integer(22)));

			Assert.NotNull(result);
			Assert.False(result.HasReturnedValue);
		}

		[Fact]
		public async Task ExecuteWithNamedArgsAndDefaultValue() {
			var name = ObjectName.Parse("a.proc");
			var info = new SqlMethodInfo(name);
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			info.Parameters.Add(new SqlMethodParameterInfo("b",
				PrimitiveTypes.String(),
				SqlExpression.Constant(SqlObject.String(new SqlString("test")))));

			var procedure = new SqlProcedureDelegate(info, ctx => {
				var a = ctx.Value("a");
				var b = ctx.Value("b");
				Assert.NotNull(b);
				Console.Out.WriteLine("a * 2 = {0}", a.Multiply(SqlObject.BigInt(2)));
			});

			Assert.Equal(name, info.MethodName);

			var result = await procedure.ExecuteAsync(context, new InvokeArgument("a", SqlObject.Integer(22)));

			Assert.NotNull(result);
			Assert.False(result.HasReturnedValue);
		}


		public void Dispose() {
			context?.Dispose();
		}
	}
}