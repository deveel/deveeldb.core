using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class SimpleSelectNode : SingleQueryPlanNode {
		public SimpleSelectNode(IQueryPlanNode child, ObjectName columnName, SqlExpressionType op, SqlExpression expression)
			: base(child) {
			ColumnName = columnName;
			Operator = op;
			Expression = expression;
		}

		public ObjectName ColumnName { get; }

		public SqlExpressionType Operator { get; }

		public SqlExpression Expression { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);
			return await table.SimpleSelectAsync(context, ColumnName, Operator, Expression);
		}
	}
}