﻿using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Deveel.Data.Sql.Tables {
	public class LimitedTableTests {
		private ITable left;

		public LimitedTableTests() {
			var leftInfo = new TableInfo(ObjectName.Parse("tab1"));
			leftInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			leftInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.Boolean()));
			leftInfo.Columns.Add(new ColumnInfo("c", PrimitiveTypes.Double()));

			left = new TestTable(leftInfo, new List<SqlObject[]> {
				new[] {SqlObject.Integer(23), SqlObject.Boolean(true), SqlObject.Double(5563.22)},
				new[] {SqlObject.Integer(54), SqlObject.Boolean(null), SqlObject.Double(921.001)},
				new[] {SqlObject.Integer(23), SqlObject.Boolean(true), SqlObject.Double(2010.221)}
			});
		}

		[Fact]
		public void LimitWithOffsetAndTotal() {
			var limited = new LimitedTable(left, 1, 1);

			Assert.Equal(1, limited.RowCount);
			Assert.Equal(3, limited.TableInfo.Columns.Count);

			var value1 = limited.GetValue(0, 0);
			var value2 = limited.GetValue(0, 1);

			Assert.Equal(SqlObject.Integer(54), value1);
			Assert.Equal(SqlObject.Boolean(null), value2);
		}

		[Fact]
		public void LimitWithTotal() {
			var limited = new LimitedTable(left, -1, 2);

			Assert.Equal(2, limited.RowCount);
			Assert.Equal(3, limited.TableInfo.Columns.Count);

			var value1 = limited.GetValue(0, 0);
			var value2 = limited.GetValue(0, 1);

			Assert.Equal(SqlObject.Integer(23), value1);
			Assert.Equal(SqlObject.Boolean(true), value2);
		}

		[Fact]
		public void EnumerateLimited() {
			var limited = new LimitedTable(left, -1, 2);

			Assert.Equal(2, limited.RowCount);
			Assert.Equal(3, limited.TableInfo.Columns.Count);

			var row1 = limited.First();

			var value1 = row1.GetValue("a");

			Assert.Equal(SqlObject.Integer(23), value1);
		}
	}
}