using System;

using Deveel.Data.Indexes;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables {
	public interface ITableSource : IDisposable {
		long CurrentUniqueId { get; }


		void SetUniqueId(long value);

		long GetNextUniqueId();

		int TableId { get; }

		TableInfo TableInfo { get; }

		IIndexSet<SqlObject,long> CreateIndexSet();


		IMutableTable CreateTransactionTable(ITransaction transaction);

		long AddRow(Row row);

		RecordState WriteRecordState(long rowNumber, RecordState recordState);

		void BuildIndexes();

	}
}