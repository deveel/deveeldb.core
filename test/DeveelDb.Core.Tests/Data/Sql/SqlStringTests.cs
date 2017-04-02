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

		[Theory]
		[InlineData("foo bar", 4, 'b')]
		public static void GetCharAtIndex(string source, int offset, char expected) {
			var s = new SqlString(source);
			var c = s[offset];

			Assert.Equal(expected, c);
		}

		[Fact]
		public static void GetCharOutOfBonds() {
			var s = new SqlString("foo bar");
			Assert.Throws<ArgumentOutOfRangeException>(() => s[100]);
		}

		[Theory]
		[InlineData("true", typeof(bool), true)]
		[InlineData("false", typeof(bool), false)]
		[InlineData("4556.931", typeof(double), 4556.931)]
		[InlineData("82211993", typeof(long), 82211993L)]
		[InlineData("3", typeof(byte), (byte)3)]
		[InlineData("5466", typeof(short), (short) 5466)]
		[InlineData("quick brown fox", typeof(string), "quick brown fox")]
		public static void Convert_ChangeType(string source, Type type, object expected) {
			var s = new SqlString(source);
			var result = Convert.ChangeType(s, type);

			Assert.NotNull(result);
			Assert.IsType(type, result);
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("457738.99931e32", 457738.99931e32)]
		public static void Convert_ToSqlNumber(string source, double expected) {
			var s = new SqlString(source);
			var result = Convert.ChangeType(s, typeof(SqlNumber));

			Assert.NotNull(result);
			Assert.IsType<SqlNumber>(result);
			Assert.Equal(expected, (double)(SqlNumber)result);
		}

		[Fact]
		public static void Convert_ToSqlBinary() {
			var s = new SqlString("the quick brown fox");
			var result = Convert.ChangeType(s, typeof(SqlBinary));

			Assert.NotNull(result);
			Assert.IsType<SqlBinary>(result);
			Assert.Equal(s.Length * 2, ((SqlBinary)result).Length);
		}

		[Theory]
		[InlineData("1", true)]
		[InlineData("true", true)]
		[InlineData("TRUE", true)]
		[InlineData("false", false)]
		public static void Convert_ToSqlBoolean(string source, bool expected) {
			var s = new SqlString(source);
			var result = Convert.ChangeType(s, typeof(SqlBoolean));

			Assert.NotNull(result);
			Assert.IsType<SqlBoolean>(result);
			Assert.Equal(expected, (bool)(SqlBoolean)result);
		}

		[Theory]
		[InlineData("the quick brown fox")]
		public static void Convert_ToCharArray(string source) {
			var s = new SqlString(source);
			var result = Convert.ChangeType(s, typeof(char[]));

			Assert.NotNull(result);
			Assert.IsType<char[]>(result);
			Assert.Equal(s.Length, ((char[])result).Length);
		}

		[Theory]
		[InlineData("the quick brown fox")]
		public static void GetCharArray(string source) {
			var s = new SqlString(source);
			var result = s.ToCharArray();

			Assert.Equal(s.Length, result.Length);
		}
	}
}