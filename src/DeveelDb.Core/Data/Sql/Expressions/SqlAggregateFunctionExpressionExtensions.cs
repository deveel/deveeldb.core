using System;

using Deveel.Data.Sql.Methods;
using Deveel.Data.Services;

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
				var resolver = context.Scope.Resolve<IMethodResolver>();
				if (resolver == null)
					throw new SqlExpressionException("No method resolver defined in this context");

				var method = resolver.ResolveMethod(context, new Invoke(expression.FunctionName, expression.Arguments));
				if (method != null && method.IsFunction &&
					((SqlFunctionBase)method).FunctionType == FunctionType.Aggregate &&
					!HasAggregates) {
					HasAggregates = true;
				}

				return base.VisitFunction(expression);
			}

		}

		#endregion
	}
}