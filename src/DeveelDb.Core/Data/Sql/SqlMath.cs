using System;

using Deveel.Math;

namespace Deveel.Data.Sql {
	public static class SqlMath {
		public static SqlNumber Add(SqlNumber a, SqlNumber b) {
			if (SqlNumber.IsNumber(a)) {
				if (SqlNumber.IsNumber(b)) {
					return new SqlNumber(SqlNumber.NumericState.None, a.innerValue.Add(b.innerValue));
				}

				return b;
			}

			return a;
		}

		public static SqlNumber Subtract(SqlNumber a, SqlNumber b) {
			if (SqlNumber.IsNumber(a)) {
				if (SqlNumber.IsNumber(b)) {
					return new SqlNumber(SqlNumber.NumericState.None, a.innerValue.Subtract(b.innerValue));
				}
				return new SqlNumber(b.InverseState(), null);
			}

			return a;
		}

		public static SqlNumber Divide(SqlNumber a, SqlNumber b) {
			if (SqlNumber.IsNumber(a)) {
				if (SqlNumber.IsNumber(b)) {
					BigDecimal divBy = b.innerValue;
					if (divBy.CompareTo(BigDecimal.Zero) != 0) {
						return new SqlNumber(SqlNumber.NumericState.None, a.innerValue.Divide(divBy, 10, RoundingMode.HalfUp));
					} else {
						throw new DivideByZeroException();
					}
				}
			}

			// Return NaN if we can't divide
			return SqlNumber.NaN;
		}

		public static SqlNumber Multiply(SqlNumber a, SqlNumber b) {
			if (SqlNumber.IsNumber(a)) {
				if (SqlNumber.IsNumber(b)) {
					return new SqlNumber(SqlNumber.NumericState.None, a.innerValue.Multiply(b.innerValue));
				}

				return b;
			}

			return a;
		}

		public static SqlNumber Modulo(SqlNumber a, SqlNumber b) {
			if (SqlNumber.IsNumber(a)) {
				if (SqlNumber.IsNumber(b)) {
					BigDecimal divBy = b.innerValue;
					if (divBy.CompareTo(BigDecimal.Zero) != 0) {
						BigDecimal remainder = a.innerValue.Remainder(divBy);
						return new SqlNumber(SqlNumber.NumericState.None, remainder);
					}
				}
			}

			return SqlNumber.NaN;
		}

		public static SqlNumber Pow(SqlNumber number, SqlNumber exp) {
			if (SqlNumber.IsNumber(number))
				return new SqlNumber(number.innerValue.Pow(exp.innerValue));

			return number;
		}

		public static SqlNumber Round(SqlNumber number) {
			return Round(number, number.MathContext.Precision);
		}

		public static SqlNumber Round(SqlNumber value, int precision) {
			if (SqlNumber.IsNumber(value))
				return new SqlNumber(value.innerValue.Round(new MathContext(precision, RoundingMode.HalfUp)));

			return value;
		}

		private static SqlNumber DoubleOperation(SqlNumber number, Func<double, double> op) {
			if (!SqlNumber.IsNumber(number))
				return number;

			var value = (double) number;
			var result = op(value);

			if (Double.IsNaN(result))
				return SqlNumber.NaN;
			if (Double.IsPositiveInfinity(result))
				return SqlNumber.PositiveInfinity;
			if (Double.IsNegativeInfinity(result))
				return SqlNumber.NegativeInfinity;

			return (SqlNumber)result;
		}

		public static SqlNumber Log(SqlNumber number) {
			return DoubleOperation(number, System.Math.Log);
		}

		public static SqlNumber Log(SqlNumber number, SqlNumber newBase) {
			if (SqlNumber.IsNumber(number))
				return (SqlNumber)System.Math.Log((double)number, (double) newBase);

			return number;
		}

		public static SqlNumber Cos(SqlNumber number) {
			return DoubleOperation(number, System.Math.Cos);
		}

		public static SqlNumber CosH(SqlNumber number) {
			return DoubleOperation(number, System.Math.Cosh);
		}

		public static SqlNumber Tan(SqlNumber number) {
			return DoubleOperation(number, System.Math.Tan);
		}

		public static SqlNumber TanH(SqlNumber number) {
			return DoubleOperation(number, System.Math.Tanh);
		}

		public static SqlNumber Sin(SqlNumber number) {
			return DoubleOperation(number, System.Math.Sin);
		}

		public static SqlNumber Abs(SqlNumber number) {
			if (SqlNumber.IsNumber(number))
				return new SqlNumber(SqlNumber.NumericState.None, number.innerValue.Abs());
			if (SqlNumber.IsNegativeInfinity(number))
				return SqlNumber.PositiveInfinity;

			return number;
		}
	}
}