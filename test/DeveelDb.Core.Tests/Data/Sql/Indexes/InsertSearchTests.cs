using System;
using System.Collections.Generic;

using Deveel.Data.Sql.Tables;

using Xunit;

namespace Deveel.Data.Sql.Indexes {
	public class InsertSearchTests : IDisposable {
		private ITable left;

		public InsertSearchTests() {
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
		public void SelectAll_SingleColumn_UniqueValues() {
			var index = CreateFullIndex("c");

			var result = index.SelectAll();

			Assert.NotNull(result);
			Assert.NotEmpty(result);

			var list = result.ToBigArray();

			Assert.Equal(3, list.Length);
			Assert.Equal(1, list[0]);
			Assert.Equal(2, list[1]);
			Assert.Equal(0, list[2]);
		}

		[Fact]
		public void SelectAll_SingleColumn() {
			var index = CreateFullIndex("a");

			var result = index.SelectAll();

			Assert.NotNull(result);
			Assert.NotEmpty(result);

			var list = result.ToBigArray();

			Assert.Equal(3, list.Length);
			Assert.Equal(0, list[0]);
			Assert.Equal(2, list[1]);
			Assert.Equal(1, list[2]);
		}

		[Fact]
		public void SelectAll_MultiColumn() {
			var index = CreateFullIndex("a", "c");

			var result = index.SelectAll();

			Assert.NotNull(result);
			Assert.NotEmpty(result);

			var list = result.ToBigArray();

			Assert.Equal(3, list.Length);
		}

		private Index CreateFullIndex(params string[] columnNames) {
			var indexInfo = new IndexInfo(ObjectName.Parse("sys.idx1"), left.TableInfo.TableName, columnNames);
			var index = new InsertSearchIndex(indexInfo);
			index.AttachTo(left);

			foreach (var row in left) {
				index.Insert(row.Id.Number);
			}

			return index;
		}

		public void Dispose() {
			left = null;
		}
	}
}