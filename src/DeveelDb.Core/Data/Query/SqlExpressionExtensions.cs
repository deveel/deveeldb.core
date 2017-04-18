using System;
using System.Collections.Generic;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query {
	static class SqlExpressionExtensions {
		public static IEnumerable<ObjectName> DiscoverReferences(this SqlExpression expression) {
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