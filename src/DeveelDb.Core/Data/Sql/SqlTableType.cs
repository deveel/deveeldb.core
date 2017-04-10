using System;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql {
	public sealed class SqlTableType : SqlType {
		internal SqlTableType()
			: base(SqlTypeCode.Table) {
		}

		public override bool IsInstanceOf(ISqlValue value) {
			return value is ITable;
		}

		public override bool IsIndexable => false;
	}
}