using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Services;

using Moq;

using Xunit;

namespace Deveel.Data.Security {
	public class RequirementTests : IDisposable {
		private IContext context;

		public RequirementTests() {
			var container = new ServiceContainer();
			container.Register<IRequirementHandler<DelegatedRequirement>, DelegatedRequirementHandler>();

			var cache = new PrivilegesCache();
			cache.SetPrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), "user1", Privileges.Insert);

			container.RegisterInstance<ISecurityResolver>(cache);

			var mock = new Mock<ISecurityContext>();
			mock.Setup(x => x.Scope)
				.Returns(container);
			mock.SetupGet(x => x.User)
				.Returns(new User("user1"));

			context = mock.Object;
		}

		[Fact]
		public void AddRequirements() {
			var requirements = new RequirementCollection();
			requirements.RequirePrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), Privileges.Alter);

			Assert.NotEmpty(requirements);
			Assert.Equal(1, requirements.Count());
		}

		[Fact]
		public void ContextAuthenticated() {
			Assert.NotNull(context.User());
			Assert.Equal("user1", context.User().Name);
			Assert.False(context.User().IsSystem);
		}

		[Theory]
		[InlineData(Privileges.Insert, true)]
		[InlineData(Privileges.Compact, false)]
		[InlineData(Privileges.Insert | Privileges.Alter, true)]
		public void AssertUserHasPrivileges(Privileges privileges, bool expected) {
			Assert.Equal(expected, context.UserHasPrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), privileges));
		}

		[Fact]
		public void CheckRequirements() {
			var requirements = new RequirementCollection();
			requirements.RequirePrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), Privileges.Insert);

			context.Scope.RegisterInstance<IRequirementCollection>(requirements);

			context.CheckRequirements();
		}

		[Fact]
		public void FailedCheckRequirements() {
			var requirements = new RequirementCollection();
			requirements.RequirePrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), Privileges.Alter);

			context.Scope.RegisterInstance<IRequirementCollection>(requirements);

			Assert.ThrowsAny<UnauthorizedAccessException>(() => context.CheckRequirements());
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}