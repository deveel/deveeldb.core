using System;
using System.Linq;

using Xunit;

namespace Deveel.Data.Configuration {
	public class ConfigurationTests {
		[Fact]
		public void DefaultConfig() {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);
			Assert.Null(config.Parent);
		}

		[Fact]
		public void GetValuesFromRoot() {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);
			config.SetValue("test.oneKey", 54);
			config.SetValue("test.twoKeys", null);

			var value1 = config.GetValue("test.oneKey");
			Assert.NotNull(value1);
			Assert.IsType<int>(value1);
			Assert.Equal(54, value1);

			var value2 = config.GetValue("test.twoKeys");
			Assert.Null(value2);
		}

		[Fact]
		public void GetValuesFromChild() {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);

			config.SetValue("test.oneKey", "one");

			IConfiguration child = new Configuration(config);
			Assert.NotNull(child);
			Assert.NotNull(child.Parent);

			child.SetValue("test.oneKey", 45);

			var value = child.GetValue("test.oneKey");
			Assert.NotNull(value);
			Assert.IsType<int>(value);
			Assert.Equal(45, value);

			value = config.GetValue("test.oneKey");
			Assert.NotNull(value);
			Assert.IsType<string>(value);
			Assert.Equal("one", value);
		}

		[Fact]
		public void GetValueAsInt32() {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);

			config.SetValue("test.oneKey", "22");

			object value = null;
			value = config.GetInt32("test.oneKey");
			Assert.NotNull(value);
			Assert.IsType<int>(value);
			Assert.Equal(22, value);
		}

		[Theory]
		[InlineData("test", "true", true)]
		[InlineData("test", "false", false)]
		[InlineData("test", "off", false)]
		[InlineData("test", "on", true)]
		[InlineData("test", "enabled", true)]
		public void GetBooleanValue(string key, string value, bool expected) {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);

			config.SetValue(key, value);

			object configValue = config.GetBoolean(key);
			Assert.NotNull(configValue);
			Assert.IsType<bool>(configValue);
			Assert.Equal(expected, configValue);
		}

		[Theory]
		[InlineData(1, TestEnum.One)]
		[InlineData("one", TestEnum.One)]
		[InlineData("TWO", TestEnum.Two)]
		[InlineData(null, TestEnum.Default)]
		public void GetEnumValue(object value, TestEnum expected) {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);

			config.SetValue("test", value);

			object configValue = config.GetValue<TestEnum>("test");
			Assert.IsType<TestEnum>(configValue);
			Assert.Equal(expected, configValue);
		}

		public enum TestEnum {
			One = 1,
			Two = 2,
			Default = 0
		}

		[Fact]
		public void GetAllKeysFromRoot() {
			var config = new Configuration();
			config.SetValue("a", 22);
			config.SetValue("b", new DateTime(2001, 02, 01));

			var keys = config.GetKeys();

			Assert.NotNull(keys);
			Assert.NotEmpty(keys);
			Assert.Contains("a", keys);
		}

		[Fact]
		public void GetAllKeysFromTree() {
			var config = new Configuration();
			config.SetValue("a", 22);
			config.SetValue("b", new DateTime(2001, 02, 01));

			var child = new Configuration(config);
			child.SetValue("a", 56);
			config.SetValue("c", "test");

			var keys = child.GetKeys();

			Assert.NotNull(keys);
			Assert.NotEmpty(keys);
			Assert.Contains("a", keys);
			Assert.Equal(1, keys.Where(x => x == "a").Count());
		}
	}
}