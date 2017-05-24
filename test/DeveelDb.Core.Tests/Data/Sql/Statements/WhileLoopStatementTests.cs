using System;
using System.Text;

using Deveel.Data.Security;
using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Statements {
	public class WhileLoopStatementTests : IDisposable {
		private IContext context;

		public WhileLoopStatementTests() {
			var container = new ServiceContainer();

			var mock = new Mock<ISession>();
			mock.Setup(x => x.Scope)
				.Returns(container);
			mock.SetupGet(x => x.User)
				.Returns(new User("user1"));
			mock.Setup(x => x.Dispose())
				.Callback(container.Dispose);

			context = mock.Object;

			var mock2 = new Mock<ISqlExpressionPreparer>();
			mock2.Setup(x => x.Prepare(It.IsAny<SqlExpression>()))
				.Returns<SqlExpression>(exp => exp);
			mock2.Setup(x => x.CanPrepare(It.IsAny<SqlExpression>()))
				.Returns(true);

			container.RegisterInstance<ISqlExpressionPreparer>(mock2.Object);
		}

		[Fact]
		public async void WhileTrueExit() {
			var loop = new WhileLoopStatement(SqlExpression.Constant(SqlObject.Boolean(true)));
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit));

			var statement = loop.Prepare(context);

			var result = await statement.ExecuteAsync(context);

			Assert.Null(result);
		}

		[Fact]
		public void SerializeSimpleLoop() {
			var loop = new WhileLoopStatement(SqlExpression.Constant(SqlObject.Boolean(true)));
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit));

			var result = BinarySerializeUtil.Serialize(loop);

			Assert.NotNull(result);
			Assert.NotNull(result.Condition);
			Assert.IsType<SqlConstantExpression>(result.Condition);
		}

		[Fact]
		public void GetStringWithLabel() {
			var loop = new WhileLoopStatement(SqlExpression.Constant(SqlObject.Boolean(true)), "l1");
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit));

			var sql = new StringBuilder();
			sql.AppendLine("<<l1>>");
			sql.AppendLine("WHILE TRUE");
			sql.AppendLine("LOOP");
			sql.AppendLine("  EXIT;");
			sql.Append("END LOOP;");

			Assert.Equal(sql.ToString(), loop.ToString());
		}

		[Fact]
		public void GetStringWithoutLabel() {
			var loop = new WhileLoopStatement(SqlExpression.Constant(SqlObject.Boolean(true)));
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit));

			var sql = new StringBuilder();
			sql.AppendLine("WHILE TRUE");
			sql.AppendLine("LOOP");
			sql.AppendLine("  EXIT;");
			sql.Append("END LOOP;");

			Assert.Equal(sql.ToString(), loop.ToString());
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}