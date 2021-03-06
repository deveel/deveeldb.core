﻿using System;
using System.Linq;
using System.Text;

using Deveel.Data.Services;

using Xunit;

namespace Deveel.Data.Configuration {
	public static class ConfigurationTests {
		[Fact]
		public static void DefaultConfig() {
			IConfiguration config = new Configuration();
			Assert.NotNull(config);
		}

		[Fact]
		public static void GetValuesFromRoot() {
			var config = new Configuration();
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
		public static void GetValuesFromChild() {
			var config = new Configuration();
			Assert.NotNull(config);

			config.SetValue("oneKey", "one");

			var child = new Configuration();
			config.AddSection("child", child);

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
		public static void GetValueAsInt32(string key, object input, int expected) {
			var config = new Configuration();
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
		public static void GetBooleanValue(string key, string value, bool expected) {
			var config = new Configuration();
			Assert.NotNull(config);

			config.SetValue(key, value);

			object configValue = config.GetBoolean(key);
			Assert.NotNull(configValue);
			Assert.IsType<bool>(configValue);
			Assert.Equal(expected, configValue);
		}

		[Fact]
		public static void GetOldStyleValue() {
			var config = new Configuration();
			config.SetValue("a", true);

			var result = config.GetValue("a", (object) true);
			Assert.NotNull(result);
			Assert.IsType<bool>(result);
			Assert.Equal(true, (bool)result);

			result = config.GetValue("b", null);
			Assert.Null(result);
		}

		[Theory]
		[InlineData(1, TestEnum.One)]
		[InlineData("one", TestEnum.One)]
		[InlineData("TWO", TestEnum.Two)]
		[InlineData(null, TestEnum.Default)]
		public static void GetEnumValue(object value, TestEnum expected) {
			var config = new Configuration();
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
		public static void GetAllKeysFromRoot() {
			var config = new Configuration();
			config.SetValue("a", 22);
			config.SetValue("b", new DateTime(2001, 02, 01));

			var keys = config.Keys;

			Assert.NotNull(keys);
			Assert.NotEmpty(keys);
			Assert.Contains("a", keys);
		}

		[Fact]
		public static void GetAllKeysFromTree() {
			var config = new Configuration();
			config.SetValue("a", 22);
			config.SetValue("b", new DateTime(2001, 02, 01));

			var child = new Configuration();
			child.SetValue("a", 56);

			config.AddSection("child", child);

			config.SetValue("c", "test");

			var keys = config.GetAllKeys();

			Assert.NotNull(keys);
			Assert.NotEmpty(keys);
			Assert.Contains("a", keys);
			Assert.Contains("child.a", keys);
		}

		[Fact]
		public static void MergeTwoConfigurations() {
			var config1 = new Configuration();
			config1.SetValue("a", 22);
			config1.SetValue("b", new DateTime(2001, 02, 01));

			var config2 = new Configuration();
			config2.SetValue("a", 55);
			config2.SetValue("c", true);

			var merged = config1.MergeWith(config2);

			var result = merged.GetInt16("a");
			Assert.Equal(55, result);
			Assert.True(merged.GetBoolean("c"));
		}

		[Fact]
		public static void ConfigureByBuildConfiguration() {
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
		public static void ConfigureByBuilder() {
			var config = Configuration.Builder()
				.WithSetting("a", 22)
				.WithSection("child",
					builder => builder
						.WithSetting("a1", "6577"))
				.Build();

			var value = config.GetDouble("a");
			Assert.Equal(22, value);
		}

		[Fact]
		public static void ConfigureFromProperties() {
			var config = Configuration.Builder()
				.AddPropertiesString(new StringBuilder()
					.AppendLine("key = 54")
					.AppendLine("sec.key = port2")
					.AppendLine("sec.key2 = 122")
					.ToString())
				.WithSetting("a", 33)
				.Build();

			var value = config.GetValue("sec.key");
			Assert.NotNull(value);
			Assert.IsType<string>(value);

			var aValue = config.GetInt64("a");
			Assert.Equal(33L, aValue);

			Assert.NotEmpty(config.Sections);
			Assert.Equal(1, config.Sections.Count());
		}

		[Fact]
		public static void RegisterAndMergeInScope() {
			var config1 = new Configuration();
			config1.SetValue("a1", 6748);
			config1.SetValue("b", "test1");

			var scope = new ServiceContainer();
			scope.RegisterInstance<IConfiguration>(config1);

			var config2 = new Configuration();
			config2.SetValue("b", 5444);

			scope.SetConfiguration(config2);

			var config = scope.Resolve<IConfiguration>();

			Assert.NotNull(config);
			Assert.Equal(5444, config.GetInt32("b"));
		}
	}
}