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
	public sealed class GroupNode : SingleQueryPlanNode {
		public GroupNode(IQueryPlanNode child, ObjectName groupMax, SqlExpression[] functions, string[] functionNames)
			: this(child, new ObjectName[0], groupMax, functions, functionNames) {
		}

		public GroupNode(IQueryPlanNode child, ObjectName[] columnNames, ObjectName groupMax, SqlExpression[] functions, string[] functionNames)
			: base(child) {
			ColumnNames = columnNames;
			GroupMax = groupMax;
			Functions = functions;
			FunctionNames = functionNames;
		}

		public ObjectName[] ColumnNames { get; }

		public ObjectName GroupMax { get; }

		public SqlExpression[] Functions { get; }

		public string[] FunctionNames { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);

			var columns = new FunctionColumnInfo[Functions.Length];

			// Create a new DataColumnInfo for each expression, and work out if the
			// expression is simple or not.
			for (int i = 0; i < Functions.Length; ++i) {
				var expr = Functions[i];

				var sqlType = expr.GetSqlType(context);

				// Examine the expression and determine if it is simple or not
				if (expr.IsConstant() && !expr.HasAggregate(context)) {
					// If expression is a constant, solve it
					var result = await expr.ReduceAsync(context);
					if (result.ExpressionType != SqlExpressionType.Constant)
						throw new InvalidOperationException();

					columns[i] = new FunctionColumnInfo(result, FunctionNames[i], sqlType, true);
				} else {
					// Otherwise must be dynamic
					columns[i] = new FunctionColumnInfo(expr, FunctionNames[i], sqlType, false);
				}
			}

			var group = new GroupTable(context, table, columns, ColumnNames);

			return group.GroupMax(GroupMax);
		}
	}
}