using System;

namespace Deveel.Data.Sql {
	public sealed class SqlDateTimeType : SqlType {
		public SqlDateTimeType(SqlTypeCode sqlType)
			: base("DATETIME", sqlType) {
			AssertDateType(sqlType);
		}

		private static void AssertDateType(SqlTypeCode sqlType) {
			if (!IsDateType(sqlType))
				throw new ArgumentException($"The SQL type {sqlType} is not a valid DATETIME", nameof(sqlType));
		}

		private static bool IsDateType(SqlTypeCode sqlType) {
			return sqlType == SqlTypeCode.Date ||
			       sqlType == SqlTypeCode.Time ||
			       sqlType == SqlTypeCode.TimeStamp ||
			       sqlType == SqlTypeCode.DateTime;
		}

		public override ISqlValue Add(ISqlValue a, ISqlValue b) {
			if (!(a is SqlDateTime))
				throw new ArgumentException();

			if (a.IsNull || b.IsNull)
				return SqlDateTime.Null;

			var date = (SqlDateTime) a;
			if (b is SqlYearToMonth) {
				var ytm = (SqlYearToMonth) b;
				return date.Add(ytm);
			}

			if (b is SqlDayToSecond) {
				var dts = (SqlDayToSecond) b;
				return date.Add(dts);
			}

			return base.Add(a, b);
		}

		public override ISqlValue Subtract(ISqlValue a, ISqlValue b) {
			if (!(a is SqlDateTime))
				throw new ArgumentException();

			if (a.IsNull || b.IsNull)
				return SqlDateTime.Null;

			var date = (SqlDateTime)a;
			if (b is SqlYearToMonth) {
				var ytm = (SqlYearToMonth)b;
				return date.Subtract(ytm);
			}

			if (b is SqlDayToSecond) {
				var dts = (SqlDayToSecond)b;
				return date.Subtract(dts);
			}

			return base.Subtract(a, b);
		}

		public override bool CanCastTo(SqlType destType) {
			return destType is SqlCharacterType ||
			       destType is SqlDateTimeType ||
			       destType is SqlNumericType;
		}

		public override ISqlValue Cast(ISqlValue value, SqlType destType) {
			if (!(value is SqlDateTime))
				throw new ArgumentException("DATETIME type cannot cast only from a SQL DATETIME");

			var date = (SqlDateTime) value;

			if (destType is SqlCharacterType)
				return ToString(date, (SqlCharacterType) destType);
			if (destType is SqlNumericType)
				return ToNumber(date);
			if (destType is SqlDateTimeType)
				return ToDateTime(date, (SqlDateTimeType) destType);

			return base.Cast(value, destType);
		}

		private ISqlValue ToDateTime(SqlDateTime date, SqlDateTimeType destType) {
			return destType.NormalizeValue(date);
		}

		private SqlNumber ToNumber(SqlDateTime date) {
			return (SqlNumber) date.Ticks;
		}

		private ISqlString ToString(SqlDateTime date, SqlCharacterType destType) {
			if (date.IsNull)
				return SqlString.Null;

			var dateString = ToString(date);
			var s = new SqlString(dateString);

			return (ISqlString) destType.NormalizeValue(s);
		}

		public override ISqlValue NormalizeValue(ISqlValue value) {
			if (!(value is SqlDateTime))
				throw new ArgumentException();

			var date = (SqlDateTime) value;
			if (date.IsNull)
				return SqlDateTime.Null;

			switch (TypeCode) {
				case SqlTypeCode.Time:
					return date.TimePart;
				case SqlTypeCode.Date:
					return date.DatePart;
			}

			return base.NormalizeValue(value);
		}

		public override bool IsInstanceOf(ISqlValue value) {
			return value is SqlDateTime;
		}

		public override string ToString(ISqlValue obj) {
			if (!(obj is SqlDateTime))
				throw new ArgumentException();

			var date = (SqlDateTime) obj;
			switch (TypeCode) {
				case SqlTypeCode.Time:
					return date.ToTimeString();
				case SqlTypeCode.Date:
					return date.ToDateString();
				default:
					return date.ToString();
			}
		}
	}
}