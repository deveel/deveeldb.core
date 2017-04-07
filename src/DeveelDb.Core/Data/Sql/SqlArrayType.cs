using System;

namespace Deveel.Data.Sql {
	public sealed class SqlArrayType : SqlType {
		public SqlArrayType(int length)
			: base(SqlTypeCode.Array) {
			if (length < 0)
				throw new ArgumentException("Invalid array length");

			Length = length;
		}

		public int Length { get; }

		public override bool IsInstanceOf(ISqlValue value) {
			return value is SqlArray && ((SqlArray) value).Length == Length;
		}

		public override bool Equals(SqlType other) {
			if (!(other is SqlArrayType))
				return false;

			var otherType = (SqlArrayType) other;
			return Length == otherType.Length;
		}
	}
}