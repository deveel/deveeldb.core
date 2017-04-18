using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query {
	public interface IQueryPlanner {
		Task<IQueryPlanNode> PlanAsync(IContext context, SqlQueryExpression expression);
	}
}