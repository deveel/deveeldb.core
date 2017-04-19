using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public static class ContextExtensions {
		public static async Task<ITableQueryInfo> GetTableQueryInfoAsync(this IContext context, ObjectName tableName, ObjectName alias) {
			var tableInfo = await context.GetTableInfoAsync(tableName);
			if (alias != null) {
				tableInfo = TableInfo.Alias(tableInfo, alias);
			}

			return new TableQueryInfo(context, tableInfo, tableName, alias);
		}

		public static async Task<IQueryPlanNode> CreateQueryPlanAsync(this IContext context, ObjectName tableName, ObjectName aliasedName) {
			var tableType = await context.GetObjectType(tableName);
			switch (tableType) {
				case DbObjectType.Table:
					return new FetchTableNode(tableName, aliasedName);
				default:
					throw new NotSupportedException();
			}
		}

		#region TableQueryInfo

		class TableQueryInfo : ITableQueryInfo {
			private readonly IContext context;

			public TableQueryInfo(IContext context, TableInfo tableInfo, ObjectName tableName, ObjectName aliasName) {
				this.context = context;
				TableInfo = tableInfo;
				TableName = tableName;
				AliasName = aliasName;
			}

			public TableInfo TableInfo { get; }

			public ObjectName TableName { get; }

			public ObjectName AliasName { get; }

			public Task<IQueryPlanNode> GetQueryPlanAsync() {
				return context.CreateQueryPlanAsync(TableName, AliasName);
			}
		}

		#endregion
	}
}