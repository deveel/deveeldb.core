using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Constraints {
	public class ConstraintTests : IDisposable {
		private IContext context;
		private TemporaryTable table;
		private TemporaryTable foreignTable;

		public ConstraintTests() {
			var container = new ServiceContainer();

			var tableName = ObjectName.Parse("sys.tab1");
			var tableInfo = new TableInfo(tableName);
			tableInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			tableInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.VarChar(33)));
			tableInfo.Columns.Add(new ColumnInfo("c", PrimitiveTypes.Boolean()) {
				DefaultValue = SqlExpression.Constant(SqlObject.Boolean(true))
			});

			table = new TemporaryTable(tableInfo);

			var resolver = new Mock<ITableConstraintResolver>();
			resolver.Setup(x => x.ResolveConstraints(It.Is<ObjectName>(y => y.Equals(tableName))))
				.Returns(NotNulls(tableName, "a", "b"));

			container.RegisterInstance<ITableConstraintResolver>(resolver.Object);

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(container);

			context = mock.Object;
		}

		private static IEnumerable<NotNullConstraint> NotNulls(ObjectName tableName, params string[] coluumnsNames) {
			return new List<NotNullConstraint> {
				new NotNullConstraint(new NotNullConstraintInfo("a_notNull", tableName, coluumnsNames))
			};
		}

		[Fact]
		public void QueryNotNulls() {
			var constraints = context.QueryTableConstraints(ObjectName.Parse("sys.tab1"))
				.OfType<NotNullConstraint>();

			Assert.NotNull(constraints);
			Assert.NotEmpty(constraints);
		}

		[Fact]
		public void CheckNotNullViolation_Fail() {
			var row = table.AddRow(new [] {
				SqlObject.Integer(2),
				SqlObject.NullOf(PrimitiveTypes.Integer()),
				SqlObject.Boolean(false), 
			});

			Assert.ThrowsAny<ConstraintViolationException>(() => context.CheckNotNullViolation(table, new long[]{row}));
		}

		[Fact]
		public void CheckNotNullViolation() {
			var row = table.AddRow(new[] {
				SqlObject.Integer(2),
				SqlObject.Integer(4),
				SqlObject.Boolean(false),
			});

			context.CheckNotNullViolation(table, new long[] { row });
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}