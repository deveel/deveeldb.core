using System;
using System.Collections.Generic;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Methods;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Tables {
	public class FunctionTableTests {
		private IContext context;
		private TemporaryTable left;

		public FunctionTableTests() {
			var leftInfo = new TableInfo(ObjectName.Parse("tab1"));
			leftInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			leftInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.Boolean()));
			left = new TemporaryTable(leftInfo);
			
			left.AddRow(new[] { SqlObject.Integer(23), SqlObject.Boolean(true) });
			left.AddRow(new[] { SqlObject.Integer(54), SqlObject.Boolean(null) });

			left.BuildIndex();

			var funcInfo = new SqlFunctionInfo(new ObjectName("count"), PrimitiveTypes.Integer());
			funcInfo.Parameters.Add(new SqlMethodParameterInfo("x", PrimitiveTypes.Table()));

			var aggResolver = new Mock<IMethodResolver>();
			aggResolver.Setup(x => x.ResolveMethod(It.IsAny<IContext>(), It.IsAny<Invoke>()))
				.Returns(new SqlAggregateFunctionDelegate(funcInfo, iterate => {
				if (iterate.IsFirst) {
					return iterate.Current;
				} else {
					return iterate.Accumulation.Add(iterate.Current);
				}
			}));

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());

			context = mock.Object;

			context.RegisterInstance<IMethodResolver>(aggResolver.Object);
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
		public void GroupByCount() {
			var exp = SqlExpression.Function(new ObjectName("count"),
				new InvokeArgument(SqlExpression.Reference(ObjectName.Parse("tab1.a"))));
			
			var cols = new[] {
				new FunctionColumnInfo(exp, "exp1", PrimitiveTypes.Integer())
			};

			var table = new GroupTable(context, left, cols, new []{ObjectName.Parse("tab1.a") });
			var value1 = table.GetValue(0, 0);

			Assert.NotNull(value1);
			Assert.Equal(SqlObject.Integer(77), value1);
		}

		[Fact]
		public void GroupMaxOverGroupBy() {
			var exp = SqlExpression.Function(new ObjectName("count"),
				new InvokeArgument(SqlExpression.Reference(ObjectName.Parse("tab1.a"))));

			var cols = new[] {
				new FunctionColumnInfo(exp, "exp1", PrimitiveTypes.Integer())
			};

			var table = new GroupTable(context, left, cols, new[] { ObjectName.Parse("tab1.a") });
			var groupMax = table.GroupMax(ObjectName.Parse("tab1.a"));

			Assert.NotNull(groupMax);
			Assert.Equal(1, groupMax.RowCount);

			var value = groupMax.GetValue(0, 0);

			Assert.NotNull(value);
			Assert.False(value.IsFalse);

			Assert.Equal(SqlObject.Integer(54), value);
		}

		[Fact]
		public void MakeFullGroupTable() {
			var exp = SqlExpression.Function(new ObjectName("count"),
				new InvokeArgument(SqlExpression.Reference(ObjectName.Parse("tab1.a"))));

			var cols = new[] {
				new FunctionColumnInfo(exp, "exp1", PrimitiveTypes.Integer())
			};

			var table = new GroupTable(context, left, cols, new ObjectName[0]);

			Assert.Equal(2, table.RowCount);
		}
	}
}