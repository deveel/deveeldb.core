using System;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

using Moq;

using Xunit;

namespace Deveel.Data.Query {
	public class SimpleNodeTests : IDisposable {
		private IContext context;

		public SimpleNodeTests() {
			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());

			context = mock.Object;

			var tableInfo = new TableInfo(new ObjectName("tab1"));
			tableInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			tableInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.Boolean()));
			tableInfo.Columns.Add(new ColumnInfo("c", PrimitiveTypes.Double()));

			var table = new TemporaryTable(tableInfo);
			table.AddRow(new[] { SqlObject.Integer(23), SqlObject.Boolean(true), SqlObject.Double(5563.22) });
			table.AddRow(new[] { SqlObject.Integer(54), SqlObject.Boolean(null), SqlObject.Double(921.001) });
			table.AddRow(new[] { SqlObject.Integer(23), SqlObject.Boolean(true), SqlObject.Double(2010.221) });

			table.BuildIndex();

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

		[Fact]
		public async Task SimpleSelect() {
			var fetchNode = new FetchTableNode(new ObjectName("tab1"));
			var selectNode = new SimpleSelectNode(fetchNode, ObjectName.Parse("tab1.a"), SqlExpressionType.Equal,
				SqlExpression.Constant(SqlObject.Integer(23)));

			var result = await selectNode.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.Equal(2, result.RowCount);
		}

		[Fact]
		public async Task QuantifySelect() {
			var columnName = ObjectName.Parse("tab1.a");
			var exp1 = SqlExpression.Constant(SqlObject.New((SqlNumber)23));
			var exp2 = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(23)));
			var array = SqlExpression.Constant(SqlObject.Array(new SqlArray(new[] { exp1, exp2 })));
			var binary = SqlExpression.Equal(SqlExpression.Reference(columnName), array);
			var expression = SqlExpression.Quantify(SqlExpressionType.All, binary);

			var fetchNode = new FetchTableNode(new ObjectName("tab1"));
			var selectNode = new QuantifiedSelectNode(fetchNode, expression);

			var result = await selectNode.ReduceAsync(context);

			Assert.Equal(2, result.RowCount);
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}