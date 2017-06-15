using System;

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public sealed class ExitStatement : LoopControlStatement {
		public ExitStatement() : base(LoopControlType.Exit) {
		}

		public ExitStatement(SqlExpression when) : base(LoopControlType.Exit, when) {
		}

		public ExitStatement(string label) : base(LoopControlType.Exit, label) {
		}

		public ExitStatement(string label, SqlExpression when) : base(LoopControlType.Exit, label, when) {
		}

		private ExitStatement(SerializationInfo info)
			: base(info) {
		}
	}
}