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
using System.Runtime.InteropServices;

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

		public override bool IsInstanceOf(ISqlValue value) {
			if (value is SqlNumber) {
				var number = (SqlNumber) value;
				switch (TypeCode) {
					case SqlTypeCode.Integer:
					case SqlTypeCode.TinyInt:
					case SqlTypeCode.SmallInt:
						return number.CanBeInt32;
					case SqlTypeCode.BigInt:
						return number.CanBeInt64;
					case SqlTypeCode.Double:
					case SqlTypeCode.Float:
						return Precision == number.Precision;
					default: {
						if (Precision > 0 && number.Precision != Precision)
							return false;
						if (Scale > 0 && number.Scale != Scale)
							return false;
					}

						return true;
				}
			}

			return value is SqlNull;
		}

		public override SqlBoolean Greater(ISqlValue a, ISqlValue b) {
			return Compare(a, b) > 0;
		}

		public override SqlBoolean GreaterOrEqual(ISqlValue a, ISqlValue b) {
			return Compare(a, b) >= 0;
		}

		public override SqlBoolean Less(ISqlValue a, ISqlValue b) {
			return Compare(a, b) < 0;
		}

		public override SqlBoolean LessOrEqual(ISqlValue a, ISqlValue b) {
			return Compare(a, b) <= 0;
		}

		public override bool IsComparable(SqlType type) {
			return type is SqlNumericType;
		}

		private static int GetIntSize(SqlTypeCode sqlType) {
			switch (sqlType) {
				case SqlTypeCode.TinyInt:
					return 1;
				case SqlTypeCode.SmallInt:
					return 2;
				case SqlTypeCode.Integer:
					return 4;
				case SqlTypeCode.BigInt:
					return 8;
				default:
					return 0;
			}
		}


		private static int GetFloatSize(SqlTypeCode sqlType) {
			switch (sqlType) {
				default:
					return 0;
				case SqlTypeCode.Real:
					return 4;
				case SqlTypeCode.Float:
				case SqlTypeCode.Double:
					return 8;
			}
		}

		public override SqlType Wider(SqlType otherType) {
			var t1SqlType = TypeCode;
			var t2SqlType = otherType.TypeCode;
			if (t1SqlType == SqlTypeCode.Decimal) {
				return this;
			}
			if (t2SqlType == SqlTypeCode.Decimal) {
				return otherType;
			}
			if (t1SqlType == SqlTypeCode.Numeric) {
				return this;
			}
			if (t2SqlType == SqlTypeCode.Numeric) {
				return otherType;
			}

			if (t1SqlType == SqlTypeCode.Bit) {
				return otherType; // It can't be any smaller than a Bit
			}
			if (t2SqlType == SqlTypeCode.Bit) {
				return this;
			}

			int t1IntSize = GetIntSize(t1SqlType);
			int t2IntSize = GetIntSize(t2SqlType);
			if (t1IntSize > 0 && t2IntSize > 0) {
				// Both are int types, use the largest size
				return (t1IntSize > t2IntSize) ? this : otherType;
			}

			int t1FloatSize = GetFloatSize(t1SqlType);
			int t2FloatSize = GetFloatSize(t2SqlType);
			if (t1FloatSize > 0 && t2FloatSize > 0) {
				// Both are floating types, use the largest size
				return (t1FloatSize > t2FloatSize) ? this : otherType;
			}

			if (t1FloatSize > t2IntSize) {
				return this;
			}
			if (t2FloatSize > t1IntSize) {
				return otherType;
			}
			if (t1IntSize >= t2FloatSize || t2IntSize >= t1FloatSize) {
				// Must be a long (8 bytes) and a real (4 bytes), widen to a double
				return new SqlNumericType(SqlTypeCode.Double, 8, 0);
			}

			// NOTREACHED - can't get here, the last three if statements cover
			// all possibilities.
			throw new InvalidOperationException("Widest type error.");
		}

		public override ISqlValue NormalizeValue(ISqlValue value) {
			if (value is SqlNull)
				return value;

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
					return (SqlNumber)(byte) number;
				case SqlTypeCode.SmallInt:
					return (SqlNumber)(short) number;
				case SqlTypeCode.Integer:
					return (SqlNumber)(int) number;
				case SqlTypeCode.BigInt:
					return (SqlNumber) (long) number;
				default:
					throw new InvalidCastException();
			}
		}

		private SqlNumber ToFloatingPoint(SqlNumber number) {
			switch (TypeCode) {
				case SqlTypeCode.Float:
				case SqlTypeCode.Real:
					return (SqlNumber) ((float) number);
				case SqlTypeCode.Double:
					return SqlNumber.FromDouble((double) number, Precision);
				default:
					throw new InvalidCastException();
			}
		}

		public override ISqlValue UnaryPlus(ISqlValue value) {
			if (!(value is SqlNumber))
				return SqlNull.Value;

			return +(SqlNumber) value;
		}

		public override ISqlValue Negate(ISqlValue value) {
			if (!(value is SqlNumber))
				return SqlNull.Value;

			return ~(SqlNumber) value;
		}

		public override ISqlValue Add(ISqlValue a, ISqlValue b) {
			if (!(a is SqlNumber) ||
				!(b is SqlNumber))
				return SqlNull.Value;

			var x = (SqlNumber) a;
			var y = (SqlNumber) b;

			return x + y;
		}

		public override ISqlValue Subtract(ISqlValue a, ISqlValue b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				return SqlNull.Value;

			var x = (SqlNumber)a;
			var y = (SqlNumber)b;

			return x - y;
		}

		public override ISqlValue Multiply(ISqlValue a, ISqlValue b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				return SqlNull.Value;

			var x = (SqlNumber)a;
			var y = (SqlNumber)b;

			return x * y;
		}

		public override ISqlValue Divide(ISqlValue a, ISqlValue b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				return SqlNull.Value;

			var x = (SqlNumber)a;
			var y = (SqlNumber)b;

			return x / y;
		}

		public override ISqlValue Modulo(ISqlValue a, ISqlValue b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				return SqlNull.Value;

			var x = (SqlNumber)a;
			var y = (SqlNumber)b;

			return x % y;
		}

		public override ISqlValue XOr(ISqlValue a, ISqlValue b) {
			if (!(a is SqlNumber) ||
			    !(b is SqlNumber))
				return SqlNull.Value;

			var x = (SqlNumber)a;
			var y = (SqlNumber)b;

			return x ^ y;
		}
	}
}