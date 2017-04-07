using System;

namespace Deveel.Data.Sql {
	public sealed class SqlYearToMonthType : SqlType {
		public SqlYearToMonthType()
			: base(SqlTypeCode.YearToMonth) {
		}

		public override bool IsInstanceOf(ISqlValue value) {
			return value is SqlYearToMonth;
		}

		public override ISqlValue Add(ISqlValue a, ISqlValue b) {
			if (a is SqlYearToMonth) {
				var x = (SqlYearToMonth)a;

				if (b is SqlYearToMonth) {
					var y = (SqlYearToMonth)b;

					return x.Add(y);
				}
				if (b is SqlNumber) {
					var y = (SqlNumber)b;
					return x.AddMonths((int)y);
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
				}
				if (b is SqlNumber) {
					var y = (SqlNumber)b;
					return x.AddMonths(-(int)y);
				}
			}

			return base.Add(a, b);
		}
	}
}