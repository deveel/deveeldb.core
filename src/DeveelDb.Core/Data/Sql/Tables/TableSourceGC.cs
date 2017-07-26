using System;
using System.IO;

using Deveel.Data.Diagnostics;
using Deveel.Data.Indexes;

namespace Deveel.Data.Sql.Tables {
	class TableSourceGC {
		private readonly TableSource tableSource;
		private BlockIndex<SqlObject, long> deletedRows;

		private DateTimeOffset lastSuccess;
		private DateTimeOffset? lastTry;

		private bool fullSweep;

		public TableSourceGC(TableSource tableSource) {
			this.tableSource = tableSource;

			deletedRows = new BlockIndex<SqlObject, long>();
			lastSuccess = DateTimeOffset.UtcNow;
			lastTry = null;
		}

		public void OnRowDeleted(long row) {
			if (!deletedRows.UniqueInsertSort(row))
				throw new InvalidOperationException($"The row {row} was already marked as deleted");
		}

		public void Collect(bool force) {
			try {
				int checkCount = 0;
				int deleteCount = 0;

				// Synchronize over the master data table source so no other threads
				// can interfere when we collect this information.
				lock (tableSource) {
					if (tableSource.IsClosed)
						return;

					// If root is locked, or has transaction changes pending, then we
					// can't delete any rows marked as deleted because they could be
					// referenced by transactions or result sets.
					if (force ||
					    (!tableSource.IsRootLocked &&
					     !tableSource.HasChangesPending)) {

						lastSuccess = DateTimeOffset.Now;
						lastTry = null;

						// Are we due a full sweep?
						if (fullSweep) {
							var rawRowCount = tableSource.RawRowCount;
							for (int i = 0; i < rawRowCount; ++i) {
								// Synchronized in dataSource.
								if (tableSource.HardCheckAndReclaimRow(i))
									++deleteCount;

								++checkCount;
							}

							fullSweep = false;
						} else {
							// Are there any rows marked as deleted?
							var size = deletedRows.Count;
							if (size > 0) {
								// Go remove all rows marked as deleted.
								foreach (int rowIndex in deletedRows) {
									// Synchronized in dataSource.
									tableSource.HardRemoveRow(rowIndex);
									++deleteCount;
									++checkCount;
								}
							}

							deletedRows = new BlockIndex<SqlObject, long>();
						}

						if (checkCount > 0) {
							tableSource.SystemContext.Information(-1, $"The table GC was checked {checkCount} times and a delete was done {deleteCount} times");
						}

					} // if not roots locked and not transactions pending

				} // lock
			} catch (IOException ex) {
				tableSource.SystemContext.Error(-1, "An error occurred while garbage collecting on table", ex);
			}
		}
	}
}