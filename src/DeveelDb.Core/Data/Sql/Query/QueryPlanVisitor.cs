using System;

namespace Deveel.Data.Sql.Query {
	public class QueryPlanVisitor {
		public virtual IQueryPlanNode Visit(IQueryPlanNode node) {
			var childNodes = node.ChildNodes;
			if (childNodes != null && childNodes.Length > 0) {
				foreach (var childNode in childNodes) {
					Visit(childNode);
				}
			}

			return node;
		}
	}
}