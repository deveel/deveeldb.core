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
	public struct SqlYearToMonth : ISqlValue, IComparable<SqlYearToMonth> {
		private int? months;

		public static readonly SqlYearToMonth Null = new SqlYearToMonth(true);

		public SqlYearToMonth(int months) 
			: this() {
			if (months <= 0)
				throw new ArgumentException("Must be a number greater than 0");

			this.months = months;
		}

		public SqlYearToMonth(int years, int months)
			: this((years*12) + months) {
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
			SqlNumber i;
			if (other is SqlYearToMonth) {
				i = CompareTo((SqlYearToMonth) other);
			} else if (other is SqlNumber) {
				i = CompareTo((SqlNumber) other);
			} else {
				throw new NotSupportedException();
			}

			if (i.IsNull)
				throw new InvalidOperationException();

			return (int) i;
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

				return ((double)months.Value/12);
			}
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			return other is SqlYearToMonth ||
			       other is SqlNumber;
		}

		int IComparable<SqlYearToMonth>.CompareTo(SqlYearToMonth other) {
			var i = CompareTo(other);
			if (i.IsNull)
				throw new InvalidOperationException();

			return (int) i;
		}

		/// <inheritdoc/>
		public SqlNumber CompareTo(SqlYearToMonth other) {
			if (months == null && 
				other.months == null)
				return SqlNumber.Null;
			if (months == null && 
				other.months != null)
				return SqlNumber.MinusOne;
			if (months != null && 
				other.months == null)
				return SqlNumber.One;

			return (SqlNumber) months.Value.CompareTo(other.months.Value);
		}

		public SqlNumber CompareTo(SqlNumber number) {
			if (IsNull && number.IsNull)
				return SqlNumber.Null;

			var other = new SqlYearToMonth((int)number);
			return CompareTo(other);
		}
	}
}