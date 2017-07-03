using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Sql.Statements;

namespace Deveel.Data.Sql.Parsing {
	public sealed class SqlParseResult {
		public SqlParseResult() {
			Messages = new List<SqlParseMessage>();
			Statements = new List<SqlStatement>();
		}

		public ICollection<SqlParseMessage> Messages { get; }

		public ICollection<SqlStatement> Statements { get; }

		public bool Failed => Messages.Any(x => x.Level == SqlParseMessageLevel.Error);

		public bool Succeeded => Messages.All(x => x.Level != SqlParseMessageLevel.Error);
	}
}