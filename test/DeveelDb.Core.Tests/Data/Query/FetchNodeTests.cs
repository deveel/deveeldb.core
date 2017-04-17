using System;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql;
using Deveel.Data.Sql.Tables;

using Moq;

using Xunit;

namespace Deveel.Data.Query {
	public class FetchNodeTests : IDisposable {
		private IContext context;

		public FetchNodeTests() {
			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());

			context = mock.Object;

			var tableInfo = new TableInfo(new ObjectName("tab1"));
			tableInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			var table = new TemporaryTable(tableInfo);

			var tableManager = new Mock<IDbObjectManager>();
			tableManager.SetupGet(x => x.ObjectType)
				.Returns(DbObjectType.Table);
			tableManager.Setup(x => x.GetObjectAsync(It.IsAny<ObjectName>()))
				.Returns<ObjectName>(name => Task.FromResult((IDbObject) table));

			context.RegisterInstance<IDbObjectManager>(tableManager.Object);
		}

		[Fact]
		public async Task FetchTable() {
			var node = new FetchTableNode(new ObjectName("tab1"));
			var table = await node.ReduceAsync(context);

			Assert.NotNull(table);
			Assert.Equal(node.TableName, table.TableInfo.TableName);
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}