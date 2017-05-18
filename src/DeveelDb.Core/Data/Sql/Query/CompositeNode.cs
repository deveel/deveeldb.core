using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class CompositeNode : BranchQueryPlanNode {
		public CompositeNode(IQueryPlanNode left, IQueryPlanNode right, CompositeFunction function, bool all) : base(left, right) {
			Function = function;
			All = all;
		}

		public CompositeFunction Function { get; }

		public bool All { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var left = await Left.ReduceAsync(context);
			var right = await Right.ReduceAsync(context);

			return new CompositeTable(left, new []{right}, Function, All);
		}
	}
}