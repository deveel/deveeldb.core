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

namespace Deveel.Data.Sql.Query.Plan {
	static class QueryPlanNodeExtensions {
		public static IList<ObjectName> DiscoverTableNames(this IQueryPlanNode queryPlan) {
			var tableNames = new List<ObjectName>();
			var visitor = new TableNameDicovery(tableNames);
			visitor.Visit(queryPlan);
			return tableNames;
		}

		public static IList<CorrelatedReferenceExpression> DiscoverCorrelatedReferences(this IQueryPlanNode queryPlan,
			int queryLevel) {
			return DiscoverCorrelatedReferences(queryPlan, queryLevel, new List<CorrelatedReferenceExpression>());
		}

		public static IList<CorrelatedReferenceExpression> DiscoverCorrelatedReferences(this IQueryPlanNode queryPlan,
			int queryLevel, IList<CorrelatedReferenceExpression> list) {
			var visitor = new CorrelatedExpressionDiscovery(queryLevel, list);
			visitor.Visit(queryPlan);
			return list;
		}

		#region TableNameDiscovery

		class TableNameDicovery : QueryPlanVisitor {
			private IList<ObjectName> tableNames;

			public TableNameDicovery(IList<ObjectName> tableNames) {
				this.tableNames = tableNames;
			}

			public override IQueryPlanNode Visit(IQueryPlanNode node) {
				if (node is FetchTableNode) {
					var fetchTableNode = (FetchTableNode) node;
					if (!tableNames.Contains(fetchTableNode.TableName))
						tableNames.Add(fetchTableNode.TableName);
				}

				return base.Visit(node);
			}
		}

		#endregion

		#region CorrelatedExpressionDiscovery

		class CorrelatedExpressionDiscovery : QueryPlanVisitor {
			private IList<CorrelatedReferenceExpression> list;
			private readonly int queryLevel;

			public CorrelatedExpressionDiscovery(int queryLevel, IList<CorrelatedReferenceExpression> list) {
				this.queryLevel = queryLevel;
				this.list = list;
			}

			public override IQueryPlanNode Visit(IQueryPlanNode node) {
				if (node is ConstantSelectNode) {
					list = ((ConstantSelectNode) node).Expression.DiscoverCorrelatedReferences(queryLevel, list);
				} else if (node is FunctionNode) {
					var function = (FunctionNode) node;
					foreach (var expression in function.Functions) {
						list = expression.DiscoverCorrelatedReferences(queryLevel, list);
					}
				} else if (node is FullSelectNode) {
					list = ((FullSelectNode) node).Expression.DiscoverCorrelatedReferences(queryLevel, list);
				} else if (node is GroupNode) {
					var group = (GroupNode) node;
					foreach (var function in group.Functions) {
						list = function.DiscoverCorrelatedReferences(queryLevel, list);
					}
				} else if (node is JoinNode) {
					list = ((JoinNode) node).RightExpression.DiscoverCorrelatedReferences(queryLevel, list);
				} else if (node is RangeSelectNode) {
					list = ((RangeSelectNode) node).Expression.DiscoverCorrelatedReferences(queryLevel, list);
				} else if (node is SimplePatternSelectNode) {
					list = ((SimplePatternSelectNode) node).Pattern.DiscoverCorrelatedReferences(queryLevel, list);
				} else if (node is SimpleSelectNode) {
					list = ((SimpleSelectNode) node).Expression.DiscoverCorrelatedReferences(queryLevel, list);
				}

				return base.Visit(node);
			}
		}

		#endregion
	}
}