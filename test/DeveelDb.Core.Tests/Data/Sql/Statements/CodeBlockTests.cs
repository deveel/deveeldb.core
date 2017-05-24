using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Deveel.Data.Security;
using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Statements {
	public class CodeBlockTests {
		private IContext context;

		public CodeBlockTests() {
			var container = new ServiceContainer();
			container.Register<IRequirementHandler<DelegatedRequirement>, DelegatedRequirementHandler>();

			var cache = new PrivilegesCache();
			cache.SetPrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), "user1", Privileges.Insert);

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
		public void SerializeBlock() {
			var block = new CodeBlockStatement();

			Assert.NotNull(block.Statements);
			Assert.Empty(block.Statements);
			Assert.Null(block.Label);

			block.Statements.Add(new EmptyStatement());

			Assert.NotEmpty(block.Statements);
			Assert.Equal(1, block.Statements.Count);

			var child = block.Statements.First();
			Assert.IsType<EmptyStatement>(child);
			Assert.NotNull(child);

			var parent = typeof(SqlStatement).GetTypeInfo().GetProperty("Parent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(child);
			Assert.NotNull(parent);

			var result = BinarySerializeUtil.Serialize(block);

			Assert.NotNull(result);
			Assert.NotEmpty(result.Statements);
			Assert.Equal(1, result.Statements.Count);
		}

		[Fact]
		public void AddAndRemoveStatements() {
			var block = new CodeBlockStatement("block");
			var statement = new EmptyStatement();
			block.Statements.Add(statement);
			block.Statements.Add(new EmptyStatement());

			Assert.Null(statement.Previous);
			Assert.NotNull(statement.Next);
			var parent = typeof(SqlStatement).GetTypeInfo().GetProperty("Parent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(statement);
			Assert.NotNull(parent);

			Assert.Equal(2, block.Statements.Count);
			block.Statements.Remove(statement);

			Assert.Null(statement.Next);

			Assert.Equal(1, block.Statements.Count);

			block.Statements.Clear();
			Assert.Equal(0, block.Statements.Count);
		}

		[Fact]
		public void GetString() {
			var block = new CodeBlockStatement();
			block.Statements.Add(new NullStatement());

			var sql = new StringBuilder();
			sql.AppendLine("BEGIN");
			sql.AppendLine("  NULL;");
			sql.Append("END;");

			Assert.Equal(sql.ToString(), block.ToString());
		}

		#region EmptyStatement

		class EmptyStatement : SqlStatement {
			public EmptyStatement() {
				
			}

			private EmptyStatement(SerializationInfo info)
				: base(info) {
			}

			protected override Task ExecuteStatementAsync(StatementContext context) {
				return Task.CompletedTask;
			}
		}

		#endregion
	}
}