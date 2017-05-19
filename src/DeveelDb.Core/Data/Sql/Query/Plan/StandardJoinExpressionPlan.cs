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

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class StandardJoinExpressionPlan : ExpressionPlan {
		public StandardJoinExpressionPlan(SqlBinaryExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			Expression = expression;
		}

		public SqlBinaryExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			var lhsRef = Expression.Left.AsReference();
			var rhsRef = Expression.Right.AsReference();

			var lhsRefs = Expression.Left.DiscoverReferences();
			var rhsRefs = Expression.Right.DiscoverReferences();

			var op = Expression.ExpressionType;

			var lhsPlan = plan.JoinAllPlansWithReferences(lhsRefs);
			var rhsPlan = plan.JoinAllPlansWithReferences(rhsRefs);

			// If the lhs and rhs plans are different (there is a joining
			// situation).
			if (lhsPlan != rhsPlan) {
				// If either the LHS or the RHS is a single variable then we can
				// optimize the join.

				if (lhsRef != null || rhsRef != null) {
					// If rhs_v is a single variable and lhs_v is not then we must
					// reverse the expression.
					JoinNode join_node;
					if (lhsRef == null && rhsRef != null) {
						// Reverse the expressions and the operator
						join_node = new JoinNode(rhsPlan.Plan, lhsPlan.Plan, rhsRef, op.Reverse(), Expression.Left);
						plan.MergeTables(rhsPlan, lhsPlan, join_node);
					} else {
						// Otherwise, use it as it is.
						join_node = new JoinNode(lhsPlan.Plan, rhsPlan.Plan, lhsRef, op, Expression.Right);
						plan.MergeTables(lhsPlan, rhsPlan, join_node);
					}

					// Return because we are done
					return;
				}

			} // if lhs and rhs plans are different

			// If we get here either both the lhs and rhs are complex expressions
			// or the lhs and rhs of the variable are not different plans, or
			// the operator is not a conditional.  Either way, we must evaluate
			// this via a natural join of the variables involved coupled with an
			// exhaustive select.  These types of queries are poor performing.

			// Get all the variables in the expression
			var allRefs = Expression.DiscoverReferences();
			// Merge it into one plan (possibly performing natural joins).
			var allPlan = plan.JoinAllPlansWithReferences(allRefs);
			// And perform the exhaustive select,
			allPlan.UpdatePlan(new FullSelectNode(allPlan.Plan, Expression));
		}
	}
}