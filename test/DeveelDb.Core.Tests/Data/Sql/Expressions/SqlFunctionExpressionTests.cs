using System;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Methods;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public class SqlFunctionExpressionTests : IDisposable {
		private IContext context;

		public SqlFunctionExpressionTests() {
			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());
			context = mock.Object;

			var methodInfo = new SqlFunctionInfo(ObjectName.Parse("sys.func1"), PrimitiveTypes.Integer());
			methodInfo.Parameters.Add(new SqlMethodParameterInfo("a", PrimitiveTypes.VarChar(155)));
			methodInfo.Parameters.Add(new SqlMethodParameterInfo("b", PrimitiveTypes.Integer(),
				SqlExpression.Constant(SqlObject.Null)));
			methodInfo.SetFunctionBody(ctx => {
				return Task.FromResult(ctx.Value("a").Add(ctx.Value("b")));
			});
			var method = new SqlFunction(methodInfo);
			var resolver = new Mock<IMethodResolver>();
			resolver.Setup(x => x.ResolveMethod(It.IsAny<IContext>(), It.IsAny<Invoke>()))
				.Returns<IContext, Invoke>((context, invoke) => method);

			context.Scope.RegisterInstance<IMethodResolver>(resolver.Object);
		}

		[Fact]
		public void CreateNew() {
			var exp = SqlExpression.Function(ObjectName.Parse("sys.func2"),
				new[] {new InvokeArgument(SqlExpression.Constant(SqlObject.Bit(false)))});

			Assert.Equal(SqlExpressionType.Function, exp.ExpressionType);
			Assert.NotNull(exp.Arguments);
			Assert.NotEmpty(exp.Arguments);
			Assert.Equal(1, exp.Arguments.Length);
		}

		[Fact]
		public void GetSqlType() {
			var function = SqlExpression.Function(ObjectName.Parse("sys.Func1"), new InvokeArgument("a", SqlObject.BigInt(33)));
		
			Assert.True(function.IsReference);
			var type = function.GetSqlType(context);

			Assert.Equal(PrimitiveTypes.Integer(), type);
		}

		[Fact]
		public void ReduceFromExisting() {
			var function = SqlExpression.Function(ObjectName.Parse("sys.Func1"), 
				new InvokeArgument("a", SqlObject.BigInt(33)));
			Assert.True(function.CanReduce);
			var result = function.Reduce(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression) result).Value;
			Assert.Equal(SqlObject.Null, value);
		}

		[Fact]
		public static void GetNamedString() {
			var function = SqlExpression.Function(ObjectName.Parse("sys.Func1"), new InvokeArgument("a", SqlObject.BigInt(33)));

			const string sql = "sys.Func1(a => 33)";
			Assert.Equal(sql, function.ToString());
		}

		[Fact]
		public static void GetAnonString() {
			var function = SqlExpression.Function(ObjectName.Parse("sys.Func1"), new InvokeArgument(SqlObject.BigInt(33)));

			const string sql = "sys.Func1(33)";
			Assert.Equal(sql, function.ToString());
		}


		public void Dispose() {
			context?.Dispose();
		}
	}
}