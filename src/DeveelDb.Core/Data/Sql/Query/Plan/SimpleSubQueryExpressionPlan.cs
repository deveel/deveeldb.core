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
	class SimpleSubQueryExpressionPlan : ExpressionPlan {
		public SimpleSubQueryExpressionPlan(SqlQuantifyExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			Expression = expression;
		}

		public SqlQuantifyExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			var op = Expression.ExpressionType;
			var subOp = Expression.Expression.ExpressionType;
			var leftRef = Expression.Expression.Left.AsReference();
			var rightPlan = Expression.Expression.Right.AsQueryPlanNode();

			var tablePlan = plan.FindTablePlan(leftRef);
			var leftPlan = tablePlan.Plan;

			tablePlan.UpdatePlan(new NonCorrelatedSelectNode(leftPlan, rightPlan, new []{leftRef}, op, subOp));
		}
	}
}