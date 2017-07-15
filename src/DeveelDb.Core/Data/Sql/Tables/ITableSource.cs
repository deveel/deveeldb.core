using System;

using Deveel.Data.Sql.Tables;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables {
	public interface ITableSource : IDisposable {
		int TableId { get; }

		TableInfo TableInfo { get; }


		long GetCurrentUniqueId();

		void SetUniqueId(long value);

		long GetNextUniqueId();

		IMutableTable CreateTableAtCommit(ITransaction transaction);
	}
}
