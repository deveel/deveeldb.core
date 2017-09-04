using System;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables {
	public interface ITableEventRegistry : IEventRegistry<ITableEvent> {
		long CommitId { get; }
	}
}