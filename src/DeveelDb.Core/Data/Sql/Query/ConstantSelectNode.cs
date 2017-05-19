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
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
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