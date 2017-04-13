using System;

using Xunit;

namespace Deveel.Data.Sql.Indexes {
	public static class IndexInfoTests {
		[Fact]
		public static void CreateNewForOneColumn() {
			var indexInfo = new IndexInfo(ObjectName.Parse("sys.table1_idx"), ObjectName.Parse("sys.table1"), new[] {"col1"});

			Assert.NotNull(indexInfo.IndexName);
			Assert.NotNull(indexInfo.TableName);
			Assert.NotNull(indexInfo.ColumnNames);
			Assert.Equal(1, indexInfo.ColumnNames.Length);
		}
	}
}