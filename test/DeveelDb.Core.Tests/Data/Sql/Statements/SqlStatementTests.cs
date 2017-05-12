using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Security;
using Deveel.Data.Services;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Statements {
	public class SqlStatementTests : IDisposable {
		private IContext context;

		public SqlStatementTests() {
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
		public async Task ExecuteWithBadprivileges() {
			var requirements = new RequirementCollection();
			requirements.RequirePrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), Privileges.Alter);

			var statement = new TestStatement {
				Requirements = requirements,
				Location = new LocationInfo(0, 0)
			};

			await Assert.ThrowsAnyAsync<UnauthorizedAccessException>(() => statement.ExecuteAsync(context));
		}

		[Fact]
		public async Task ExecutePrivileged() {
			var requirements = new RequirementCollection();
			requirements.RequirePrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), Privileges.Insert);

			var statement = new TestStatement {
				Requirements = requirements,
				Location = new LocationInfo(0, 0)
			};

			await statement.ExecuteAsync(context);
		}

		public void Dispose() {
			context.Dispose();
		}

		#region TestStatement

		class TestStatement : SqlStatement {
			public IEnumerable<IRequirement> Requirements { get; set; }

			public Func<IContext, Task<SqlStatementResult>> Body { get; set; }

			protected override void Require(IRequirementCollection requirements) {
				requirements.Append(Requirements);
			}

			protected override Task<SqlStatementResult> ExecuteStatementAsync(IContext context) {
				if (Body == null)
					return Task.FromResult<SqlStatementResult>(null);

				return Body(context);
			}
		}

		#endregion
	}
}