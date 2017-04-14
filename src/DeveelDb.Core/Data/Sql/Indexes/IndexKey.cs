using System;
using System.Linq;
using System.Text;

namespace Deveel.Data.Sql.Indexes {
	public sealed class IndexKey : IComparable<IndexKey>, IEquatable<IndexKey> {
		private readonly SqlObject[] values;

		public IndexKey(SqlObject[] values) {
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (values.Length == 0)
				throw new ArgumentException();

			if (values.Any(x => !x.Type.IsIndexable))
				throw new ArgumentException();

			this.values = values;
		}

		public IndexKey(SqlObject value)
			: this(new[] {value}) {
		}

		public bool IsNull => values.Any(x => x.IsNull);

		public IndexKey NullKey => new IndexKey(values.Select(x => SqlObject.Null).ToArray());

		public int CompareTo(IndexKey other) {
			int c = 0;
			for (int i = 0; i < values.Length; i++) {
				c = (c* i) + values[i].CompareTo(other.values[i]);
			}

			return c;
		}

		public bool Equals(IndexKey other) {
			if (values.Length != other.values.Length)
				return false;

			for (int i = 0; i < values.Length; i++) {
				if (!values[i].Equals(other.values[i]))
					return false;
			}

			return true;
		}

		public override string ToString() {
			var sb = new StringBuilder();
			sb.Append("[");
			for (int i = 0; i < values.Length; i++) {
				sb.Append(values[i]);

				if (i < values.Length - 1)
					sb.Append(",");
			}

			sb.Append("]");
			return sb.ToString();
		}
	}
}