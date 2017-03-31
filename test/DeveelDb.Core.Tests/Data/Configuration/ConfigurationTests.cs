using System;
using System.Linq;

using Xunit;

namespace Deveel.Data.Configuration {
	public class ConfigurationTests {
		[Fact]
		public void DefaultConfig() {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);
		}

		[Fact]
		public void GetValuesFromRoot() {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);
			config.SetValue("oneKey", 54);
			config.SetValue("twoKeys", null);

			var value1 = config.GetValue("oneKey");
			Assert.NotNull(value1);
			Assert.IsType<int>(value1);
			Assert.Equal(54, value1);

			var value2 = config.GetValue("twoKeys");
			Assert.Null(value2);
		}

		[Fact]
		public void GetValuesFromChild() {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);

			config.SetValue("oneKey", "one");

			IConfiguration child = new Configuration();
			config.AddChild("child", child);

			child.SetValue("oneKey", 45);

			var value = child.GetValue("oneKey");
			Assert.NotNull(value);
			Assert.IsType<int>(value);
			Assert.Equal(45, value);

			value = config.GetValue("child.oneKey");
			Assert.NotNull(value);
			Assert.IsType<int>(value);
			Assert.Equal(45, value);
		}

		[Theory]
		[InlineData("test.oneKey", 22, 22)]
		[InlineData("test.oneKey", "334", 334)]
		public void GetValueAsInt32(string key, object input, int expected) {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);

			config.SetValue(key, input);

			object value = config.GetInt32(key);
			Assert.NotNull(value);
			Assert.IsType<int>(value);
			Assert.Equal(expected, value);
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

			var keys = config.Keys;

			Assert.NotNull(keys);
			Assert.NotEmpty(keys);
			Assert.Contains("a", keys);
		}

		[Fact]
		public void GetAllKeysFromTree() {
			var config = new Configuration();
			config.SetValue("a", 22);
			config.SetValue("b", new DateTime(2001, 02, 01));

			var child = new Configuration();
			child.SetValue("a", 56);

			config.AddChild("child", child);

			config.SetValue("c", "test");

			var keys = config.GetAllKeys();

			Assert.NotNull(keys);
			Assert.NotEmpty(keys);
			Assert.Contains("a", keys);
			Assert.Contains("child.a", keys);
		}

		[Fact]
		public void ConfigureByBuildConfiguration() {
			var config = Configuration.Build(builder => {
				builder.WithSetting("a", 43.01f)
					.WithSetting("b", "test");
			});

			Assert.NotEmpty(config);
			Assert.NotEmpty(config);

			var value = config.GetSingle("a");
			Assert.Equal(43.01f, value);

			var s = config.GetString("b");
			Assert.Equal("test", s);
		}

		[Fact]
		public void ConfigureByBuilder() {
			var config = Configuration.Builder()
				.WithSetting("a", 22)
				.WithSection("child",
					builder => builder
						.WithSetting("a1", "6577"))
				.Build();

			var value = config.GetDouble("a");
			Assert.Equal(22, value);
		}
	}
}