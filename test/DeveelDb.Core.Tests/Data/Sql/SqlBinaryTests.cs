using System;

using Deveel.Data.Serialization;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlBinaryTests {
		[Theory]
		[InlineData(564)]
		[InlineData(2)]
		public static void Serialize(int length) {
			var binary = CreateRandom(length);
			var result = BinarySerializeUtil.Serialize(binary);

			Assert.Equal(binary, result);
		}

		private static SqlBinary CreateRandom(int length) {
			var bytes = new byte[length];
			var random = new Random();
			for (int i = 0; i < length; i++) {
				bytes[i] = (byte) random.Next(Byte.MaxValue);
			}

			return new SqlBinary(bytes);
		}
	}
}