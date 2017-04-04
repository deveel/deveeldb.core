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
		private readonly int months;

		public SqlYearToMonth(int months)
			: this() {
			if (months <= 0)
				throw new ArgumentException("Must be a number greater than 0");

			this.months = months;
		}

		public SqlYearToMonth(int years, int months)
			: this((years * 12) + months) {
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

		
		/// <summary>
		/// Gets the total number of months that represents the time span.
		/// </summary>
		public int TotalMonths {
			get {
				return months;
			}
		}

		/// <summary>
		/// Gets the total number of years that represents the time span.
		/// </summary>
		public double TotalYears {
			get {
				return ((double) months / 12);
			}
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			return other is SqlYearToMonth ||
			       other is SqlNumber;
		}

		public bool Equals(SqlYearToMonth other) {
			return months == other.months;
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
			return AddMonths(other.TotalMonths);
		}

		public SqlYearToMonth AddMonths(int value) {
			var result = months + value;
			return new SqlYearToMonth(result);
		}

		public SqlYearToMonth Subtract(SqlYearToMonth other) {
			return AddMonths(-other.TotalMonths);
		}

		/// <inheritdoc/>
		public int CompareTo(SqlYearToMonth other) {
			return months.CompareTo(other.months);
		}

		public int CompareTo(SqlNumber number) {
			var other = new SqlYearToMonth((int) number);
			return CompareTo(other);
		}

		public static SqlYearToMonth operator +(SqlYearToMonth a, SqlYearToMonth b) {
			return a.Add(b);
		}

		public static SqlYearToMonth operator -(SqlYearToMonth a, SqlYearToMonth b) {
			return a.Subtract(b);
		}

		public static bool operator ==(SqlYearToMonth a, SqlYearToMonth b) {
			return a.CompareTo(b) == 0;
		}

		public static bool operator !=(SqlYearToMonth a, SqlYearToMonth b) {
			return a.CompareTo(b) != 0;
		}

		public static bool operator >(SqlYearToMonth a, SqlYearToMonth b) {
			return a.CompareTo(b) > 0;
		}

		public static bool operator <(SqlYearToMonth a, SqlYearToMonth b) {
			return a.CompareTo(b) < 0;
		}

		public static bool operator >=(SqlYearToMonth a, SqlYearToMonth b) {
			return a.CompareTo(b) >= 0;
		}

		public static bool operator <=(SqlYearToMonth a, SqlYearToMonth b) {
			return a.CompareTo(b) <= 0;
		}


		public static explicit operator SqlYearToMonth(int value) {
			return new SqlYearToMonth(value);
		}


		public static explicit operator int(SqlYearToMonth value) {
			return value.months;
		}
	}
}