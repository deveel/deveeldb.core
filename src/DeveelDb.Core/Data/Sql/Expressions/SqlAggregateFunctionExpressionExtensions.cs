using System;

namespace Deveel.Data.Sql.Expressions {
	static class SqlAggregateFunctionExpressionExtensions {
		public static bool HasAggregate(this SqlExpression expression, IContext context) {
			var visitor = new AggregateVisitor(context);
			visitor.Visit(expression);
			return visitor.HasAggregates;
		}

		#region AggregateVisitor

		class AggregateVisitor : SqlExpressionVisitor {
			private IContext context;

			public AggregateVisitor(IContext context) {
				this.context = context;
			}

			public bool HasAggregates { get; private set; }

			public override SqlExpression VisitFunction(SqlFunctionExpression expression) {
				// TODO: resolve the function and gett its type

				return base.VisitFunction(expression);
			}
		}

		#endregion
	}
}