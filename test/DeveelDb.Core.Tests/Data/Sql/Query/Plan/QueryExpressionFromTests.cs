using System;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Query.Plan {
	public class QueryExpressionFromTests {
		private IContext context;

		public QueryExpressionFromTests() {
			var tableManager = new TransientTableManager();
			tableManager.CreateTableAsync(new TableInfo(ObjectName.Parse("sys.tab1"))).Wait();

			var config = new Configuration.Configuration();
			config.SetValue("currentSchema", "sys");

			var container = new ServiceContainer();
			container.RegisterInstance<IDbObjectManager>(tableManager, DbObjectType.Table);

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(container);
			mock.As<IConfigurationScope>()
				.Setup(x => x.Configuration)
				.Returns(config);

			context = mock.Object;
		}

		[Fact]
		public async Task CreateFromSimpleQuery() {
			var query = new SqlQueryExpression();
			query.Items.Add(new SqlReferenceExpression(new ObjectName("a")));
			query.From.Table(new ObjectName("tab1"));

			var queryFrom = await QueryExpressionFrom.CreateAsync(context, query);

			Assert.NotNull(queryFrom);
			Assert.Null(queryFrom.Parent);
			Assert.Equal(1, queryFrom.SourceCount);
		}
	}
}