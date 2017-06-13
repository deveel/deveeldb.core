using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	class VersionedTableEventRegistry : IDisposable {
		private List<TableEventRegistry> eventRegistries;

		public VersionedTableEventRegistry(TableSource tableSource) {
			TableSource = tableSource;

			eventRegistries = new List<TableEventRegistry>();
		}

		~VersionedTableEventRegistry() {
			Dispose(false);
		}

		public TableSource TableSource { get; private set; }

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (eventRegistries != null)
					eventRegistries.Clear();
			}

			eventRegistries = null;
			TableSource = null;
		}

		public void AddRegistry(TableEventRegistry registry) {
			eventRegistries.Add(registry);
		}

		public bool MergeChanges(long commitId) {
			// TODO: report the stat to the system

			while (eventRegistries.Count > 0) {
				var registry = eventRegistries[0];

				if (commitId > registry.CommitId) {
					// Remove the top registry from the list.
					eventRegistries.RemoveAt(0);
				} else {
					return false;
				}
			}

			return true;
		}
	}
}