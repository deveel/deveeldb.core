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

		internal static bool IsDateType(SqlTypeCode sqlType) {
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
			return destType is SqlStringType ||
			       destType is SqlDateTimeType ||
			       destType is SqlNumericType;
		}

		public override ISqlValue Cast(ISqlValue value, SqlType destType) {
			if (!(value is SqlDateTime))
				throw new ArgumentException("DATETIME type cannot cast only from a SQL DATETIME");

			var date = (SqlDateTime) value;

			if (destType is SqlStringType)
				return ToString(date, (SqlStringType) destType);
			if (destType is SqlNumericType)
				return ToNumber(date);
			if (destType is SqlDateTimeType)
				return ToDateTime(date, (SqlDateTimeType) destType);

			return base.Cast(value, destType);
		}

		private SqlDateTime ToDateTime(SqlDateTime date, SqlDateTimeType destType) {
			if (date.IsNull)
				return SqlDateTime.Null;

			switch (destType.TypeCode) {
				case SqlTypeCode.DateTime:
				case SqlTypeCode.TimeStamp:
					return new SqlDateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, date.Offset);
				case SqlTypeCode.Date:
					return date.DatePart;
				case SqlTypeCode.Time:
					return date.TimePart;
				default:
					throw new InvalidCastException();
			}
		}

		private SqlNumber ToNumber(SqlDateTime date) {
			return (SqlNumber) date.Ticks;
		}

		private SqlString ToString(SqlDateTime date, SqlStringType destType) {
			if (date.IsNull)
				return SqlString.Null;

			var s = ToString(date);

			switch (destType.TypeCode) {
				case SqlTypeCode.VarChar:
				case SqlTypeCode.String:
					return VarString(s, destType);
				case SqlTypeCode.Char:
					return CharString(s, destType);
				default:
					throw new InvalidCastException();
			}
		}

		private SqlString VarString(string s, SqlStringType destType) {
			if (destType.HasMaxSize && s.Length >= destType.MaxSize)
				throw new InvalidCastException();

			return new SqlString(s);
		}

		private SqlString CharString(string s, SqlStringType destType) {
			if (s.Length > destType.MaxSize) {
				s = s.Substring(0, destType.MaxSize);
			} else if (s.Length < destType.MaxSize) {
				s = s.PadRight(destType.MaxSize);
			}

			return new SqlString(s);
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