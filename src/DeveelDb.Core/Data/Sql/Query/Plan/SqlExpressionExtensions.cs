using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	static class SqlExpressionExtensions {
		public static IList<CorrelatedReferenceExpression> DiscoverCorrelatedReferences(this SqlExpression expression,
			int queryLevel) {
			return DiscoverCorrelatedReferences(expression, queryLevel, new List<CorrelatedReferenceExpression>());
		}

		public static IList<CorrelatedReferenceExpression> DiscoverCorrelatedReferences(this SqlExpression expression,
			int queryLevel, IList<CorrelatedReferenceExpression> list) {
			var visitor = new CorrelatedReferenceDiscovery(queryLevel, list);
			visitor.Visit(expression);
			return list;
		}

		public static bool HasSubQuery(this SqlExpression expression) {
			var visitor = new SubQueryDiscovery();
			visitor.Visit(expression);
			return visitor.SubQueryFound;
		}

		#region CorrelatedReferenceDiscovery

		class CorrelatedReferenceDiscovery : SqlExpressionVisitor {
			private readonly IList<CorrelatedReferenceExpression> expressions;
			private readonly int level;

			public CorrelatedReferenceDiscovery(int level, IList<CorrelatedReferenceExpression> expressions) {
				this.level = level;
				this.expressions = expressions;
			}

			public override SqlExpression Visit(SqlExpression expression) {
				if (expression is CorrelatedReferenceExpression) {
					var correlated = (CorrelatedReferenceExpression) expression;
					if (correlated.QueryLevel == level &&
					    !expressions.Any(x => x.Reference.Equals(correlated.Reference))) {
						expressions.Add(correlated);
					}
				}

				return base.Visit(expression);
			}
		}

		#endregion

		#region SubQueryDiscovery

		class SubQueryDiscovery : SqlExpressionVisitor {
			public bool SubQueryFound { get; private set; }

			public override SqlExpression VisitConstant(SqlConstantExpression expression) {
				if (expression.Value.Type is SqlQueryType)
					SubQueryFound = true;

				return base.VisitConstant(expression);
			}
		}

		#endregion
	}
}