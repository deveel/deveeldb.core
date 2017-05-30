using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public interface ITableEventRegistry : IEnumerable<ITableEvent> {
		long CommitId { get; }
	}
}