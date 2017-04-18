using System;

namespace Deveel.Data.Sql.Tables {
	public interface ITableCache {
		bool TryGetTable(string key, out ITable table);

		void SetTable(string key, ITable table);
	}
}