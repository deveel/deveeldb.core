using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Deveel.Data.Services;

namespace Deveel.Data.Sql.Indexes {
	public static class ContextExtensions {
		public static async Task<Index> GetIndexAsync(this IContext context, ObjectName indexName) {
			return (Index) await context.GetObjectAsync(DbObjectType.Index, indexName);
		}

		public static Index GetIndex(this IContext context, ObjectName indexName) {
			return context.GetIndexAsync(indexName).Result;
		}

		public static async Task<Index> CreateIndexAsync(this IContext context, IndexInfo indexInfo) {
			return (Index) await context.CreateObjectAsync(indexInfo);
		}

		public static Index CreateIndex(this IContext context, IndexInfo indexInfo) {
			return context.CreateIndexAsync(indexInfo).Result;
		}

		public static Task<IndexInfo> FindIndexAsync(this IContext context, ObjectName tableName, string[] columnNames) {
			var manager = context.Scope.Resolve<IIndexManager>();
			if (manager == null)
				return null;

			return manager.FindIndexAsync(tableName, columnNames);
		}

		public static IndexInfo FindIndex(this IContext context, ObjectName tableName, string[] columnNames) {
			return context.FindIndexAsync(tableName, columnNames).Result;
		}
	}
}