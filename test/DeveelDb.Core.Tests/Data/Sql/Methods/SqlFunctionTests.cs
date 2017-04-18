using System;
using System.Linq;
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
			Assert.NotNull(info.ReturnType);
			Assert.Equal(SqlTypeCode.Integer, info.ReturnType.TypeCode);
		}

		[Fact]
		public static void GetString() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			var function = new SqlFunctionDelegate(info, ctx => {
				var a = ctx.Value("a");
				return Task.FromResult(a.Multiply(SqlObject.BigInt(2)));
			});

			var sql = $"FUNCTION a.func(a INTEGER) RETURNS INTEGER";
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
		public async Task ExecuteWithSequentialArgs() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			var function = new SqlFunctionDelegate(info, ctx => {
				var a = ctx.Value("a");
				return Task.FromResult(a.Multiply(SqlObject.BigInt(2)));
			});

			Assert.Equal(name, info.MethodName);
			Assert.Equal(FunctionType.Scalar, function.FunctionType);
			Assert.NotNull(info.ReturnType);
			Assert.Equal(SqlTypeCode.Integer, info.ReturnType.TypeCode);

			var result = await function.ExecuteAsync(context, SqlObject.Integer(22));

			Assert.NotNull(result);
			Assert.True(result.HasReturnedValue);
			Assert.NotNull(result.ReturnedValue);
			Assert.IsType<SqlConstantExpression>(result.ReturnedValue);
		}

		[Fact]
		public async Task ExecuteWithNamedArgs() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			var function = new SqlFunctionDelegate(info, ctx => {
				var a = ctx.Value("a");
				return Task.FromResult(a.Multiply(SqlObject.BigInt(2)));
			});

			Assert.Equal(name, info.MethodName);
			Assert.Equal(FunctionType.Scalar, function.FunctionType);
			Assert.NotNull(info.ReturnType);
			Assert.Equal(SqlTypeCode.Integer, info.ReturnType.TypeCode);

			var result = await function.ExecuteAsync(context, new InvokeArgument("a", SqlObject.Integer(22)));

			Assert.NotNull(result);
			Assert.True(result.HasReturnedValue);
			Assert.NotNull(result.ReturnedValue);
			Assert.IsType<SqlConstantExpression>(result.ReturnedValue);
		}

		[Fact]
		public async Task ExecuteWithNamedArgsAndDefaultValue() {
			var name = ObjectName.Parse("a.func");
			var info = new SqlFunctionInfo(name, PrimitiveTypes.Integer());
			info.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.Integer()));
			info.Parameters.Add(new SqlMethodParameterInfo("b",
				PrimitiveTypes.String(),
				SqlExpression.Constant(SqlObject.String(new SqlString("test")))));

			var function = new SqlFunctionDelegate(info, ctx => {
				var a = ctx.Value("a");
				var b = ctx.Value("b");
				Assert.NotNull(b);
				return Task.FromResult(a.Multiply(SqlObject.BigInt(2)));
			});

			Assert.Equal(name, info.MethodName);
			Assert.Equal(FunctionType.Scalar, function.FunctionType);
			Assert.NotNull(info.ReturnType);
			Assert.Equal(SqlTypeCode.Integer, info.ReturnType.TypeCode);

			var result = await function.ExecuteAsync(context, new InvokeArgument("a", SqlObject.Integer(22)));

			Assert.NotNull(result);
			Assert.True(result.HasReturnedValue);
			Assert.NotNull(result.ReturnedValue);
			Assert.IsType<SqlConstantExpression>(result.ReturnedValue);
		}
	}
}