using System;

namespace Deveel.Data.Sql {
	public sealed class SqlDeterministicType : SqlType {
		public SqlDeterministicType()
			: base(SqlTypeCode.Unknown) {
		}

		public override bool IsComparable(SqlType type) {
			return false;
		}

		public override bool CanCastTo(ISqlValue value, SqlType destType) {
			return false;
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("DETERMINISTIC");
		}
	}
}