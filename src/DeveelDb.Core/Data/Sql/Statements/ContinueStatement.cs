using System;

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public sealed class ContinueStatement : LoopControlStatement {
		public ContinueStatement() : base(LoopControlType.Continue) {
		}

		public ContinueStatement(SqlExpression when) : base(LoopControlType.Continue, when) {
		}

		public ContinueStatement(string label) : base(LoopControlType.Continue, label) {
		}

		public ContinueStatement(string label, SqlExpression when) : base(LoopControlType.Continue, label, when) {
		}

		private ContinueStatement(SerializationInfo info) : base(info) {
		}
	}
}