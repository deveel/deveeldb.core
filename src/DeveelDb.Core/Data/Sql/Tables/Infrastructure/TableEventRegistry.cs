using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	class TableEventRegistry : ITableEventRegistry {
		public IEnumerator<ITableEvent> GetEnumerator() {
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public long CommitId { get; }
	}
}