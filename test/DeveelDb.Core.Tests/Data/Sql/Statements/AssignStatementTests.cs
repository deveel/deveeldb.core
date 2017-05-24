using System;
using System.Linq;

using Deveel.Data.Diagnostics;
using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Variables;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Statements {
	public class AssignStatementTests {
		private IContext context;

		public AssignStatementTests() {
			var container = new ServiceContainer();

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(container);
			mock.As<IVariableScope>()
				.Setup(x => x.Variables)
				.Returns(new VariableManager());

			context = mock.Object;

			var mock2 = new Mock<ISqlExpressionPreparer>();
			mock2.Setup(x => x.Prepare(It.IsAny<SqlExpression>()))
				.Returns<SqlExpression>(exp => exp);
			mock2.Setup(x => x.CanPrepare(It.IsAny<SqlExpression>()))
				.Returns(true);

			container.RegisterInstance<ISqlExpressionPreparer>(mock2.Object);
		}

		[Fact]
		public async void AssignNewWariable() {
			var assign = new AssignStatement("a", SqlExpression.Constant(SqlObject.Bit(true)));

			var statement = assign.Prepare(context);
			var result = await statement.ExecuteAsync(context);

			Assert.NotNull(result);
			Assert.IsType<StatementExpressionResult>(result);

			var variable = context.ResolveVariable("a");
			Assert.NotNull(variable);
			Assert.Equal("a", variable.Name);
			Assert.IsType<SqlConstantExpression>(variable.Value);
		}

		[Fact]
		public void GetMetadata() {
			var assign = new AssignStatement("a", SqlExpression.Constant(SqlObject.Bit(true)));
			var statementContext = new StatementContext(context, assign);

			var meta = (statementContext as IEventSource).Metadata.ToDictionary(x => x.Key, y => y.Value);
			Assert.NotNull(meta);
			Assert.NotEmpty(meta);
			Assert.True(meta.ContainsKey("statement.var"));
			Assert.True(meta.ContainsKey("statement.value"));
		}

		[Fact]
		public void Serialize() {
			var statement = new AssignStatement("a", SqlExpression.Constant(SqlObject.Bit(true)));
			var result = BinarySerializeUtil.Serialize(statement);

			Assert.NotNull(result);
			Assert.NotNull(result.Variable);
			Assert.NotNull(result.Value);
		}

		[Fact]
		public void GetString() {
			var statement = new AssignStatement("a", SqlExpression.Constant(SqlObject.Bit(true)));

			Assert.Equal("a := 1;", statement.ToString());
		}
	}
}