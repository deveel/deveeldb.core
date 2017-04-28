using System;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Services;
using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Variables;

using Moq;

using Xunit;

namespace Deveel.Data {
	public class ObjectsContextTests : IDisposable {
		private IContext context;

		public ObjectsContextTests() {
			var config = new Configuration.Configuration();
			config.SetValue("currentSchema", "test");

			var objManager = new Mock<IDbObjectManager>();
			objManager.SetupGet(x => x.ObjectType)
				.Returns(DbObjectType.Variable);
			objManager.Setup(x => x.GetObjectInfoAsync(It.IsAny<ObjectName>()))
				.Returns<ObjectName>(name => Task.FromResult<IDbObjectInfo>(
					new VariableInfo(name.Name, PrimitiveTypes.String(), true, SqlExpression.Constant(SqlObject.Integer(2)))));
			objManager.Setup(x => x.GetObjectAsync(It.IsAny<ObjectName>()))
				.Returns<ObjectName>(name => Task.FromResult<IDbObject>(
					new Variable(name.Name, PrimitiveTypes.String(), true, SqlExpression.Constant(SqlObject.Integer(2)))));
			objManager.Setup(x => x.ResolveNameAsync(It.IsAny<ObjectName>(), It.IsAny<bool>()))
				.Returns<ObjectName, bool>((name, ignoreCase) => Task.FromResult(name));

			var container = new ServiceContainer();
			container.RegisterInstance<IDbObjectManager>(objManager.Object);

			var mock = new Mock<IContext>();
			mock.As<IConfigurationScope>()
				.SetupGet(x => x.Configuration).Returns(config);
			mock.SetupGet(x => x.Scope)
				.Returns(container);

			context = mock.Object;
		}

		[Theory]
		[InlineData("table1", "test.table1")]
		[InlineData("test.tab1", "test.tab1")]
		public void QualifyName(string name, string expected) {
			var result = context.QualifyName(ObjectName.Parse(name));

			var exp = ObjectName.Parse(expected);
			Assert.Equal(exp, result);
		}

		[Fact]
		public async void GetObject() {
			var obj = await context.GetObjectAsync(DbObjectType.Variable, new ObjectName("var1"));

			Assert.NotNull(obj);
			Assert.IsType<Variable>(obj);
			Assert.NotNull(obj.ObjectInfo);
			Assert.NotNull(obj.ObjectInfo.FullName);
			Assert.Equal(new ObjectName("var1"), obj.ObjectInfo.FullName);
		}

		[Fact]
		public async void GetObjectInfo() {
			var objInfo = await context.GetObjectInfoAsync(DbObjectType.Variable, new ObjectName("var1"));

			Assert.NotNull(objInfo);
			Assert.IsType<VariableInfo>(objInfo);
			Assert.Equal(DbObjectType.Variable, objInfo.ObjectType);
			Assert.NotNull(objInfo.FullName);
			Assert.Equal(new ObjectName("var1"), objInfo.FullName);
		}

		[Fact]
		public async void ResolveName() {
			var name = await context.ResolveNameAsync(new ObjectName("var2"));

			Assert.NotNull(name);
			Assert.Equal(new ObjectName("var2"), name);
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}