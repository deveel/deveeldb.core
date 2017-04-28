using System;

namespace Deveel.Data.Sql {
	public sealed class QueryLimit : ISqlFormattable {
		public QueryLimit(long total) 
			: this(0, total) {
		}

		public QueryLimit(long offset, long total) {
			if (total < 0)
				throw new ArgumentException("Invalid total", nameof(total));

			Offset = offset;
			Total = total;
		}

		public long Offset { get; }

		public long Total { get; }

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			builder.Append("LIMIT ");
			if (Offset >= 0) {
				builder.AppendFormat("{0},", Offset);
			}

			builder.Append(Total);
		}

		public override string ToString() {
			return this.ToSqlString();
		}
	}
}