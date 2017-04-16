using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;

namespace Deveel.Data.Sql.Tables {
	public class VirtualTableTests {
		private ITable left;

		public VirtualTableTests() {
			var leftInfo = new TableInfo(ObjectName.Parse("tab1"));
			leftInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			leftInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.Boolean()));

			left = new TestTable(leftInfo, new List<SqlObject[]> {
				new [] { SqlObject.Integer(23), SqlObject.Boolean(true) },
				new [] { SqlObject.Integer(54),SqlObject.Boolean(null) }
			});
		}

		[Fact]
		public void NewVirtualTableFromOneTableSource() {
			var table = new VirtualTable(ObjectName.Parse("#table#"), left, new long[]{1});

			Assert.Equal(1, table.RowCount);
			Assert.Equal(2, table.TableInfo.Columns.Count);
		}

		[Fact]
		public async Task GetLastValueOfOneTableSource() {
			var table = new VirtualTable(ObjectName.Parse("#table#"), left, new long[] { 1 });

			var value = await table.GetValueAsync(0, 1);

			Assert.NotNull(value);
			Assert.IsType<SqlBooleanType>(value.Type);
		}
	}
}