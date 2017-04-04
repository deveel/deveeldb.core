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
	public sealed class SqlBooleanType : SqlType {
		public SqlBooleanType(SqlTypeCode typeCode) 
			: base("BOOLEAN", typeCode) {
			AssertIsBoolean(typeCode);
		}

		private static void AssertIsBoolean(SqlTypeCode sqlType) {
			if (!IsBooleanType(sqlType))
				throw new ArgumentException(String.Format("The SQL type {0} is not BOOLEAN.", sqlType));
		}

		private static bool IsBooleanType(SqlTypeCode sqlType) {
			return (sqlType == SqlTypeCode.Bit ||
			        sqlType == SqlTypeCode.Boolean);
		}

		public override int Compare(ISqlValue x, ISqlValue y) {
			if (!(x is SqlBoolean))
				throw new ArgumentException("Arguments of a boolean comparison must be boolean", "x");
			if (!(y is SqlBoolean))
				throw new ArgumentException("Arguments of a boolean comparison must be boolean", "y");

			var a = (SqlBoolean)x;
			var b = (SqlBoolean)y;

			return a.CompareTo(b);
		}

		public override bool IsComparable(SqlType type) {
			return type is SqlBooleanType;
		}

		public override bool IsInstanceOf(ISqlValue value) {
			return value is SqlBoolean || value is SqlNull;
		}

		public override bool CanCastTo(SqlType destType) {
			return destType is SqlNumericType ||
			       destType is SqlCharacterType ||
				   destType is SqlBinaryType;
		}

		public override ISqlValue Cast(ISqlValue value, SqlType destType) {
			if (!(value is SqlBoolean))
				throw new ArgumentException();

			var b = (SqlBoolean) value;

			if (destType is SqlNumericType)
				return ToNumber(b);

			if (destType is SqlBinaryType)
				return ToBinary(b);

			if (destType is SqlCharacterType)
				return ToString(b, (SqlCharacterType) destType);

			return base.Cast(value, destType);
		}

		private SqlNumber ToNumber(SqlBoolean value) {
			return value ? SqlNumber.One : SqlNumber.Zero;
		}

		private SqlBinary ToBinary(SqlBoolean value) {
			var bytes = new[] { value ? (byte)1 : (byte)0 };
			return new SqlBinary(bytes);
		}

		private ISqlString ToString(SqlBoolean value, SqlCharacterType destType) {
			var s = new SqlString(ToString(value));
			return (ISqlString) destType.NormalizeValue(s);
		}

		public override ISqlValue Reverse(ISqlValue value) {
			return Negate(value);
		}

		public override ISqlValue Negate(ISqlValue value) {
			var b = (SqlBoolean)value;
			return !b;
		}

		public override ISqlValue And(ISqlValue a, ISqlValue b) {
			var b1 = (SqlBoolean)a;
			var b2 = (SqlBoolean)b;

			return b1 & b2;
		}

		public override ISqlValue Or(ISqlValue a, ISqlValue b) {
			var b1 = (SqlBoolean)a;
			var b2 = (SqlBoolean)b;

			return b1 | b2;
		}

		public override ISqlValue XOr(ISqlValue a, ISqlValue b) {
			var b1 = (SqlBoolean)a;
			var b2 = (SqlBoolean)b;

			return b1 ^ b2;
		}

		public override string ToString(ISqlValue obj) {
			var b = (SqlBoolean)obj;
			if (TypeCode == SqlTypeCode.Bit) {
				if (b == SqlBoolean.True)
					return "1";
				if (b == SqlBoolean.False)
					return "0";
			} else {
				if (b == SqlBoolean.True)
					return "TRUE";
				if (b == SqlBoolean.False)
					return "FALSE";
			}

			return base.ToString(obj);
		}

		public override SqlBoolean Equal(ISqlValue a, ISqlValue b) {
			var b1 = (SqlBoolean)a;
			var b2 = (SqlBoolean)b;
			return b1.Equals(b2);
		}

		public override SqlBoolean NotEqual(ISqlValue a, ISqlValue b) {
			var b1 = (SqlBoolean)a;
			var b2 = (SqlBoolean)b;
			return !b1.Equals(b2);
		}

		public override bool Equals(SqlType other) {
			return other is SqlBooleanType;
		}
	}
}