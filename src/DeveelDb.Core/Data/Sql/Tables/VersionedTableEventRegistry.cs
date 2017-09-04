using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Tables {
	class VersionedTableEventRegistry : IDisposable {
		private List<ITableEventRegistry> eventRegistries;

		public VersionedTableEventRegistry(TableSource tableSource) {
			TableSource = tableSource;

			eventRegistries = new List<ITableEventRegistry>();
		}

		public TableSource TableSource { get; private set; }

		public bool HasChanges => eventRegistries.Any();

		private void Dispose(bool disposing) {
			if (disposing) {
				if (eventRegistries != null)
					eventRegistries.Clear();
			}

			eventRegistries = null;
			TableSource = null;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void AddRegistry(ITableEventRegistry registry) {
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

		public IEnumerable<ITableEventRegistry> FindSinceCommit(long commitId) {
			return eventRegistries.Where(x => x.CommitId >= commitId);
		}
	}
}