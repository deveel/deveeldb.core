using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;

namespace Deveel.Data.Sql.Tables {
	public class SubsetTableTests {
		private ITable left;

		public SubsetTableTests() {
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
		public async Task MapTwoColumns() {
			var subset = new SubsetTable(left, new[] {1, 2},
				new ObjectName[] {ObjectName.Parse("tab1.b1"), ObjectName.Parse("tab1.c1"),});

			Assert.Equal(3, subset.RowCount);
			Assert.Equal(2, subset.TableInfo.Columns.Count);

			Assert.Equal(0, subset.TableInfo.Columns.IndexOf(ObjectName.Parse("tab1.b1")));
			Assert.Equal(1, subset.TableInfo.Columns.IndexOf(ObjectName.Parse("tab1.c1")));
			Assert.Equal(0, subset.TableInfo.Columns.IndexOf("b1"));
			Assert.Equal(1, subset.TableInfo.Columns.IndexOf("c1"));

			var value1 = await subset.GetValueAsync(0, 1);
			var value2 = await subset.GetValueAsync(2, 0);

			Assert.Equal(SqlObject.Double(5563.22), value1);
			Assert.Equal(SqlObject.Boolean(true), value2);
		}
	}
}