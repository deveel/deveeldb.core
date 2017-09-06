using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Indexes {
	public interface IIndexManager : IDbObjectManager {
		Task<IndexInfo> FindIndexAsync(ObjectName tableName, string[] columnNames);
	}
}