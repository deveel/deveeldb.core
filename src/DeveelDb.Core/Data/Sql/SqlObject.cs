using System;
using System.Globalization;

namespace Deveel.Data.Sql {
	public sealed class SqlObject : IComparable<SqlObject>, IComparable, ISqlFormattable, IEquatable<SqlObject> {
		public SqlObject(SqlType type, ISqlValue value) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (!type.IsInstanceOf(value))
				throw new ArgumentException($"The value given is not an instance of {type}", nameof(value));

			Type = type;
			Value = type.NormalizeValue(value);
		}

		public ISqlValue Value { get; }

		public SqlType Type { get; }

		public bool IsNull => Value.IsNull;

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

			if (Value.IsNull && other.Value.IsNull)
				return true;
			if (Value.IsNull || other.Value.IsNull)
				return false;

			return Value.Equals(other.Value);
		}

		#region Object Factories

		public static SqlObject String(SqlString value) {
			return String(-1, value);
		}

		public static SqlObject String(int maxSize, SqlString value) {
			return String(maxSize, null, value);
		}

		public static SqlObject String(CultureInfo locale, SqlString value) {
			return String(-1, locale, value);
		}

		public static SqlObject String(int maxSize, CultureInfo locale, SqlString value) {
			return String(SqlTypeCode.String, maxSize, locale, value);
		}

		public static SqlObject String(SqlTypeCode typeCode, int maxSize, CultureInfo locale, ISqlString value) {
			return new SqlObject(PrimitiveTypes.String(typeCode, maxSize, locale), value);
		}

		public static SqlObject VarChar(SqlString value) {
			return VarChar(-1, value);
		}

		public static SqlObject VarChar(int maxSize, SqlString value) {
			return VarChar(maxSize, null, value);
		}

		public static SqlObject VarChar(int maxSize, CultureInfo locale, SqlString value) {
			return String(SqlTypeCode.VarChar, maxSize, locale, value);
		}

		#endregion
	}
}