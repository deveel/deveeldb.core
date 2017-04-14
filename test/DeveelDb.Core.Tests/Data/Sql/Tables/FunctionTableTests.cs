using System;
using System.Collections.Generic;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Tables {
	public class FunctionTableTests {
		private IContext context;
		private ITable left;

		public FunctionTableTests() {
			var leftInfo = new TableInfo(ObjectName.Parse("tab1"));
			leftInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			leftInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.Boolean()));

			left = new TestTable(leftInfo, new List<SqlObject[]> {
				new [] { SqlObject.Integer(23), SqlObject.Boolean(true) },
				new [] { SqlObject.Integer(54),SqlObject.Boolean(null) }
			});

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());

			context = mock.Object;
		}

		[Fact]
		public void CreateNewFunctionTable() {
			var exp = SqlExpression.Equal(SqlExpression.Reference(ObjectName.Parse("tab1.a")), 
				SqlExpression.Constant(SqlObject.Integer(2)));

			var cols = new[] {
				new FunctionColumnInfo(exp, "exp1", PrimitiveTypes.Integer()) 
			};

			var table = new FunctionTable(context, left, cols);

			Assert.NotNull(table.TableInfo);
			Assert.Equal(2, table.RowCount);

			var value = table.GetValue(0, 0);

			Assert.NotNull(value);
			Assert.True(value.IsFalse);
		}

		[Fact]
		public void GroupMax() {
			var exp = SqlExpression.Equal(SqlExpression.Reference(ObjectName.Parse("tab1.a")),
				SqlExpression.Constant(SqlObject.Integer(2)));

			var cols = new[] {
				new FunctionColumnInfo(exp, "exp1", PrimitiveTypes.Integer())
			};

			var table = new FunctionTable(context, left, cols);

			var result = table.GroupMax(new ObjectName("b"));

			Assert.NotNull(result);

			var value1 = result.GetValue(0, 0);

			Assert.NotNull(value1);
			Assert.Equal(SqlObject.Integer(23), value1);
		}

		[Fact]
		public void GroupBy() {
			var exp = SqlExpression.Equal(SqlExpression.Reference(ObjectName.Parse("tab1.a")),
				SqlExpression.Constant(SqlObject.Integer(2)));

			var cols = new[] {
				new FunctionColumnInfo(exp, "exp1", PrimitiveTypes.Integer())
			};

			var table = new GroupTable(context, left, cols, new []{ObjectName.Parse("tab1.a") });
			var value1 = table.GetValue(0, 0);

			Assert.NotNull(value1);
		}
	}
}