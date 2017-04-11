using System;
using System.Collections.Generic;

using Xunit;

namespace Deveel.Data.Sql.Tables {
	public class CrossTableTests {
		private ITable left;
		private ITable right;

		public CrossTableTests() {
			var leftInfo = new TableInfo(ObjectName.Parse("tab1"));
			leftInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			leftInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.Boolean()));

			left = new TestTable(leftInfo, new List<SqlObject[]> {
				new [] { SqlObject.Integer(23), SqlObject.Boolean(true) },
				new [] { SqlObject.Integer(54),SqlObject.Boolean(null) }
			});
			
			var rightInfo = new TableInfo(ObjectName.Parse("tab2"));
			rightInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			rightInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.Boolean()));

			right = new TestTable(rightInfo, new List<SqlObject[]> {
				new [] { SqlObject.Integer(15), SqlObject.Boolean(true) },
				new [] { SqlObject.Integer(544),SqlObject.Boolean(false) }
			});
		}

		[Fact]
		public void CreateJoinedTable() {
			var table = new CrossTable(left, right);

			Assert.Equal(4, table.RowCount);
			Assert.Equal(4, table.TableInfo.Columns.Count);
		}

		[Fact]
		public void GetValueFromJoined() {
			var table = new CrossTable(left, right);

			Assert.Equal(4, table.RowCount);

			var value1 = table.GetValue(0, 0);
			var value2 = table.GetValue(1, 0);
			var value3 = table.GetValue(2, 0);
			var value4 = table.GetValue(3, 0);

			Assert.NotNull(value1);
			Assert.NotNull(value2);
			Assert.NotNull(value3);
			Assert.NotNull(value4);

			Assert.Equal(value1, value2);
			Assert.Equal(value3, value4);
			Assert.NotEqual(value1, value3);
			Assert.NotEqual(value2, value4);
		}

		[Fact]
		public void EnumerateRows() {
			var table = new CrossTable(left, right);
			var row1 = table.ElementAt(0);
			var row2 = table.ElementAt(1);
		}
	}
}