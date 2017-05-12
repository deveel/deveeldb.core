using System;
using System.IO;

using Deveel.Data.Configuration;
using Deveel.Data.Services;

using Xunit;

namespace Deveel.Data {
	public class SystemBuildTests {
		[Fact]
		public async void BuildDefault() {
			Environment.SetEnvironmentVariable("DEVEELDB_ENVIRONMENT", "Production");

			var system = new SystemBuilder()
				.UseRootPath(Directory.GetCurrentDirectory())
				.UseSystemServices()
				.Build();

			Assert.NotNull(system);
			Assert.NotNull(system.Configuration);
			Assert.NotNull(system.Scope);
			Assert.Null(system.ParentContext);
			Assert.Equal("system", system.ContextName);

			var basePath = system.Configuration.RootPath();
			Assert.NotNull(basePath);
			Assert.Equal(Directory.GetCurrentDirectory(), basePath);

			var env = system.Scope.Resolve<ISystemEnvironment>();
			Assert.NotNull(env);
			Assert.Equal("Production", env.EnvironmentName);

			await system.StartAsync();

			Assert.Empty(system.GetDatabases());
		}

		[Fact]
		public async void BuildDefaultAndCreateDatabase() {
			var system = new SystemBuilder()
				.UseRootPath(Directory.GetCurrentDirectory())
				.UseSystemServices()
				.Build();

			Assert.NotNull(system);
			Assert.NotNull(system.Configuration);
			Assert.NotNull(system.Scope);
			Assert.Null(system.ParentContext);

			var config = new ConfigurationBuilder()
				.WithSetting("database.name", "test")
				.Build();

			await system.StartAsync();

			var database = await system.CreateDatabaseAsync(config);

			Assert.NotNull(database);
			Assert.Equal("test", database.Name);
		}
	}
}