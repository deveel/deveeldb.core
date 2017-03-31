using System;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlTypeTests {
		[Theory]
		[InlineData(typeof(bool), SqlTypeCode.Boolean)]
		[InlineData(typeof(byte), SqlTypeCode.TinyInt)]
		[InlineData(typeof(short), SqlTypeCode.SmallInt)]
		[InlineData(typeof(int), SqlTypeCode.Integer)]
		[InlineData(typeof(long), SqlTypeCode.BigInt)]
		[InlineData(typeof(string), SqlTypeCode.String)]
		[InlineData(typeof(DateTime), SqlTypeCode.TimeStamp)]
		[InlineData(typeof(DateTimeOffset), SqlTypeCode.TimeStamp)]
		[InlineData(null, SqlTypeCode.Unknown)]
		public static void GetTypeCode(Type type, SqlTypeCode expected) {
			Assert.Equal(expected, SqlType.GetTypeCode(type));
		}

		[Theory]
		[InlineData(typeof(object))]
		[InlineData(typeof(IntPtr))]
		public static void GetNotSupportedTypeCode(Type type) {
			Assert.Throws<NotSupportedException>(() => SqlType.GetTypeCode(type));
		}
	}
}