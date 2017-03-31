using System;

namespace Deveel.Data.Sql {
	public sealed class SqlBooleanType : SqlType {
		public SqlBooleanType(SqlTypeCode typeCode) 
			: base("BOOLEAN", typeCode) {
			AssertIsBoolean(typeCode);
		}

		private static void AssertIsBoolean(SqlTypeCode sqlType) {
			if (!IsBooleanType(sqlType))
				throw new ArgumentException(String.Format("The SQL type {0} is not BOOLEAN.", sqlType));
		}

		internal static bool IsBooleanType(SqlTypeCode sqlType) {
			return (sqlType == SqlTypeCode.Bit ||
			        sqlType == SqlTypeCode.Boolean);
		}

		public override ISqlValue Reverse(ISqlValue value) {
			return Negate(value);
		}

		public override ISqlValue Negate(ISqlValue value) {
			if (value.IsNull)
				return SqlBoolean.Null;

			var b = (SqlBoolean)value;
			return b.Not();
		}

		public override ISqlValue And(ISqlValue a, ISqlValue b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			var b1 = (SqlBoolean)a;
			var b2 = (SqlBoolean)b;

			return b1.And(b2);
		}

		public override ISqlValue Or(ISqlValue a, ISqlValue b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			var b1 = (SqlBoolean)a;
			var b2 = (SqlBoolean)b;

			return b1.Or(b2);
		}

		public override ISqlValue XOr(ISqlValue a, ISqlValue b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			var b1 = (SqlBoolean)a;
			var b2 = (SqlBoolean)b;

			return b1.XOr(b2);
		}

		public override string ToString(ISqlValue obj) {
			var b = (SqlBoolean)obj;
			if (b.IsNull)
				return "NULL";
			if (b == SqlBoolean.True)
				return "TRUE";
			if (b == SqlBoolean.False)
				return "FALSE";

			return base.ToString(obj);
		}

		public override SqlBoolean Equal(ISqlValue a, ISqlValue b) {
			if (a.IsNull && b.IsNull)
				return true;

			var b1 = (SqlBoolean)a;
			var b2 = (SqlBoolean)b;
			return b1.Equals(b2);
		}

		public override SqlBoolean NotEqual(ISqlValue a, ISqlValue b) {
			if (a.IsNull && b.IsNull)
				return false;

			var b1 = (SqlBoolean)a;
			var b2 = (SqlBoolean)b;
			return !b1.Equals(b2);
		}
	}
}