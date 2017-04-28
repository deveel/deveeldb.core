using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class SimplePatternSelectNode : SingleQueryPlanNode {
		public SimplePatternSelectNode(IQueryPlanNode child, ObjectName columnName, SqlExpressionType op, SqlExpression pattern, SqlExpression escape)
			: base(child) {
			Column = columnName;
			Operator = op;
			Pattern = pattern;
			Escape = escape;
		}

		public ObjectName Column { get; }

		public SqlExpressionType Operator { get; }

		public SqlExpression Pattern { get; }

		public SqlExpression Escape { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);

			return await table.SearchAsync(context, Column, Operator, Pattern, Escape);
		}
	}
}