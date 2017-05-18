using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Statements {
	public interface IStatementContainer {
		IEnumerable<SqlStatement> Statements { get; }
	}
}