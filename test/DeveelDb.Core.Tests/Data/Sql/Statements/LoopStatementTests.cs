using System;

using Deveel.Data.Security;
using Deveel.Data.Serialization;
using Deveel.Data.Services;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Statements
{
    public class LoopStatementTests {
	    private IContext context;

	    public LoopStatementTests() {
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
	    public async void EmptyLoopAndExit() {
		    var loop = new LoopStatement();
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit));

			var executeContext = new StatementContext(context, loop);
			await loop.ExecuteAsync(executeContext);
		}

		[Fact]
	    public async void LabeledLoopAndExit_WasFound() {
			var loop = new LoopStatement("l1");
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit, "l1"));

			var executeContext = new StatementContext(context, loop);
			await loop.ExecuteAsync(executeContext);
		}

	    [Fact]
	    public async void LabeledLoopAndExit_NotFound()
	    {
		    var loop = new LoopStatement("l1");
		    loop.Statements.Add(new LoopControlStatement(LoopControlType.Exit, "l2"));

		    var executeContext = new StatementContext(context, loop);
		    await Assert.ThrowsAnyAsync<SqlStatementException>(() => loop.ExecuteAsync(executeContext));
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
	}
}
