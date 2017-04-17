using System;
using System.Threading.Tasks;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public interface IQueryPlanNode : ISqlValue {
		Task<ITable> ReduceAsync(IContext context);
	}
}