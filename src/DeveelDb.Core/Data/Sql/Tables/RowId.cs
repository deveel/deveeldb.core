using System;

namespace Deveel.Data.Sql.Tables {
	public struct RowId {
		public RowId(int tableId, long number)
			: this() {
			TableId = tableId;
			Number = number;
		}

		public int TableId { get; }

		public long Number { get; }

		public override bool Equals(object obj) {
			if (!(obj is RowId))
				return false;

			var other = (RowId) obj;
			return Number == other.Number &&
			       TableId == other.TableId;
		}

		public override int GetHashCode() {
			return Number.GetHashCode() + TableId.GetHashCode();
		}

		public override string ToString() {
			return $"{TableId}:{Number}";
		}
	}
}