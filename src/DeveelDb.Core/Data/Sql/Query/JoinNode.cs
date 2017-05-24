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
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class JoinNode : BranchQueryPlanNode {
		public JoinNode(IQueryPlanNode left, IQueryPlanNode right, ObjectName leftColumnName, SqlExpressionType op, SqlExpression rightExpression)
			: base(left, right) {
			LeftColumnName = leftColumnName;
			Operator = op;
			RightExpression = rightExpression;
		}

		public ObjectName LeftColumnName { get; }

		public SqlExpressionType Operator { get; }

		public SqlExpression RightExpression { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var left = await Left.ReduceAsync(context);
			var right = await Right.ReduceAsync(context);

			return await left.JoinAsync(context, right, LeftColumnName, Operator, RightExpression);
		}
	}
}