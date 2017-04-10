using System;
using System.Threading;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Methods {
	public class SqlFunctionTests {
		private IContext context;

		public SqlFunctionTests() {
			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());
			mock.SetupGet(x => x.ContextName)
				.Returns("test");

			context = mock.Object;
		}

		[Fact]
		public static void MakeFunctionInfo() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));

			Assert.Equal(name, info.MethodName);
			Assert.Equal(FunctionType.Scalar, info.FunctionType);
			Assert.NotNull(info.ReturnType);
			Assert.Equal(SqlTypeCode.Integer, info.ReturnType.TypeCode);
		}

		[Fact]
		public static void GetString() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			info.SetFunctionBody(ctx => {
				var a = ctx.Value("a");
				return Task.FromResult(a.Multiply(SqlObject.BigInt(2)));
			});

			var function = new SqlFunction(info);

			var sql = $"FUNCTION a.func(a INTEGER) RETURNS INTEGER IS{Environment.NewLine}  [delegate]";
			Assert.Equal(sql, function.ToString());
		}

		[Fact]
		public static void MatchInvoke() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));

			var invoke = new Invoke(name, new []{new InvokeArgument(SqlObject.BigInt(11)) });

			Assert.True(info.Matches(null, invoke));
		}

		[Fact]
		public async Task ExecuteFunctionWithSequentialArgs() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			info.SetFunctionBody(ctx => {
				var a = ctx.Value("a");
				return Task.FromResult(a.Multiply(SqlObject.BigInt(2)));
			});

			var function = new SqlFunction(info);

			Assert.Equal(name, info.MethodName);
			Assert.Equal(FunctionType.Scalar, info.FunctionType);
			Assert.NotNull(info.ReturnType);
			Assert.Equal(SqlTypeCode.Integer, info.ReturnType.TypeCode);
			Assert.NotNull(info.Body);
			Assert.IsType<SqlMethodDelegate>(info.Body);

			var result = await function.ExecuteAsync(context, SqlObject.Integer(22));

			Assert.NotNull(result);
			Assert.True(result.HasReturnedValue);
			Assert.NotNull(result.ReturnedValue);
			Assert.IsType<SqlConstantExpression>(result.ReturnedValue);
		}

		[Fact]
		public async Task ExecuteFunctionWithNamedArgs() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			info.SetFunctionBody(ctx => {
				var a = ctx.Value("a");
				return Task.FromResult(a.Multiply(SqlObject.BigInt(2)));
			});

			var function = new SqlFunction(info);

			Assert.Equal(name, info.MethodName);
			Assert.Equal(FunctionType.Scalar, info.FunctionType);
			Assert.NotNull(info.ReturnType);
			Assert.Equal(SqlTypeCode.Integer, info.ReturnType.TypeCode);
			Assert.NotNull(info.Body);
			Assert.IsType<SqlMethodDelegate>(info.Body);

			var result = await function.ExecuteAsync(context, new InvokeArgument("a", SqlObject.Integer(22)));

			Assert.NotNull(result);
			Assert.True(result.HasReturnedValue);
			Assert.NotNull(result.ReturnedValue);
			Assert.IsType<SqlConstantExpression>(result.ReturnedValue);
		}

		[Fact]
		public async Task ExecuteFunctionWithNamedArgsAndDefaultValue() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			info.Parameters.Add(new SqlMethodParameterInfo("b",
				PrimitiveTypes.String(),
				SqlExpression.Constant(SqlObject.String(new SqlString("test")))));
			info.SetFunctionBody(ctx => {
				var a = ctx.Value("a");
				var b = ctx.Value("b");
				Assert.NotNull(b);
				return Task.FromResult(a.Multiply(SqlObject.BigInt(2)));
			});

			var function = new SqlFunction(info);

			Assert.Equal(name, info.MethodName);
			Assert.Equal(FunctionType.Scalar, info.FunctionType);
			Assert.NotNull(info.ReturnType);
			Assert.Equal(SqlTypeCode.Integer, info.ReturnType.TypeCode);
			Assert.NotNull(info.Body);
			Assert.IsType<SqlMethodDelegate>(info.Body);

			var result = await function.ExecuteAsync(context, new InvokeArgument("a", SqlObject.Integer(22)));

			Assert.NotNull(result);
			Assert.True(result.HasReturnedValue);
			Assert.NotNull(result.ReturnedValue);
			Assert.IsType<SqlConstantExpression>(result.ReturnedValue);
		}

	}
}