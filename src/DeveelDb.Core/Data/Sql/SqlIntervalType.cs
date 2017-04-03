using System;

namespace Deveel.Data.Sql {
	public sealed class SqlIntervalType : SqlType {
		public SqlIntervalType(SqlTypeCode typeCode)
			: base("INTERVAL", typeCode) {
			AssertIntervalType(typeCode);
		}

		private void AssertIntervalType(SqlTypeCode typeCode) {
			if (!IsIntervalType(typeCode))
				throw new ArgumentException($"The SQL type {typeCode} is not a valid INTERVAL");
		}

		private static bool IsIntervalType(SqlTypeCode typeCode) {
			return typeCode == SqlTypeCode.YearToMonth ||
			       typeCode == SqlTypeCode.DayToSecond;
		}
	}
}