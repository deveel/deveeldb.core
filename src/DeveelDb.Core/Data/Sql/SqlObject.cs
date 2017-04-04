using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace Deveel.Data.Sql {
	public sealed class SqlObject : IComparable<SqlObject>, IComparable, ISqlFormattable, IEquatable<SqlObject> {
		public SqlObject(SqlType type, ISqlValue value) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (value == null)
				value = SqlNull.Value;

			if (!type.IsInstanceOf(value))
				throw new ArgumentException($"The value given is not an instance of {type}", nameof(value));

			Type = type;
			Value = type.NormalizeValue(value);
		}

		public ISqlValue Value { get; }

		public SqlType Type { get; }

		public bool IsNull => SqlNull.Value == Value;

		private int CompareToNotNull(SqlObject other) {
			var type = Type;
			// Strings must be handled as a special case.
			if (type is SqlCharacterType) {
				// We must determine the locale to compare against and use that.
				var stype = (SqlCharacterType)type;
				// If there is no locale defined for this type we use the locale in the
				// given type.
				if (stype.Locale == null) {
					type = other.Type;
				}
			}
			return type.Compare(Value, other.Value);

		}

		int IComparable.CompareTo(object obj) {
			if (!(obj is SqlObject))
				throw new ArgumentException();

			return CompareTo((SqlObject) obj);
		}

		public int CompareTo(SqlObject obj) {
			// If this is null
			if (IsNull) {
				// and value is null return 0 return less
				if (obj.IsNull)
					return 0;

				return -1;
			}
			// If this is not null and value is null return +1
			if (ReferenceEquals(null, obj) ||
			    obj.IsNull)
				return 1;

			// otherwise both are non null so compare normally.
			return CompareToNotNull(obj);
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			builder.Append(Type.ToString(Value));
		}

		public override string ToString() {
			return this.ToSqlString();
		}

		public override int GetHashCode() {
			unchecked {
				var code = Type.GetHashCode() * 23;
				code = code ^ Value.GetHashCode();
				return code;
			}
		}

		public override bool Equals(object obj) {
			if (!(obj is SqlObject))
				return false;

			var other = (SqlObject) obj;
			return Equals(other);
		}

		public bool Equals(SqlObject other) {
			if (ReferenceEquals(other, null))
				return false;

			if (!Type.Equals(other.Type))
				return false;

			if (SqlNull.Value == Value &&
			    SqlNull.Value == other.Value)
				return true;
			if (SqlNull.Value == Value ||
			    SqlNull.Value == other.Value)
				return false;

			return Value.Equals(other.Value);
		}

		#region Operators

		private SqlObject BinaryOperator(Func<SqlType, Func<ISqlValue, ISqlValue, SqlBoolean>> selector, SqlObject other) {
			if (IsNull || (other == null || other.IsNull))
				return new SqlObject(PrimitiveTypes.Boolean(), SqlNull.Value);

			if (!Type.IsComparable(other.Type))
				throw new InvalidOperationException();	// TODO: should instead return null?

			var op = selector(Type);
			var result = op(Value, other.Value);

			return new SqlObject(PrimitiveTypes.Boolean(), result);
		}

		public SqlObject Equal(SqlObject other) {
			return BinaryOperator(type => type.Equal, other);
		}

		public SqlObject NotEqual(SqlObject other) {
			return BinaryOperator(type => type.NotEqual, other);
		}

		public SqlObject GreaterThan(SqlObject other) {
			return BinaryOperator(type => type.Greater, other);
		}

		#endregion

		#region Factories

		public static SqlObject New(ISqlValue value) {
			if (value is SqlNull)
				return new SqlObject(PrimitiveTypes.Null(), value);

			if (value is SqlNumber) {
				var number = (SqlNumber) value;
				if (number.CanBeInt32)
					return new SqlObject(PrimitiveTypes.Integer(), value);
				if (number.CanBeInt64)
					return new SqlObject(PrimitiveTypes.BigInt(), value);
				
				return new SqlObject(PrimitiveTypes.Numeric(number.Precision, number.Scale), value);
			}

			if (value is ISqlString) {
				// TODO: support the long string
				var length = ((ISqlString) value).Length;
				return new SqlObject(PrimitiveTypes.VarChar((int)length), value);
			}

			if (value is SqlBinary) {
				var bin = (SqlBinary) value;
				return new SqlObject(PrimitiveTypes.VarBinary((int)bin.Length), value);
			}

			if (value is SqlDateTime) {
				return new SqlObject(PrimitiveTypes.TimeStamp(), value);
			}

			if (value is SqlBoolean)
				return new SqlObject(PrimitiveTypes.Boolean(), value);

			if (value is SqlYearToMonth)
				return new SqlObject(PrimitiveTypes.YearToMonth(), value);
			if (value is SqlDayToSecond)
				return new SqlObject(PrimitiveTypes.DayToSecond(), value);

			throw new NotSupportedException();
		}

		#region Boolean Objects

		public static SqlObject Boolean(SqlBoolean? value) {
			return new SqlObject(PrimitiveTypes.Boolean(), value);
		}

		public static SqlObject Bit(SqlBoolean? value)
			=> new SqlObject(PrimitiveTypes.Bit(), value);

		#endregion

		#region String Objects

		public static SqlObject String(SqlString value) {
			return new SqlObject(PrimitiveTypes.String(), value);
		}

		#endregion

		#endregion
	}
}