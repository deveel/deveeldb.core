using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	public interface IQueryPlanner {
		Task<IQueryPlanNode> PlanAsync(IContext context, QueryInfo queryInfo);
	}
}