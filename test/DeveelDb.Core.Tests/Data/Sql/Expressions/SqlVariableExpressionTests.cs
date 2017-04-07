using System;

using Deveel.Data.Services;
using Deveel.Data.Sql.Variables;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public class SqlVariableExpressionTests : IDisposable {
		private IContext context;

		public SqlVariableExpressionTests() {
			var variable = new Variable("a", PrimitiveTypes.Boolean());
			variable.SetValue(SqlExpression.Constant(SqlObject.New(new SqlBoolean(false))));

			var resolver = new Mock<IVariableResolver>();
			resolver.Setup(x => x.ResolveVariable(It.Is<string>(s => s == "a")))
				.Returns<string>(name => variable);

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());
			context = mock.Object;

			context.Scope.RegisterInstance<IVariableResolver>(resolver.Object);
		}

		[Theory]
		[InlineData("a")]
		public void CreateVarRef(string name) {
			var varRef = SqlExpression.Variable(name);

			Assert.NotEmpty(varRef.VariableName);
			Assert.Equal(name, varRef.VariableName);
		}

		[Theory]
		[InlineData("a", ":a")]
		[InlineData("ab_test", ":ab_test")]
		public void GetVariableString(string name, string expected) {
			var varRef = SqlExpression.Variable(name);
			var sql = varRef.ToString();

			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData("a")]
		public void ReduceVariable(string name) {
			var varRef = SqlExpression.Variable(name);

			Assert.True(varRef.CanReduce);
			var result = varRef.Reduce(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;
			Assert.NotEqual(SqlObject.Unknown, value);
		}

		[Theory]
		[InlineData("b")]
		public void ReduceNotFoundVariable(string name) {
			var varRef = SqlExpression.Variable(name);

			Assert.True(varRef.CanReduce);
			var result = varRef.Reduce(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression) result).Value;
			Assert.Equal(SqlObject.Unknown, value);
		}

		public void Dispose() {
			if (context != null)
				context.Dispose();
		}
	}
}