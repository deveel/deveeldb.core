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