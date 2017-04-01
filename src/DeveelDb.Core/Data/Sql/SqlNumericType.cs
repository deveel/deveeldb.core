using System;

namespace Deveel.Data.Sql {
	public class SqlNumericType : SqlType {
		public SqlNumericType(SqlTypeCode typeCode, int precision, int scale)
			: base("NUMERIC", typeCode) {
			AssertIsNumeric(typeCode);
			Precision = precision;
			Scale = scale;
		}

		public int Precision { get; }

		public int Scale { get; }

		private static void AssertIsNumeric(SqlTypeCode typeCode) {
			if (!IsNumericType(typeCode))
				throw new ArgumentException(String.Format("The type '{0}' is not a valid NUMERIC type.", typeCode));
		}

		internal static bool IsNumericType(SqlTypeCode typeCode) {
			return typeCode == SqlTypeCode.TinyInt ||
			       typeCode == SqlTypeCode.SmallInt ||
			       typeCode == SqlTypeCode.Integer ||
			       typeCode == SqlTypeCode.BigInt ||
			       typeCode == SqlTypeCode.Real ||
			       typeCode == SqlTypeCode.Float ||
			       typeCode == SqlTypeCode.Double ||
			       typeCode == SqlTypeCode.Decimal ||
			       typeCode == SqlTypeCode.Numeric;
		}
	}
}