using System;
using System.Text;

using Deveel.Data.Security;
using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Statements {
	public class ConditionalStatementTests {
		private IContext context;

		public ConditionalStatementTests() {
			var container = new ServiceContainer();
			container.Register<IRequirementHandler<DelegatedRequirement>, DelegatedRequirementHandler>();

			var cache = new PrivilegesCache();
			cache.SetPrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), "user1", SqlPrivileges.Insert);

			container.RegisterInstance<ISecurityResolver>(cache);

			var mock = new Mock<ISession>();
			mock.Setup(x => x.Scope)
				.Returns(container);
			mock.SetupGet(x => x.User)
				.Returns(new User("user1"));

			context = mock.Object;

			var mock2 = new Mock<ISqlExpressionPreparer>();
			mock2.Setup(x => x.Prepare(It.IsAny<SqlExpression>()))
				.Returns<SqlExpression>(exp => exp);
			mock2.Setup(x => x.CanPrepare(It.IsAny<SqlExpression>()))
				.Returns(true);

			container.RegisterInstance<ISqlExpressionPreparer>(mock2.Object);
		}

		[Fact]
		public async void IfTrueReturn() {
			var test = SqlExpression.Constant(SqlObject.Boolean(true));
			var condition = new ConditionalStatement(test);
			condition.Statements.Add(new ReturnStatement(SqlExpression.Constant(SqlObject.BigInt(33))));

			var statement = condition.Prepare(context);
			var result = await statement.ExecuteAsync(context);

			Assert.NotNull(result);
			Assert.IsType<StatementExpressionResult>(result);
			Assert.Equal((SqlNumber)33L, ((StatementExpressionResult)result).Value.Value);
		}

		[Fact]
		public async void IfThenElse() {
			var test = SqlExpression.Constant(SqlObject.Boolean(false));
			var condition = new ConditionalStatement(test, new ReturnStatement(SqlExpression.Constant(SqlObject.BigInt(100))));
			condition.Statements.Add(new ReturnStatement(SqlExpression.Constant(SqlObject.BigInt(33))));

			var statement = condition.Prepare(context);

			var result = await statement.ExecuteAsync(context);

			Assert.NotNull(result);
			Assert.IsType<StatementExpressionResult>(result);
			Assert.Equal((SqlNumber)100L, ((StatementExpressionResult)result).Value.Value);
		}

		[Fact]
		public void GetStringWithNoElse() {
			var test = SqlExpression.Constant(SqlObject.Boolean(true));
			var condition = new ConditionalStatement(test);
			condition.Statements.Add(new ReturnStatement(SqlExpression.Constant(SqlObject.BigInt(33))));

			var expected = new StringBuilder();
			expected.AppendLine("IF TRUE THEN");
			expected.AppendLine("  RETURN 33;");
			expected.Append("END IF;");

			var sql = condition.ToString();

			Assert.Equal(expected.ToString(), sql);
		}

		[Fact]
		public void GetStringWithElse() {
			var test = SqlExpression.Constant(SqlObject.Boolean(true));
			var condition = new ConditionalStatement(test, new ReturnStatement(SqlExpression.Constant(SqlObject.BigInt(100))));
			condition.Statements.Add(new ReturnStatement(SqlExpression.Constant(SqlObject.BigInt(33))));

			var expected = new StringBuilder();
			expected.AppendLine("IF TRUE THEN");
			expected.AppendLine("  RETURN 33;");
			expected.AppendLine("ELSE");
			expected.AppendLine("  RETURN 100;");
			expected.Append("END IF;");

			var sql = condition.ToString();

			Assert.Equal(expected.ToString(), sql);
		}

		[Fact]
		public void SerializeConditional() {
			var test = SqlExpression.Constant(SqlObject.Boolean(true));
			var condition = new ConditionalStatement(test, new ReturnStatement(SqlExpression.Constant(SqlObject.BigInt(100))));
			condition.Statements.Add(new ReturnStatement(SqlExpression.Constant(SqlObject.BigInt(33))));

			var result = BinarySerializeUtil.Serialize(condition);

			Assert.NotNull(result);
			Assert.NotNull(result.Condition);
			Assert.NotNull(condition.Else);
			Assert.NotEmpty(condition.Statements);
			Assert.Equal(1, result.Statements.Count);
		}
	}
}