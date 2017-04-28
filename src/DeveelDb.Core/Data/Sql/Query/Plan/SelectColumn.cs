using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class SelectColumn {
		public SqlExpression Expression { get; set; }

		public string Alias { get; set; }

		public ObjectName ResolvedName { get; set; }

		public ObjectName InternalName { get; set; }

		public ObjectName GlobName { get; set; }
	}
}