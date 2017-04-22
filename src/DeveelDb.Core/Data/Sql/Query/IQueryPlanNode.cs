using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public interface IQueryPlanNode : ISqlValue {
		string NodeName { get; }

		IDictionary<string, object> Data { get; }
		
		IQueryPlanNode[] ChildNodes { get; }


		Task<ITable> ReduceAsync(IContext context);
	}
}