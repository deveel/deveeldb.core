using System;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Variables {
	public class VariableManagerTests : IDisposable {
		private VariableManager manager;
		private IContext context;

		public VariableManagerTests() {
			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());

			context = mock.Object;
			context.RegisterService<VariableManager>();
			manager = context.ResolveService<VariableManager>();

			var obj1 = new SqlObject(PrimitiveTypes.Integer(), (SqlNumber)1);
			manager.AssignVariable("a", SqlExpression.Constant(obj1), context);

			var obj2 = new SqlObject(PrimitiveTypes.Boolean(), (SqlBoolean)false);
			manager.AssignVariable("a_b", SqlExpression.Constant(obj2), context);
		}

		[Theory]
		[InlineData("A", true, true)]
		[InlineData("a_B", true, true)]
		[InlineData("ab", false, false)]
		[InlineData("aB", true, false)]
		public void ResolveVariable(string name, bool ignoreCase, bool expected) {
			var variable = manager.ResolveVariable(name, ignoreCase);

			Assert.Equal(expected, variable != null);
		}

		[Theory]
		[InlineData("a", true)]
		[InlineData("b", false)]
		public void RemoveVariable(string name, bool expected) {
			Assert.Equal(expected, manager.RemoveVariable(name));
		}

		[Theory]
		[InlineData("a", true)]
		[InlineData("b", false)]
		public void VarivableExists(string name, bool expected) {
			Assert.Equal(expected, manager.VariableExists(name));
		}

		[Theory]
		[InlineData("A", true, true)]
		[InlineData("a_B", true, true)]
		[InlineData("ab", false, false)]
		[InlineData("aB", true, false)]
		public async Task ObjectManager_ResolveName(string name, bool ignoreCase, bool expected) {
			var objManager = (manager as IDbObjectManager);
			var result = await objManager.ResolveNameAsync(new ObjectName(name), ignoreCase);

			Assert.Equal(expected, result != null);
		}

		[Theory]
		[InlineData("a", true)]
		[InlineData("b", false)]
		[InlineData("a_b", true)]
		[InlineData("a_B", false)]
		[InlineData("A", false)]
		public async Task ObjectManager_VariableExists(string name, bool expected) {
			var objManager = (manager as IDbObjectManager);

			Assert.Equal(expected, await objManager.ObjectExistsAsync(new ObjectName(name)));
		}

		[Theory]
		[InlineData("a", true)]
		[InlineData("b", false)]
		[InlineData("a_b", true)]
		[InlineData("a_B", false)]
		public async Task ObjectManager_GetVariable(string name, bool expected) {
			var objManager = (manager as IDbObjectManager);

			var result = await objManager.GetObjectAsync(new ObjectName(name));
			Assert.Equal(expected, result != null);
		}

		[Theory]
		[InlineData("a", true)]
		[InlineData("b", false)]
		public async Task ObjectManager_DropVariable(string name, bool expected) {
			var objManager = (manager as IDbObjectManager);

			Assert.Equal(expected, await objManager.DropObjectAsync(new ObjectName(name)));
		}

		[Fact]
		public async Task ObjectManager_AlterVariable() {
			var variable = manager.GetVariable("a");

			var objManager = (manager as IDbObjectManager);
			await Assert.ThrowsAsync<NotSupportedException>(() => objManager.AlterObjectAsync(variable.VariableInfo));
		}

		public void Dispose() {
			manager.Dispose();
			context.Dispose();
		}
	}
}