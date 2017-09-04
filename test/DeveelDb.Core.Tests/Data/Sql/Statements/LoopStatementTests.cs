using System;
using System.Text;

using Deveel.Data.Security;
using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Statements {
	public class LoopStatementTests {
		private IContext context;

		public LoopStatementTests() {
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
		public async void EmptyLoopAndExit() {
			var loop = new LoopStatement();
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit));

			var statement = loop.Prepare(context);
			var result = await statement.ExecuteAsync(context);

			Assert.Null(result);
		}

		[Fact]
		public async void LabeledLoopAndExit_WasFound() {
			var loop = new LoopStatement("l1");
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit, "l1"));

			var statement = loop.Prepare(context);
			var result = await statement.ExecuteAsync(context);

			Assert.Null(result);
		}

		[Fact]
		public async void LabeledLoopAndExit_NotFound() {
			var loop = new LoopStatement("l1");
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit, "l2"));

			var statement = loop.Prepare(context);
			await Assert.ThrowsAnyAsync<SqlStatementException>(() => statement.ExecuteAsync(context));
		}


		[Fact]
		public void SerializeEmptyLoop() {
			var loop = new LoopStatement();
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit));

			var result = BinarySerializeUtil.Serialize(loop);

			Assert.NotNull(result);
			Assert.Null(result.Label);
			Assert.NotEmpty(result.Statements);
			Assert.Equal(1, result.Statements.Count);
		}

		[Theory]
		[InlineData(LoopControlType.Continue, null, null, "CONTINUE;")]
		[InlineData(LoopControlType.Exit, "l1", true, "EXIT 'l1' WHEN TRUE;")]
		[InlineData(LoopControlType.Exit, null, null, "EXIT;")]
		[InlineData(LoopControlType.Exit, null, false, "EXIT WHEN FALSE;")]
		public void GetControlString(LoopControlType controlType, string label, bool? when, string expected) {
			var whenExp = when == null ? null : SqlExpression.Constant(SqlObject.Boolean(when.Value));
			var statement = new LoopControlStatement(controlType, label, whenExp);

			var sql = statement.ToString();
			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData("l1", true, "EXIT 'l1' WHEN TRUE;")]
		[InlineData(null, null, "EXIT;")]
		[InlineData(null, false, "EXIT WHEN FALSE;")]
		public void GetExitString(string label, bool? when, string expected) {
			var whenExp = when == null ? null : SqlExpression.Constant(SqlObject.Boolean(when.Value));
			var statement = new ExitStatement(label, whenExp);

			var sql = statement.ToString();
			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData("l1", true, "CONTINUE 'l1' WHEN TRUE;")]
		[InlineData(null, null, "CONTINUE;")]
		[InlineData(null, false, "CONTINUE WHEN FALSE;")]
		public void GetContinueString(string label, bool? when, string expected)
		{
			var whenExp = when == null ? null : SqlExpression.Constant(SqlObject.Boolean(when.Value));
			var statement = new ContinueStatement(label, whenExp);

			var sql = statement.ToString();
			Assert.Equal(expected, sql);
		}


		[Fact]
		public void GetLoopString() {
			var loop = new LoopStatement("l1");
			loop.Statements.Add(new ExitStatement(SqlExpression.Constant(SqlObject.Boolean(true))));

			var expected = new StringBuilder();
			expected.AppendLine("<<l1>>");
			expected.AppendLine("LOOP");
			expected.AppendLine("  EXIT WHEN TRUE;");
			expected.Append("END LOOP;");

			var sql = loop.ToString();
			Assert.Equal(expected.ToString(), sql);
		}
	}
}