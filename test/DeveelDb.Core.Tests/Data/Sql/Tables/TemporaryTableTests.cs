using System;
using System.Collections.Generic;

using Xunit;

namespace Deveel.Data.Sql.Tables {
	public static class TemporaryTableTests {
		[Fact]
		public static void CreateNew() {
			var columns = new List<ColumnInfo> {
				new ColumnInfo("a", PrimitiveTypes.Integer()),
				new ColumnInfo("b", PrimitiveTypes.Char(22))
			};

			var table = new TemporaryTable(new ObjectName("tab1"), columns);

			Assert.NotNull(table.TableInfo);
			Assert.Equal(2, table.TableInfo.Columns.Count);
		}

		[Fact]
		public static void AddRows() {
			var columns = new List<ColumnInfo> {
				new ColumnInfo("a", PrimitiveTypes.Integer()),
				new ColumnInfo("b", PrimitiveTypes.Char(2))
			};

			var table = new TemporaryTable(new ObjectName("tab1"), columns);

			table.AddRow(new[] {SqlObject.Integer(31), SqlObject.Char(new SqlString("OK"))});

			Assert.Equal(1, table.RowCount);
		}

		[Fact]
		public static void AddRows2() {
			var columns = new List<ColumnInfo> {
				new ColumnInfo("a", PrimitiveTypes.Integer()),
				new ColumnInfo("b", PrimitiveTypes.Char(2))
			};

			var table = new TemporaryTable(new ObjectName("tab1"), columns);
			var row = table.NewRow();
			table.SetValue(row, 0, SqlObject.Integer(43));
			table.SetValue(row, 1, SqlObject.Char(new SqlString("KO")));

			Assert.Equal(1, table.RowCount);

			var value1 = table.GetValue(row, 0);
			var value2 = table.GetValue(row, 1);

			Assert.Equal(SqlObject.Integer(43), value1);
			Assert.Equal(SqlObject.Char(new SqlString("KO")), value2);
		}
	}
}