// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


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
			private IList<CorrelatedReferenceExpression> expressions;
			private readonly int level;

			public CorrelatedReferenceDiscovery(int level, IList<CorrelatedReferenceExpression> expressions) {
				this.level = level;
				this.expressions = expressions;
			}

			public override SqlExpression Visit(SqlExpression expression) {
				if (expression is CorrelatedReferenceExpression) {
					var correlated = (CorrelatedReferenceExpression) expression;
					if (correlated.QueryLevel == level &&
					    !expressions.Any(x => x.ReferenceName.Equals(correlated.ReferenceName))) {
						expressions.Add(correlated);
					}
				}

				return base.Visit(expression);
			}

			public override SqlExpression VisitConstant(SqlConstantExpression expression) {
				if (expression.Value.Type is SqlQueryType) {
					var queryPlan = (IQueryPlanNode) expression.Value.Value;
					expressions = queryPlan.DiscoverCorrelatedReferences(level + 1, expressions);
				}

				return base.VisitConstant(expression);
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