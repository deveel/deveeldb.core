// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


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

		public override SqlBoolean Greater(ISqlValue a, ISqlValue b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			return Compare(a, b) > 0;
		}

		public override SqlBoolean GreaterOrEqual(ISqlValue a, ISqlValue b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			return Compare(a, b) >= 0;
		}

		public override SqlBoolean Smaller(ISqlValue a, ISqlValue b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			return Compare(a, b) < 0;
		}

		public override SqlBoolean SmallerOrEqual(ISqlValue a, ISqlValue b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			return Compare(a, b) <= 0;
		}

		public override ISqlValue NormalizeValue(ISqlValue value) {
			if (!(value is SqlNumber))
				throw new ArgumentException();

			var number = (SqlNumber) value;

			switch (TypeCode) {
				case SqlTypeCode.TinyInt:
				case SqlTypeCode.SmallInt:
				case SqlTypeCode.Integer:
				case SqlTypeCode.BigInt:
					return ToInteger(number);
				case SqlTypeCode.Real:
				case SqlTypeCode.Float:
				case SqlTypeCode.Double:
					return ToFloatingPoint(number);
				case SqlTypeCode.Numeric:
					return ToDecimal(number);
			}

			return base.NormalizeValue(value);
		}

		private SqlNumber ToDecimal(SqlNumber number) {
			if (SqlNumber.IsNaN(number))
				return SqlNumber.NaN;
			if (SqlNumber.IsNegativeInfinity(number))
				return SqlNumber.NegativeInfinity;
			if (SqlNumber.IsPositiveInfinity(number))
				return SqlNumber.PositiveInfinity;

			var precision = number.Precision;
			var scale = number.Scale;
			if (Precision > 0)
				precision = Precision;
			if (Scale > 0)
				scale = Scale;

			return new SqlNumber(number.ToUnscaledByteArray(), scale, precision);
		}

		private SqlNumber ToInteger(SqlNumber number) {
			if (!number.CanBeInt32 && !number.CanBeInt64)
				throw new InvalidCastException("Not a valid integer");

			switch (TypeCode) {
				case SqlTypeCode.TinyInt:
					return new SqlNumber((int) (byte) number);
				case SqlTypeCode.SmallInt:
					return new SqlNumber((int) (short) number);
				case SqlTypeCode.Integer:
					return new SqlNumber((int) number);
				case SqlTypeCode.BigInt:
					return new SqlNumber((long) number);
				default:
					throw new InvalidCastException();
			}
		}

		private SqlNumber ToFloatingPoint(SqlNumber number) {
			switch (TypeCode) {
				case SqlTypeCode.Float:
				case SqlTypeCode.Real:
					return new SqlNumber((double) (float) number);
				case SqlTypeCode.Double:
					return new SqlNumber((double) number);
				default:
					throw new InvalidCastException();
			}
		}
	}
}