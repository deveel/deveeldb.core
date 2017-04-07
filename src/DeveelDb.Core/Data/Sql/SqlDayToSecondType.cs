using System;

namespace Deveel.Data.Sql {
	public sealed class SqlDayToSecondType : SqlType {
		public SqlDayToSecondType()
			: base(SqlTypeCode.DayToSecond) {
		}

		public override ISqlValue Add(ISqlValue a, ISqlValue b) {
			if (a is SqlDayToSecond) {
				var x = (SqlDayToSecond)a;

				if (b is SqlDayToSecond) {
					var y = (SqlDayToSecond)b;
					return x.Add(y);
				}
			}

			return base.Add(a, b);
		}

		public override ISqlValue Subtract(ISqlValue a, ISqlValue b) {
			if (a is SqlDayToSecond) {
				var x = (SqlDayToSecond)a;

				if (b is SqlDayToSecond) {
					var y = (SqlDayToSecond)b;
					return x.Subtract(y);
				}
			}

			return base.Subtract(a, b);
		}
	}
}