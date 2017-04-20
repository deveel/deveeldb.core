using System;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Query.Plan {
	public class TablePlanTests {
		[Fact]
		public void FromUnaryExpression() {
			var queryInfo = new Mock<ITableQueryInfo>();
			queryInfo.SetupGet(x => x.TableInfo)
				.Returns(() => {
					var tableInfo = new TableInfo(ObjectName.Parse("sys.tab1"));
					return tableInfo;
				});

			var expression = SqlExpression.Negate(SqlExpression.Constant(SqlObject.Boolean(true)));

			var fromTable = new FromTableDirect(true, queryInfo.Object, "sys_tab1_rnd2", null, ObjectName.Parse("sys.tab1"));

			var planSet = new TableSetPlan();
			planSet.AddFromTable(new FetchTableNode(new ObjectName("tab1")), fromTable);
			var node = planSet.PlanSearchExpression(expression);

			Assert.NotNull(node);
			Assert.IsType<ConstantSelectNode>(node);
		}

		[Fact]
		public void FromArrayQuantify() {
			var queryInfo = new Mock<ITableQueryInfo>();
			queryInfo.SetupGet(x => x.TableInfo)
				.Returns(() => {
					var tableInfo = new TableInfo(ObjectName.Parse("sys.tab1"));
					return tableInfo;
				});

			var array = SqlExpression.Constant(SqlObject.Array(SqlObject.Integer(3), SqlObject.Integer(5)));
			var expression = SqlExpression.All(SqlExpression.Equal(SqlExpression.Reference(ObjectName.Parse("sys.tab1.a")), array));

			var fromTable = new FromTableDirect(true, queryInfo.Object, "sys_tab1_rnd2", null, ObjectName.Parse("sys.tab1"));

			var planSet = new TableSetPlan();
			planSet.AddFromTable(new FetchTableNode(new ObjectName("tab1")), fromTable);
			var node = planSet.PlanSearchExpression(expression);

			Assert.NotNull(node);
			Assert.IsType<QuantifiedSelectNode>(node);
		}
	}
}