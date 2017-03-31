using System;

using Xunit;

namespace Deveel.Data.Sql {
	public class PrimitiveTypesTests {
		[Theory]
		[InlineData("varchar", true)]
		[InlineData("int", true)]
		[InlineData("day to second", true)]
		[InlineData("object", false)]
		[InlineData("TINYINT", true)]
		[InlineData("USERObject", false)]
		[InlineData("DATE", true)]
		[InlineData("YEAR TO MONTH", true)]
		public static void IsPrimitive(string name, bool expected) {
			Assert.Equal(expected, PrimitiveTypes.IsPrimitive(name));
		}

		[Theory]
		[InlineData(SqlTypeCode.Type, false)]
		[InlineData(SqlTypeCode.Boolean, true)]
		[InlineData(SqlTypeCode.Blob, true)]
		[InlineData(SqlTypeCode.BigInt, true)]
		public static void IsPrimitive(SqlTypeCode typeCode, bool expected) {
			Assert.Equal(expected, PrimitiveTypes.IsPrimitive(typeCode));
		}

		[Theory]
		[InlineData(SqlTypeCode.Bit)]
		[InlineData(SqlTypeCode.Boolean)]
		public static void GetBoolean(SqlTypeCode typeCode) {
			var type = PrimitiveTypes.Boolean(typeCode);

			Assert.IsType<SqlBooleanType>(type);
			Assert.Equal(typeCode, type.TypeCode);
			Assert.False(type.IsLargeObject);
			Assert.False(type.IsNull);
			Assert.False(type.IsReference);
			Assert.True(type.IsPrimitive);
			Assert.True(type.IsIndexable);
		}

		[Fact]
		public static void GetInvalidBoolean() {
			Assert.Throws<ArgumentException>(() => PrimitiveTypes.Boolean(SqlTypeCode.Blob));
		}
	}
}