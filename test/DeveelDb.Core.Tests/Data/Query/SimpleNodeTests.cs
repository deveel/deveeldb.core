using System;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Methods;
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

			var tableInfo1 = new TableInfo(new ObjectName("tab1"));
			tableInfo1.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			tableInfo1.Columns.Add(new ColumnInfo("b", PrimitiveTypes.Boolean()));
			tableInfo1.Columns.Add(new ColumnInfo("c", PrimitiveTypes.Double()));

			var table1 = new TemporaryTable(tableInfo1);
			table1.AddRow(new[] { SqlObject.Integer(23), SqlObject.Boolean(true), SqlObject.Double(5563.22) });
			table1.AddRow(new[] { SqlObject.Integer(54), SqlObject.Boolean(null), SqlObject.Double(921.001) });
			table1.AddRow(new[] { SqlObject.Integer(23), SqlObject.Boolean(true), SqlObject.Double(2010.221) });

			table1.BuildIndex();

			var tableInfo2 = new TableInfo(new ObjectName("tab2"));
			tableInfo2.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			tableInfo2.Columns.Add(new ColumnInfo("aa", PrimitiveTypes.BigInt()));
			tableInfo2.Columns.Add(new ColumnInfo("b1", PrimitiveTypes.Boolean()));

			var table2 = new TemporaryTable(tableInfo2);
			table2.AddRow(new []{SqlObject.Integer(89), SqlObject.BigInt(120111233), SqlObject.Boolean(true) });
			table2.AddRow(new[] { SqlObject.Integer(127), SqlObject.BigInt(-21445665), SqlObject.Boolean(false) });

			table2.BuildIndex();

			var tableManager = new Mock<IDbObjectManager>();
			tableManager.SetupGet(x => x.ObjectType)
				.Returns(DbObjectType.Table);
			tableManager.Setup(x => x.GetObjectAsync(It.Is<ObjectName>(name => name.Name == "tab1")))
				.Returns<ObjectName>(name => Task.FromResult((IDbObject) table1));
			tableManager.Setup(x => x.GetObjectAsync(It.Is<ObjectName>(name => name.Name == "tab2")))
				.Returns<ObjectName>(name => Task.FromResult<IDbObject>(table2));

			context.RegisterInstance<IDbObjectManager>(tableManager.Object);
			context.RegisterService<ITableCache, InMemoryTableCache>();

			context.RegisterService<IMethodResolver, SystemFunctionProvider>();
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

		[Fact]
		public async Task UnionWithSelf() {
			var fetchNode1 = new FetchTableNode(new ObjectName("tab1"));
			var fetchNode2 = new FetchTableNode(new ObjectName("tab1"));
			var unionNode = new LogicalUnionNode(fetchNode1, fetchNode2);

			var result = await unionNode.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.Equal(3, result.RowCount);
		}

		[Fact]
		public async Task SampleSimpleSelect() {
			var fetchNode = new FetchTableNode(new ObjectName("tab1"));
			var selectNode = new SimpleSelectNode(fetchNode, ObjectName.Parse("tab1.a"), SqlExpressionType.Equal,
				SqlExpression.Constant(SqlObject.Integer(23)));

			var sample = await selectNode.SampleAsync(context);

			Assert.NotNull(sample);
			Assert.NotNull(sample.SampleInfo);
			Assert.Equal(2, sample.SampleInfo.RowCount);
			Assert.NotNull(sample.ChildNodes);
			Assert.NotEmpty(sample.ChildNodes);
			Assert.Equal(1, sample.ChildNodes.Count());
			Assert.Equal(3, sample.ChildNodes.ElementAt(0).SampleInfo.RowCount);
		}

		[Fact]
		public async Task Subset() {
			var tableName = new ObjectName("tab1");
			var columnName = new ObjectName(tableName, "a");
			var fetchNode = new FetchTableNode(tableName);
			var subsetNode = new SubsetNode(fetchNode, new[] {columnName}, new[] {columnName});

			var result = await subsetNode.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.Equal(1, result.TableInfo.Columns.Count);
			Assert.Equal(3, result.RowCount);
		}

		[Fact]
		public async Task FullSelectOfRoot() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);
			var exp = SqlExpression.Equal(SqlExpression.Reference(new ObjectName(tableName, "a")),
				SqlExpression.Constant(SqlObject.Integer(23)));
			var fullSelectNode = new FullSelectNode(fetchNode, exp);

			var result = await fullSelectNode.ReduceAsync(context);

			Assert.Equal(2, result.RowCount);
		}

		[Fact]
		public async Task SimpleSort() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);
			var sortNode = new SortNode(fetchNode, new ObjectName[] {new ObjectName(tableName, "a")}, new bool[] {false});

			var result = await sortNode.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.Equal(3, result.RowCount);

			var value1 = await result.GetValueAsync(0, 0);
			Assert.Equal(SqlObject.Integer(54), value1);
		}

		[Fact]
		public async Task ConstantSelect() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);
			var constantNode = new ConstantSelectNode(fetchNode, SqlExpression.Constant(SqlObject.Boolean(false)));

			var result = await constantNode.ReduceAsync(context);

			Assert.Equal(0, result.RowCount);
		}

		[Fact]
		public async Task LimitSelect() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);
			var limitNode = new LimitResultNode(fetchNode, 1, 1);

			var result = await limitNode.ReduceAsync(context);

			Assert.Equal(1, result.RowCount);
		}

		[Fact]
		public async Task NonCorrelatedSelect() {
			var tableName1 = new ObjectName("tab1");
			var fetchNode1 = new FetchTableNode(tableName1);
			var subset1 = new SubsetNode(fetchNode1, new []{new ObjectName(tableName1, "a")});
			var tableName2 = new ObjectName("tab2");
			var fetchNode2 = new FetchTableNode(tableName2);
			var subset2 = new SubsetNode(fetchNode2, new[] { new ObjectName(tableName2, "a") });
			var nonCorrelated = new NonCorrelatedSelectNode(subset1, subset2, new[] {new ObjectName(tableName1, "a")},
				SqlExpressionType.All, SqlExpressionType.LessThan);

			var result = await nonCorrelated.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.Equal(3, result.RowCount);
		}

		[Fact]
		public async Task SimplePatternSelect() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);
			var exp = SqlExpression.Equal(SqlExpression.Reference(new ObjectName(tableName, "a")),
				SqlExpression.Constant(SqlObject.Integer(23)));

			var simplePattern = new SimplePatternSelectNode(fetchNode, exp);

			var result = await simplePattern.ReduceAsync(context);

			Assert.Equal(2, result.RowCount);
		}

		[Fact]
		public async Task SimpleJoin() {
			var tableName1 = new ObjectName("tab1");
			var fetchNode1 = new FetchTableNode(tableName1);
			var tableName2 = new ObjectName("tab2");
			var fetchNode2 = new FetchTableNode(tableName2);
			var joinNode = new JoinNode(fetchNode1,
				fetchNode2,
				ObjectName.Parse("tab1.b"),
				SqlExpressionType.Equal,
				SqlExpression.Reference(ObjectName.Parse("tab2.b1")));

			var result = await joinNode.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.Equal(2, result.RowCount);
			Assert.Equal(6, result.TableInfo.Columns.Count);

			var value1 = await result.GetValueAsync(0, 0);
			Assert.Equal(SqlObject.Integer(23), value1);
		}

		[Fact]
		public async Task RangeSelect() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);

			var exp1 = SqlExpression.GreaterThanOrEqual(SqlExpression.Reference(new ObjectName(tableName, "a")),
				SqlExpression.Constant(SqlObject.Integer(12)));
			var exp2 = SqlExpression.LessThan(SqlExpression.Reference(new ObjectName(tableName, "a")),
				SqlExpression.Constant(SqlObject.Integer(43)));
			var exp = SqlExpression.And(exp1, exp2);

			var rangeNode = new RangeSelectNode(fetchNode, exp);

			var result = await rangeNode.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.Equal(2, result.RowCount);
		}

		[Fact]
		public async Task CacheMarkJoinNode() {
			var tableName1 = new ObjectName("tab1");
			var fetchNode1 = new FetchTableNode(tableName1);
			var tableName2 = new ObjectName("tab2");
			var fetchNode2 = new FetchTableNode(tableName2);
			var joinNode = new JoinNode(fetchNode1,
				fetchNode2,
				ObjectName.Parse("tab1.b"),
				SqlExpressionType.Equal,
				SqlExpression.Reference(ObjectName.Parse("tab2.b1")));

			var markNode = new CacheMarkNode(joinNode, "JOIN");

			var result = await markNode.ReduceAsync(context);

			var cache = context.ResolveService<ITableCache>();
			ITable cachedTable;
			Assert.True(cache.TryGetTable("JOIN", out cachedTable));

			Assert.Equal(result.GetHashCode(), cachedTable.GetHashCode());
		}

		[Fact]
		public async Task DistinctByOneColumn() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);

			var distinctNode = new DistinctNode(fetchNode, new []{new ObjectName(tableName, "a") });

			var result = await distinctNode.ReduceAsync(context);

			Assert.Equal(2, result.RowCount);
		}

		[Fact]
		public async Task LeftOuterJoin() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);
			var distinctNode = new DistinctNode(fetchNode, new[] { new ObjectName(tableName, "a") });

			var table = await fetchNode.ReduceAsync(context);
			var cache = context.ResolveService<ITableCache>();
			cache.SetTable("JOIN", table);

			var leftOuterNode = new LeftOuterJoinNode(distinctNode, "JOIN");

			var result = await leftOuterNode.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.Equal(3, result.RowCount);		// all rows in the original table includes all rows in the computed one
		}

		[Fact]
		public async Task GroupByColumn() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);
			var groupExp = SqlExpression.Function(new ObjectName("count"), new InvokeArgument(SqlExpression.Reference(new ObjectName("*"))));
			var groupNode = new GroupNode(fetchNode, new []{new ObjectName(tableName, "b") }, null, new []{groupExp}, new []{"bGroup"});

			var result = await groupNode.ReduceAsync(context);
			Assert.Equal(1, result.RowCount);
		}

		[Fact]
		public async Task GroupBy() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);
			var groupExp = SqlExpression.Function(new ObjectName("count"), new InvokeArgument(SqlExpression.Reference(new ObjectName("*"))));
			var groupNode = new GroupNode(fetchNode, null, new[] { groupExp }, new[] { "bGroup" });

			var result = await groupNode.ReduceAsync(context);
			Assert.Equal(1, result.RowCount);
		}

		[Fact]
		public async Task GroupMax() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);
			var groupExp = SqlExpression.Function(new ObjectName("count"), new InvokeArgument(SqlExpression.Reference(new ObjectName("*"))));
			var groupMaxColumn = new ObjectName(tableName, "a");
			var groupNode = new GroupNode(fetchNode, new[] { new ObjectName(tableName, "b") }, groupMaxColumn, new[] { groupExp }, new[] { "bGroup" });

			var result = await groupNode.ReduceAsync(context);
			Assert.Equal(1, result.RowCount);
		}


		[Fact]
		public async Task FunctionSelect() {
			var tableName = new ObjectName("tab1");
			var fetchNode = new FetchTableNode(tableName);
			var exp = SqlExpression.Add(SqlExpression.Constant(SqlObject.BigInt(22)),
				SqlExpression.Constant(SqlObject.BigInt(1)));

			var functionNode = new FunctionNode(fetchNode, new []{exp}, new []{"func1"});

			var result = await functionNode.ReduceAsync(context);

			Assert.NotNull(result);
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}