using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class ConstantSelectNode : SingleQueryPlanNode {
		public ConstantSelectNode(IQueryPlanNode child, SqlExpression expression)
			: base(child) {
			Expression = expression;
		}

		public SqlExpression Expression { get; }

		protected override void GetData(IDictionary<string, object> data) {
			data["constant"] = Expression;
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var result = await Expression.ReduceAsync(context);

			if (result.ExpressionType != SqlExpressionType.Constant)
				throw new InvalidOperationException("The expression was not reduced to constant");

			var value = ((SqlConstantExpression) result).Value;

			var table = await Child.ReduceAsync(context);

			if (value.IsNull || value.IsFalse)
				table = table.EmptySelect();

			return table;
		}
	}
}