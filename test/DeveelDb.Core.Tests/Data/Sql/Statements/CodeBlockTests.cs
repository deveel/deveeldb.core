using System;
using System.Linq;
using System.Reflection;
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

		}

		[Fact]
		public void SerializeBlock() {
			var block = new TestCodeBlock();

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
		public async void ExecuteTransfer() {
			var parentBlock = new TestCodeBlock();
			var block = new TestCodeBlock("block");
			block.Statements.Add(new EmptyStatement());
			parentBlock.Statements.Add(block);

			var executeContext = new StatementContext(context, parentBlock);
			await executeContext.TransferAsync("block");

			Assert.NotNull(executeContext.Result);
			Assert.True(executeContext.HasResult);
			Assert.IsType<StatementExpressionResult>(executeContext.Result);
			Assert.IsType<SqlConstantExpression>(((StatementExpressionResult) executeContext.Result).Value);
		}

		[Fact]
		public void AddAndRemoveStatements() {
			var block = new TestCodeBlock("block");
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

		#region TestCodeBlock

		class TestCodeBlock : CodeBlock {
			public TestCodeBlock(string label)
				: base(label) {
			}

			public TestCodeBlock()
				: base() {
			}

			private TestCodeBlock(SerializationInfo info)
				: base(info) {
			}

			protected override Task ExecuteStatementAsync(StatementContext context) {
				context.Return(SqlExpression.Constant(SqlObject.BigInt(22)));
				return Task.CompletedTask;
			}
		}

		#endregion

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