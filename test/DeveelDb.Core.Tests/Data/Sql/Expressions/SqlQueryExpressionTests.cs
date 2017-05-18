using System;

using Deveel.Data.Configuration;
using Deveel.Data.Services;
using Deveel.Data.Sql.Methods;
using Deveel.Data.Sql.Query.Plan;
using Deveel.Data.Sql.Tables;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public class SqlQueryExpressionTests {
		private IContext context;

		public SqlQueryExpressionTests() {
			var tableManager = new TransientTableManager();

			var tableInfo1 = new TableInfo(ObjectName.Parse("sys.tab1"));
			tableInfo1.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			tableInfo1.Columns.Add(new ColumnInfo("a1", PrimitiveTypes.VarChar(155)));

			tableManager.CreateTableAsync(tableInfo1).Wait();

			var table1 = (TemporaryTable) tableManager.GetTableAsync(tableInfo1.TableName).Result;
			table1.NewRow();
			table1.SetValue(0, 0, SqlObject.Integer(45));
			table1.NewRow();
			table1.SetValue(1, 0, SqlObject.Integer(98));

			table1.BuildIndex();

			var tableInfo2 = new TableInfo(ObjectName.Parse("sys.tab2"));
			tableInfo2.Columns.Add(new ColumnInfo("b", PrimitiveTypes.Integer()));

			tableManager.CreateTableAsync(tableInfo2).Wait();

			var table2 = (TemporaryTable)tableManager.GetTableAsync(tableInfo2.TableName).Result;
			table2.NewRow();
			table2.SetValue(0, 0, SqlObject.Integer(22));
			table2.NewRow();
			table2.SetValue(1, 0, SqlObject.Integer(98));

			table2.BuildIndex();

			var config = new Configuration.Configuration();
			config.SetValue("currentSchema", "sys");

			var container = new ServiceContainer();
			container.AddObjectManager(tableManager);
			container.Register<ITableCache, InMemoryTableCache>();

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(container);
			mock.As<IConfigurationScope>()
				.Setup(x => x.Configuration)
				.Returns(config);

			context = mock.Object;

			container.Register<IMethodResolver, SystemFunctionProvider>();
			container.Register<IQueryPlanner, DefaultQueryPlanner>();
		}

		[Theory]
		[InlineData("a.*", null, "a.*")]
		[InlineData("a", "b", "a AS b")]
		public static void CreateNewTableSource(string tableName, string alias, string expected) {
			var name = ObjectName.Parse(tableName);
			var source = new SqlQueryExpressionSource(name, alias);

			Assert.True(source.IsTable);
			Assert.False(source.IsQuery);
			Assert.Equal(!String.IsNullOrWhiteSpace(alias), source.IsAliased);
			Assert.Equal(expected, source.ToString());
		}

		[Fact]
		public static void CreateNewQuerySource() {
			var fromTable = ObjectName.Parse("table1");
			var query = new SqlQueryExpression();
			query.AllItems = true;
			query.From.Table(fromTable);
			var source = new SqlQueryExpressionSource(query, "a");
			
			Assert.True(source.IsQuery);
			Assert.False(source.IsTable);
			Assert.True(source.IsAliased);
			Assert.Equal("a", source.Alias);
			Assert.True(source.Query.AllItems);

			var expected = new SqlStringBuilder();
			expected.AppendLine("(SELECT *");
			expected.Append("  FROM table1) AS a");

			Assert.Equal(expected.ToString(), source.ToString());
		}

		[Fact]
		public static void CreateNewSimpleQuery() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("a.*")));
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("b")));
			query.From.Table(ObjectName.Parse("tab1"));


			var expected = new SqlStringBuilder();
			expected.AppendLine("SELECT a.*, b");
			expected.Append("  FROM tab1");

			Assert.False(query.From.IsEmpty);
			Assert.Equal(expected.ToString(), query.ToString());
		}

		[Fact]
		public static void MakeNewNaturalJoinQuery() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("a.*")));
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("b.*")));
			query.From.Table(ObjectName.Parse("table1"), "a");
			query.From.Table(ObjectName.Parse("table2"), "b");

			var expected = new SqlStringBuilder();
			expected.AppendLine("SELECT a.*, b.*");
			expected.Append("  FROM table1 AS a, table2 AS b");
			Assert.Equal(expected.ToString(), query.ToString());
		}

		[Fact]
		public static void MakeNewInnerJoinQuery() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("a.*")));
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("b.*")));
			query.From.Table(ObjectName.Parse("table1"), "a");
			query.From.Join(JoinType.Inner,
				SqlExpression.Equal(SqlExpression.Reference(ObjectName.Parse("a.id")),
					SqlExpression.Reference(ObjectName.Parse("b.a_id"))));
			query.From.Table(ObjectName.Parse("table2"), "b");

			var expected = new SqlStringBuilder();
			expected.AppendLine("SELECT a.*, b.*");
			expected.Append("  FROM table1 AS a INNER JOIN table2 AS b ON a.id = b.a_id");

			Assert.Equal(expected.ToString(), query.ToString());
		}

		[Fact]
		public async void ReduceSimpleSelect() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(ObjectName.Parse("sys.tab1"));

			var result = await query.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression) result).Value.Type);

			var table = (ITable) ((SqlConstantExpression) result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(2, table.RowCount);

			var value1 = await table.GetValueAsync(0, 0);
			Assert.Equal(SqlObject.Integer(45), value1);
		}

		[Fact]
		public async void ReduceCountGroupByColumn() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Function(new ObjectName("Count"),
				new InvokeArgument(SqlExpression.Reference(new ObjectName("*")))));
			query.From.Table(ObjectName.Parse("sys.tab1"));
			query.GroupBy.Add(SqlExpression.Reference(new ObjectName("a")));

			var result = await query.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(1, table.RowCount);

			var value1 = await table.GetValueAsync(0, 0);
			Assert.Equal(SqlObject.BigInt(2), value1);
		}

		[Fact]
		public async void ReduceQuantifyArray() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(ObjectName.Parse("tab1"));
			query.Where = SqlExpression.Quantify(SqlExpressionType.Any,
				SqlExpression.Equal(SqlExpression.Reference(new ObjectName("a")),
					SqlExpression.Constant(SqlObject.Array(new[] { SqlObject.Integer(3), SqlObject.Integer(56) }))));

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(0, table.RowCount);
		}

		[Fact]
		public async void ReduceQuantifyInRef() {
			var subQuery = new SqlQueryExpression();
			subQuery.Items.Add(SqlExpression.Reference(new ObjectName("b")));
			subQuery.From.Table(new ObjectName("tab2"));

			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(ObjectName.Parse("tab1"));
			query.Where = SqlExpression.Quantify(SqlExpressionType.Any,
				SqlExpression.Equal(SqlExpression.Reference(new ObjectName("a")), subQuery));

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(1, table.RowCount);
		}


		[Fact]
		public async void ReduceQuantifySubQuery() {
			var subQuery = new SqlQueryExpression();
			subQuery.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			subQuery.From.Table(new ObjectName("tab1"));

			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(ObjectName.Parse("tab1"));
			query.Where = SqlExpression.Quantify(SqlExpressionType.Any,
				SqlExpression.Equal(SqlExpression.Reference(new ObjectName("a")), subQuery));

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(2, table.RowCount);

			var value2 = await table.GetValueAsync(1, 0);
			Assert.Equal(SqlObject.Integer(98), value2);
		}

		[Fact]
		public async void ReduceCorrelatedSubQuery() {
			var subQuery = new SqlQueryExpression();
			subQuery.Items.Add(SqlExpression.Function(new ObjectName("AVG"),
				new InvokeArgument(SqlExpression.Reference(new ObjectName("b")))));
			subQuery.From.Table(new ObjectName("tab2"));
			subQuery.Where = SqlExpression.Equal(SqlExpression.Reference(new ObjectName("a")), SqlExpression.Constant(SqlObject.Integer(45)));

			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(new ObjectName("tab1"));
			query.Where = SqlExpression.GreaterThan(SqlExpression.Reference(new ObjectName("a")), subQuery);

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(1, table.RowCount);
		}


		[Fact]
		public async void ReduceSimpleOr() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(new ObjectName("tab1"));
			query.Where = SqlExpression.Or(
				SqlExpression.Equal(SqlExpression.Reference(new ObjectName("a")), SqlExpression.Constant(SqlObject.Integer(3))),
				SqlExpression.Equal(SqlExpression.Reference(new ObjectName("a")), SqlExpression.Constant(SqlObject.Integer(45))));

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(1, table.RowCount);
		}

		[Fact]
		public async void ReduceSublogic() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(new ObjectName("tab1"));
			query.Where = SqlExpression.And(
				SqlExpression.Equal(SqlExpression.Reference(new ObjectName("a")), SqlExpression.Constant(SqlObject.Integer(3))),
				SqlExpression.And(
					SqlExpression.Equal(SqlExpression.Reference(new ObjectName("a")), SqlExpression.Constant(SqlObject.Integer(45))),
					SqlExpression.IsNot(SqlExpression.Reference(new ObjectName("a")), SqlExpression.Constant(SqlObject.Null))));

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
		}

		[Fact]
		public async void ReduceNaturalJoin() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("t1.a")));
			query.From.Table(new ObjectName("tab1"), "t1");
			query.From.Table(new ObjectName("tab2"));
			query.Where = SqlExpression.Equal(SqlExpression.Reference(ObjectName.Parse("t1.a")),
				SqlExpression.Constant(SqlObject.Integer(98)));

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(2, table.RowCount);
		}

		[Fact]
		public async void ReduceInnerJoin() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("t1.a")));
			query.From.Table(new ObjectName("tab1"), "t1");
			query.From.Table(new ObjectName("tab2"));
			query.From.Join(JoinType.Inner,
				SqlExpression.Equal(SqlExpression.Reference(ObjectName.Parse("t1.a")),
					SqlExpression.Reference(ObjectName.Parse("tab2.b"))));
			query.Where = SqlExpression.Equal(SqlExpression.Reference(ObjectName.Parse("t1.a")),
				SqlExpression.Constant(SqlObject.Integer(98)));

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(1, table.RowCount);
		}


		[Fact]
		public async void ReduceConstant() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Constant(SqlObject.Boolean(true)));

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(1, table.RowCount);

			var value = table.GetValue(0, 0);
			Assert.Equal(SqlObject.Boolean(true), value);
		}

		[Fact]
		public async void ReduceSubQuery() {
			var subQuery = new SqlQueryExpression();
			subQuery.All = true;
			subQuery.From.Table(new ObjectName("tab1"));

			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Query(subQuery);

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(2, table.RowCount);
		}

		[Fact]
		public async void ReduceLike() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(ObjectName.Parse("sys.tab1"));
			query.Where = SqlExpression.Like(SqlExpression.Reference(new ObjectName("a1")),
				SqlExpression.Constant(SqlObject.String(new SqlString("2%"))));

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
		}

		[Fact]
		public async void ReduceComplexLike() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(ObjectName.Parse("sys.tab1"));
			query.Where = SqlExpression.Like(
				SqlExpression.Add(SqlExpression.Reference(new ObjectName("a1")),
					SqlExpression.Cast(SqlExpression.Reference(new ObjectName("a")), PrimitiveTypes.String())),
				SqlExpression.Constant(SqlObject.String(new SqlString("2%"))));

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
		}

		[Fact]
		public async void ReduceComposite() {
			var query2 = new SqlQueryExpression();
			query2.Items.Add(SqlExpression.Reference(new ObjectName("b")));
			query2.From.Table(ObjectName.Parse("sys.tab2"));

			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(ObjectName.Parse("sys.tab1"));
			query.NextComposite = new SqlQueryExpressionComposite(CompositeFunction.Union, true, query2);

			var result = await query.ReduceAsync(context);
			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
			Assert.IsType<SqlTableType>(((SqlConstantExpression)result).Value.Type);

			var table = (ITable)((SqlConstantExpression)result).Value.Value;

			Assert.NotNull(table);
			Assert.Equal(2, table.RowCount);

			var value1 = await table.GetValueAsync(0, 0);
			Assert.Equal(SqlObject.Integer(22), value1);

			var rows = table.SelectAllRows().ToBigArray();
			Assert.Equal(2, rows.Length);
		}
	}
}