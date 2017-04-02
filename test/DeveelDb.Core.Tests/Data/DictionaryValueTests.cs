using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Deveel.Data {
	public static class DictionaryValueTests {
		[Fact]
		public static void GetNullKey() {
			var dictionary = new Dictionary<string, object> {
				{ "key1", "Hello!" }
			};

			Assert.Throws<ArgumentNullException>(() => dictionary.GetValue<string>(null));
		}

		[Fact]
		public static void GetFromNullDictionary() {
			IDictionary<string, object> dictionary = null;

			var value = dictionary.GetValue<string>("key");
			Assert.Null(value);
		}

		[Fact]
		public static void GetConvertibleValue() {
			var dictionary = new Dictionary<string, object> {
				{ "key1", "Hello!" },
				{ "key2", 456 }
			};

			var value = dictionary.GetValue<double>("key2");
			Assert.Equal((double)456, value);
		}

		[Fact]
		public static void GetNotFoundValue() {
			var dictionary = new Dictionary<string, object> {
				{ "key1", "Hello!" },
				{ "key2", 456 }
			};

			var value = dictionary.GetValue<double?>("key3");
			Assert.Null(value);
		}

		[Fact]
		public static void GetFromEnumerable() {
			var dictionary = new Dictionary<string, object> {
				{ "key1", "Hello!" },
				{ "key2", 456 }
			}.AsEnumerable();

			var value = dictionary.GetValue<string>("key1");
			Assert.NotNull(value);
			Assert.Equal("Hello!", value);
		}

		[Fact]
		public static void GetNullableConvert() {
			var dictionary = new Dictionary<string, object> {
				{ "key1", "Hello!" },
				{ "key2", 456 }
			};

			var value = dictionary.GetValue<double?>("key2");
			Assert.NotNull(value);
			Assert.Equal(456, value);
		}
	}
}