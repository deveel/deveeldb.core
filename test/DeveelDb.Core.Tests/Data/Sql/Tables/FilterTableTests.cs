using System;
using System.Linq;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Indexes;

using Xunit;

namespace Deveel.Data.Sql.Tables {
	public class FilterTableTests : IDisposable {
		private TemporaryTable left;

		public FilterTableTests() {
			var leftInfo = new TableInfo(ObjectName.Parse("tab1"));
			leftInfo.Columns.Add(new ColumnInfo("a", PrimitiveTypes.Integer()));
			leftInfo.Columns.Add(new ColumnInfo("b", PrimitiveTypes.Boolean()));
			left = new TemporaryTable(leftInfo);

			left.AddRow(new[] { SqlObject.Integer(23), SqlObject.Boolean(true) });
			left.AddRow(new[] { SqlObject.Integer(54), SqlObject.Boolean(null) });

			left.BuildIndex();
		}

		[Fact]
		public void GetSubsetIndex() {
			var table = new FilterTable(left);
			var index = table.GetColumnIndex(0);

			Assert.NotNull(index);

			var rows = index.SelectGreater(new IndexKey(SqlObject.Integer(24)));

			Assert.Equal(1, rows.Count());
		}

		public void Dispose() {
			
		}
	}
}