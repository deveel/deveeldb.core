using System;
using System.Threading.Tasks;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public abstract class QueryPlanNodeBase : IQueryPlanNode {
		int IComparable.CompareTo(object obj) {
			throw new NotSupportedException();
		}

		int IComparable<ISqlValue>.CompareTo(ISqlValue other) {
			throw new NotSupportedException();
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			return false;
		}

		string IQueryPlanNode.NodeName => NodeName;

		protected virtual string NodeName {
			get {
				var typeName = GetType().Name;
				if (typeName.EndsWith("Node", StringComparison.OrdinalIgnoreCase)) {
					typeName = typeName.Substring(0, typeName.Length - 4);
				}

				return typeName;
			}
		}

		protected virtual IQueryPlanNode[] ChildNodes => new IQueryPlanNode[0];

		IQueryPlanNode[] IQueryPlanNode.ChildNodes => ChildNodes;

		public abstract Task<ITable> ReduceAsync(IContext context);
	}
}