using System;
using System.Globalization;

using Deveel.Data.Text;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlStringSearchTests {
		[Theory]
		[InlineData("the quick brown fox", "the quick%", true)]
		[InlineData("the quick brown fox", "%fox", true)]
		[InlineData("the quick brown fox", "ant%", false)]
		[InlineData("antonello", "an__nello", true)]
		[InlineData("antonello",  "an_nello", false)]
		[InlineData("the quick\\brown fox", "the quick%", true)]
		public static void Search(string source, string pattern, bool matches) {
			var s = SqlValueUtil.FromObject(source);

			Assert.IsType<SqlString>(s);

			var stringSearch = new SqlDefaultStringSearch();
			Assert.Equal(matches, stringSearch.Matches((SqlString)s, pattern, PatternSearch.EscapeCharacter));
		}
	}
}