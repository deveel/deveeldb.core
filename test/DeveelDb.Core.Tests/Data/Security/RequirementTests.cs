﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Methods;

using Moq;

using Xunit;

namespace Deveel.Data.Security {
	public class RequirementTests : IDisposable {
		private IContext context;

		public RequirementTests() {
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
		}

		[Fact]
		public void AddRequirements() {
			var requirements = new RequirementCollection();
			requirements.RequirePrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), SqlPrivileges.Alter);

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
		[InlineData("Insert", true)]
		[InlineData("Insert, Alter", true)]
		public async Task AssertUserHasPrivileges(string privilegeString, bool expected) {
			var privilege = SqlPrivileges.Resolver.ResolvePrivilege(privilegeString);
			Assert.Equal(expected, await context.UserHasPrivileges(DbObjectType.Table, ObjectName.Parse("sys.tab1"), privilege));
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}