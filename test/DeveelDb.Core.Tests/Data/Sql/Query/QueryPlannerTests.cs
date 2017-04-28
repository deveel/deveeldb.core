using System;

using Deveel.Data.Configuration;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Methods;
using Deveel.Data.Sql.Query.Plan;
using Deveel.Data.Sql.Tables;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Query {
	public class QueryPlannerTests : IDisposable {
		private IContext context;

		public QueryPlannerTests() {
			var tableManager = new TransientTableManager();

			var tableInfo1 = new TableInfo(ObjectName.Parse("sys.tab1"));
			tableInfo1.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			tableInfo1.Columns.Add(new ColumnInfo("a1", PrimitiveTypes.VarChar(155)));

			tableManager.CreateTableAsync(tableInfo1).Wait();

			var table1 = (TemporaryTable)tableManager.GetTableAsync(tableInfo1.TableName).Result;
			table1.NewRow();
			table1.SetValue(0, 0, SqlObject.Integer(45));
			table1.NewRow();
			table1.SetValue(1, 0, SqlObject.Integer(98));

			table1.BuildIndex();

			var config = new Configuration.Configuration();
			config.SetValue("currentSchema", "sys");

			var container = new ServiceContainer();
			container.RegisterInstance<IDbObjectManager>(tableManager);
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

		[Fact]
		public async void SortResult() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(ObjectName.Parse("sys.tab1"));

			var planner = context.ResolveService<IQueryPlanner>();
			var node = await planner.PlanAsync(context,
				new QueryInfo(query, new[] {new SortColumn(SqlExpression.Reference(new ObjectName("a")), false)}));

			var result = await node.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.Equal(2, result.RowCount);

			var value1 = await result.GetValueAsync(0, 0);
			Assert.Equal(SqlObject.Integer(98), value1);
		}

		[Fact]
		public async void LimitResult() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			query.From.Table(ObjectName.Parse("sys.tab1"));

			var planner = context.ResolveService<IQueryPlanner>();
			var node = await planner.PlanAsync(context, new QueryInfo(query, new QueryLimit(1)));

			var result = await node.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.Equal(1, result.RowCount);
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}