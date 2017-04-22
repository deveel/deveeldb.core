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