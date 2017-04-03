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

		public override bool IsInstanceOf(ISqlValue value) {
			return value is SqlYearToMonth || value is SqlDayToSecond;
		}

		public override ISqlValue Add(ISqlValue a, ISqlValue b) {
			if (a is SqlYearToMonth) {
				var x = (SqlYearToMonth) a;

				if (b is SqlYearToMonth) {
					var y = (SqlYearToMonth) b;

					return x.Add(y);
				} else if (b is SqlDayToSecond) {
					var y = (SqlDayToSecond) b;
					// TODO:
					throw new NotImplementedException();
				} else if (b is SqlNumber) {
					var y = (SqlNumber) b;
					return x.AddMonths((int)y);
				}
			} else if (a is SqlDayToSecond) {
				var x = (SqlDayToSecond) a;

				if (b is SqlDayToSecond) {
					var y = (SqlDayToSecond) b;
					return x.Add(y);
				} else if (b is SqlYearToMonth) {
					var y = (SqlYearToMonth) b;
					// TODO:
					throw new NotImplementedException();
				}
			}

			return base.Add(a, b);
		}

		public override ISqlValue Subtract(ISqlValue a, ISqlValue b) {
			if (a is SqlYearToMonth) {
				var x = (SqlYearToMonth)a;

				if (b is SqlYearToMonth) {
					var y = (SqlYearToMonth)b;

					return x.Subtract(y);
				} else if (b is SqlDayToSecond) {
					var y = (SqlDayToSecond)b;
					// TODO:
					throw new NotImplementedException();
				} else if (b is SqlNumber) {
					var y = (SqlNumber)b;
					return x.AddMonths(-(int)y);
				}
			} else if (a is SqlDayToSecond) {
				var x = (SqlDayToSecond)a;

				if (b is SqlDayToSecond) {
					var y = (SqlDayToSecond)b;
					return x.Subtract(y);
				} else if (b is SqlYearToMonth) {
					var y = (SqlYearToMonth)b;
					// TODO:
					throw new NotImplementedException();
				}
			}

			return base.Subtract(a, b);
		}
	}
}