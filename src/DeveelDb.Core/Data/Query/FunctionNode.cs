using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class FunctionNode : SingleQueryPlanNode {
		public FunctionNode(IQueryPlanNode child, SqlExpression[] functions, string[] functionNames)
			: base(child) {
			Functions = functions;
			FunctionNames = functionNames;
		}

		public SqlExpression[] Functions { get; }

		public string[] FunctionNames { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);

			var columns = new FunctionColumnInfo[Functions.Length];

			// Create a new DataColumnInfo for each expression, and work out if the
			// expression is simple or not.
			for (int i = 0; i < Functions.Length; ++i) {
				var expr = Functions[i];
				// Examine the expression and determine if it is simple or not
				if (expr.IsConstant() && !expr.HasAggregate(context)) {
					// If expression is a constant, solve it
					var result = await expr.ReduceAsync(context);
					if (result.ExpressionType != SqlExpressionType.Constant)
						throw new InvalidOperationException();

					var sqlType = expr.GetSqlType(context);
					columns[i] = new FunctionColumnInfo(expr, FunctionNames[i], sqlType, true);
				} else {
					// Otherwise must be dynamic
					var sqlType = expr.GetSqlType(context);
					columns[i] = new FunctionColumnInfo(expr, FunctionNames[i], sqlType, false);
				}
			}

			var function = new FunctionTable(context, table, columns);
			return function.GroupMax(null);
		}
	}
}