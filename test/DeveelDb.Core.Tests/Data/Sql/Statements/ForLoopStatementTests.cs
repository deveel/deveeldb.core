﻿using System;

using Deveel.Data.Security;
using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Statements {
	public class ForLoopStatementTests : IDisposable {
		private IContext context;

		public ForLoopStatementTests() {
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
			mock.Setup(x => x.Dispose())
				.Callback(container.Dispose);

			context = mock.Object;
		}

		[Fact]
		public async void ForwardForLoop() {
			var loop = new ForLoopStatement("i",
				SqlExpression.Constant(SqlObject.BigInt(0)),
				SqlExpression.Constant(SqlObject.BigInt(5)));
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Continue));

			var statement = loop.Prepare(context);
			var result = await statement.ExecuteAsync(context);

			Assert.Null(result);
		}

		[Fact]
		public async void ReverseForLoop() {
			var loop = new ForLoopStatement("i",
				SqlExpression.Constant(SqlObject.BigInt(0)),
				SqlExpression.Constant(SqlObject.BigInt(5)),
				true);
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Continue));

			var statement = loop.Prepare(context);
			var result = await statement.ExecuteAsync(context);

			Assert.Null(result);
		}

		[Fact]
		public void SerializeForLoop() {
			var loop = new ForLoopStatement("i",
				SqlExpression.Constant(SqlObject.BigInt(0)),
				SqlExpression.Constant(SqlObject.BigInt(5)),
				true);
			loop.Statements.Add(new LoopControlStatement(LoopControlType.Continue));

			var result = BinarySerializeUtil.Serialize(loop);

			Assert.NotNull(result);
			Assert.NotNull(result.LowerBound);
			Assert.NotNull(result.UpperBound);
		}


		public void Dispose() {
			context.Dispose();
		}
	}
}