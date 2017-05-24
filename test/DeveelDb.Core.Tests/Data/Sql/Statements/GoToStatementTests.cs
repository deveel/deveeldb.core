using System;

using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Statements {
	public class GoToStatementTests {
		private IContext context;

		public GoToStatementTests() {
			var container = new ServiceContainer();

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(container);

			context = mock.Object;
		}

		[Fact]
		public async void GoToBreak() {
			var loop = new LoopStatement();
			loop.Statements.Add(new GoToStatement("exitLoop"));
			
			var exitLoop = new CodeBlockStatement("exitLoop");
			exitLoop.Statements.Add(new ExitStatement());

			loop.Statements.Add(exitLoop);

			var result = await loop.ExecuteAsync(context);

			Assert.Null(result);
		}

		[Fact]
		public void Serialize() {
			var statement = new GoToStatement("exitLoop");

			var result = BinarySerializeUtil.Serialize(statement);

			Assert.NotNull(result);
			Assert.Equal("exitLoop", result.Label);
		}

		[Fact]
		public void GetString() {
			var statement = new GoToStatement("exitLoop");

			Assert.Equal("GOTO 'exitLoop';", statement.ToString());
		}
	}
}