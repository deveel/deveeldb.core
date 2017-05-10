using System;
using System.Threading.Tasks;

using Deveel.Data.Configuration;

namespace Deveel.Data {
	public static class DatabaseSystemExtensions {
		public static Task<IDatabase> CreateDatabaseAsync(this IDatabaseSystem system, IConfiguration configuration) {
			return system.CreateDatabaseAsync(new DatabaseBuildInfo(configuration));
		}
	}
}