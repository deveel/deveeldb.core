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

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query {
	static class SqlExpressionExtensions {
		public static ObjectName AsReference(this SqlExpression expression) {
			return (expression as SqlReferenceExpression)?.ReferenceName;
		}

		public static IQueryPlanNode AsQueryPlanNode(this SqlExpression expression) {
			if (expression is SqlConstantExpression &&
			    ((SqlConstantExpression) expression).Type is SqlQueryType)
				return (IQueryPlanNode) ((SqlConstantExpression) expression).Value.Value;

			return null;
		}

		public static IList<ObjectName> DiscoverReferences(this SqlExpression expression) {
			var visitor = new ReferenceDiscovery();
			visitor.Visit(expression);
			return visitor.References;
		}

		#region RefrerenceDiscovery

		class ReferenceDiscovery : SqlExpressionVisitor {
			public List<ObjectName> References { get; }

			public ReferenceDiscovery() {
				References = new List<ObjectName>();
			}

			public override SqlExpression VisitReference(SqlReferenceExpression expression) {
				if (!References.Contains(expression.ReferenceName))
					References.Add(expression.ReferenceName);

				return base.VisitReference(expression);
			}

			public override SqlExpression VisitReferenceAssign(SqlReferenceAssignExpression expression) {
				if (!References.Contains(expression.ReferenceName))
					References.Add(expression.ReferenceName);

				return base.VisitReferenceAssign(expression);
			}
		}

		#endregion
	}
}