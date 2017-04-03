// 
//  Copyright 2010-2016 Deveel
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
	/// <summary>
	/// A month span representation of time.
	/// </summary>
	public struct SqlYearToMonth : ISqlValue, IComparable<SqlYearToMonth>, IEquatable<SqlYearToMonth> {
		private int? months;

		public static readonly SqlYearToMonth Null = new SqlYearToMonth(true);

		public SqlYearToMonth(int months)
			: this() {
			if (months <= 0)
				throw new ArgumentException("Must be a number greater than 0");

			this.months = months;
		}

		public SqlYearToMonth(int years, int months)
			: this((years * 12) + months) {
		}

		private SqlYearToMonth(bool isNull)
			: this() {
			if (isNull)
				months = null;
		}

		int IComparable.CompareTo(object obj) {
			if (!(obj is ISqlValue))
				throw new ArgumentException();

			return (this as IComparable<ISqlValue>).CompareTo((ISqlValue) obj);
		}

		int IComparable<ISqlValue>.CompareTo(ISqlValue other) {
			int i;
			if (other is SqlYearToMonth) {
				i = CompareTo((SqlYearToMonth) other);
			} else if (other is SqlNumber) {
				i = CompareTo((SqlNumber) other);
			} else {
				throw new NotSupportedException();
			}

			return i;
		}

		/// <inheritdoc/>
		public bool IsNull {
			get { return months == null; }
		}

		/// <summary>
		/// Gets the total number of months that represents the time span.
		/// </summary>
		public int TotalMonths {
			get {
				if (months == null)
					throw new NullReferenceException();

				return months.Value;
			}
		}

		/// <summary>
		/// Gets the total number of years that represents the time span.
		/// </summary>
		public double TotalYears {
			get {
				if (months == null)
					throw new NullReferenceException();

				return ((double) months.Value / 12);
			}
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			return other is SqlYearToMonth ||
			       other is SqlNumber;
		}

		public bool Equals(SqlYearToMonth other) {
			if (IsNull && other.IsNull)
				return true;
			if (IsNull && !other.IsNull)
				return false;
			if (!IsNull && other.IsNull)
				return false;

			return months.Value == other.months.Value;
		}

		public override bool Equals(object obj) {
			if (!(obj is SqlYearToMonth))
				return false;

			return Equals((SqlYearToMonth)obj);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public SqlYearToMonth Add(SqlYearToMonth other) {
			if (IsNull || other.IsNull)
				return Null;

			return AddMonths(other.TotalMonths);
		}

		public SqlYearToMonth AddMonths(int value) {
			if (months == null)
				return Null;

			var result = months.Value + value;
			if (result <= 0)
				return Null;

			return new SqlYearToMonth(result);
		}

		public SqlYearToMonth Subtract(SqlYearToMonth other) {
			if (IsNull || other.IsNull)
				return Null;

			return AddMonths(-other.TotalMonths);
		}

		/// <inheritdoc/>
		public int CompareTo(SqlYearToMonth other) {
			if (IsNull && other.IsNull)
				return 0;
			if (!IsNull && other.IsNull)
				return 1;
			if (IsNull && !other.IsNull)
				return -1;

			return months.Value.CompareTo(other.months.Value);
		}

		public int CompareTo(SqlNumber number) {
			if (IsNull && number.IsNull)
				return 0;
			if (!IsNull && number.IsNull)
				return 1;
			if (IsNull && !number.IsNull)
				return -1;

			var other = new SqlYearToMonth((int) number);
			return CompareTo(other);
		}

		public static SqlYearToMonth operator +(SqlYearToMonth a, SqlYearToMonth b) {
			return a.Add(b);
		}

		public static SqlYearToMonth operator -(SqlYearToMonth a, SqlYearToMonth b) {
			return a.Subtract(b);
		}

		public static SqlBoolean operator ==(SqlYearToMonth a, SqlYearToMonth b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			return a.CompareTo(b) == 0;
		}

		public static SqlBoolean operator !=(SqlYearToMonth a, SqlYearToMonth b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			return a.CompareTo(b) != 0;
		}

		public static SqlBoolean operator >(SqlYearToMonth a, SqlYearToMonth b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			return a.CompareTo(b) > 0;
		}

		public static SqlBoolean operator <(SqlYearToMonth a, SqlYearToMonth b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			return a.CompareTo(b) < 0;
		}

		public static SqlBoolean operator >=(SqlYearToMonth a, SqlYearToMonth b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			return a.CompareTo(b) >= 0;
		}

		public static SqlBoolean operator <=(SqlYearToMonth a, SqlYearToMonth b) {
			if (a.IsNull || b.IsNull)
				return SqlBoolean.Null;

			return a.CompareTo(b) <= 0;
		}


		public static explicit operator SqlYearToMonth(int value) {
			return new SqlYearToMonth(value);
		}

		public static explicit operator SqlYearToMonth(int? value) {
			return value == null ? Null : new SqlYearToMonth(value.Value);
		}

		public static explicit operator int?(SqlYearToMonth value) {
			return value.months;
		}

		public static explicit operator int(SqlYearToMonth value) {
			if (value.months == null)
				throw new InvalidCastException();

			return value.months.Value;
		}
	}
}