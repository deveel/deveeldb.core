using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class FetchTableNode : QueryPlanNodeBase {
		public FetchTableNode(ObjectName tableName) : this(tableName, null) {
		}

		public FetchTableNode(ObjectName tableName, ObjectName alias) {
			TableName = tableName;
			Alias = alias;
		}

		public ObjectName TableName { get; }

		public ObjectName Alias { get; }

		protected override void GetData(IDictionary<string, object> data) {
			data["table"] = TableName;
			data["alias"] = Alias;
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await context.GetTableAsync(TableName);
			if (Alias != null)
				table = new AliasedTable(table, Alias);

			return table;
		}
	}
}