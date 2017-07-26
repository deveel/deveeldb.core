using System;

using Deveel.Data.Serialization;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlArrayTypeTests {
		[Theory]
		[InlineData(67)]
		[InlineData(1024)]
		[InlineData(65740000)]
		public static void Create(int value) {
			var type = PrimitiveTypes.Array(value);

			Assert.Equal(value, type.Length);
		}

		[Theory]
		[InlineData(67)]
		[InlineData(1024)]
		[InlineData(65740000)]
		public static void Serialize(int value) {
			var type = PrimitiveTypes.Array(value);
			var result = BinarySerializeUtil.Serialize(type);

			Assert.Equal(type, result);
		}

		[Theory]
		[InlineData(67)]
		[InlineData(1024)]
		[InlineData(65740000)]
		public static void SerializeExplicit(int value) {
			var type = PrimitiveTypes.Array(value);
			var result = SqlTypeUtil.Serialize(type);

			Assert.Equal(type, result);
		}

		[Theory]
		[InlineData(564, 564, true)]
		[InlineData(1024, 1025, false)]
		public static void TypesEqual(int length1, int length2, bool expected) {
			var type1 = PrimitiveTypes.Array(length1);
			var type2 = PrimitiveTypes.Array(length2);

			Assert.Equal(expected, type1.Equals(type2));
		}
	}
}