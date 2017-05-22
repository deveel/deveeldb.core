using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;

namespace Deveel.Data.Sql.Statements {
	public sealed class NullStatement : SqlStatement, IPlSqlStatement {
		public NullStatement() {
			
		}

		private NullStatement(SerializationInfo info)
			: base(info) {
		}

		protected override Task ExecuteStatementAsync(StatementContext context) {
			return Task.CompletedTask;
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("NULL;");
		}
	}
}