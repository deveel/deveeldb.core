using System;

using Deveel.Math;

namespace Deveel.Data.Sql {
	public static class SqlMath {
		public static SqlNumber Pow(SqlNumber number, SqlNumber exp) {
			if (number.State == SqlNumber.NumericState.None)
				return new SqlNumber(number.innerValue.Pow(exp.innerValue));

			return number;
		}

		public static SqlNumber Round(SqlNumber number) {
			return Round(number, number.MathContext.Precision);
		}

		public static SqlNumber Round(SqlNumber value, int precision) {
			if (value.State == SqlNumber.NumericState.None)
				return new SqlNumber(value.innerValue.Round(new MathContext(precision, RoundingMode.HalfUp)));

			return value;
		}

		private static SqlNumber DoubleOperation(SqlNumber number, Func<double, double> op) {
			if (number.State != SqlNumber.NumericState.None)
				return number;

			var value = (double) number;
			var result = op(value);

			if (Double.IsNaN(result))
				return SqlNumber.NaN;
			if (Double.IsPositiveInfinity(result))
				return SqlNumber.PositiveInfinity;
			if (Double.IsNegativeInfinity(result))
				return SqlNumber.NegativeInfinity;

			return new SqlNumber(result);
		}

		public static SqlNumber Log(SqlNumber number) {
			return DoubleOperation(number, System.Math.Log);
		}

		public static SqlNumber Log(SqlNumber number, SqlNumber newBase) {
			if (number.State == SqlNumber.NumericState.None)
				return new SqlNumber(System.Math.Log((double)number, (double) newBase));

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
			if (number.State == SqlNumber.NumericState.None)
				return new SqlNumber(SqlNumber.NumericState.None, number.innerValue.Abs());
			if (number.State == SqlNumber.NumericState.NegativeInfinity)
				return new SqlNumber(SqlNumber.NumericState.PositiveInfinity, null);
			return new SqlNumber(number.State, null);
		}
	}
}