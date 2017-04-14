using System;

namespace Deveel.Data.Sql.Tables {
	public struct FieldId : IEquatable<FieldId> {
		public FieldId(RowId rowId, int column)
			: this() {
			RowId = rowId;
			Column = column;
		}

		public RowId RowId { get; }

		public int Column { get; }

		public override bool Equals(object obj) {
			return base.Equals(obj);
		}

		public bool Equals(FieldId other) {
			return RowId.Equals(other.RowId) &&
			       Column == other.Column;
		}

		public override int GetHashCode() {
			return RowId.GetHashCode() + Column.GetHashCode();
		}

		public override string ToString() {
			return $"{RowId}[{Column}]";
		}
	}
}