using System;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlStringTests {
		[Theory]
		[InlineData("the quick ", "brown fox", "the quick brown fox")]
		public static void ConcatSimplestrings(string s1, string s2, string expected) {
			var sqlString1 = new SqlString(s1);
			var sqlString2 = new SqlString(s2);

			Assert.Equal(expected, sqlString1.Concat(sqlString2));
		}

		[Theory]
		[InlineData("foo bar", 12, "foo bar     ")]
		[InlineData("the quick brow fox", 5, "the quick brow fox")]
		public static void PadRight(string source, int length, string expected) {
			var s = new SqlString(source);
			var pad = s.PadRight(length);

			Assert.Equal(expected, pad);
		}
	}
}