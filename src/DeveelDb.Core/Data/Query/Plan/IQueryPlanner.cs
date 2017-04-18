using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query.Plan {
	public interface IQueryPlanner {
		Task<IQueryPlanNode> PlanAsync(IContext context, SqlQueryExpression expression);
	}
}