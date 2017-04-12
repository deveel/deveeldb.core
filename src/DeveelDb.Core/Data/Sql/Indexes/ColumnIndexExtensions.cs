using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Indexes {
	public static class ColumnIndexExtensions {
		public static IEnumerable<long> SelectRange(this Index index, IndexRange range) {
			return index.SelectRange(new[] { range });
		}

		public static IEnumerable<long> SelectAll(this Index index)
			=> index.SelectRange(IndexRange.FullRange);

		// NOTE: This will find NULL at start which is probably wrong.  The
		//   first value should be the first non null value.
		public static IEnumerable<long> SelectFirst(this Index index)
			=> index.SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, IndexRange.FirstInSet,
				RangeFieldOffset.LastValue, IndexRange.FirstInSet));

		public static IEnumerable<long> SelectLast(this Index index)
			=> index.SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, IndexRange.LastInSet,
				RangeFieldOffset.LastValue, IndexRange.LastInSet));

		public static IEnumerable<long> SelectNotNull(this Index index)
			=> index.SelectRange(new IndexRange(
				RangeFieldOffset.AfterLastValue, SqlObject.Null,
				RangeFieldOffset.LastValue, IndexRange.LastInSet));

		public static IEnumerable<long> SelectEqual(this Index index, SqlObject value) {
			if (value.IsNull)
				return new long[0];

			return index.SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, value,
				RangeFieldOffset.LastValue, value));
		}

		public static IEnumerable<long> SelectNotEqual(this Index index, SqlObject value) {
			if (value.IsNull)
				return new long[0];

			return index.SelectRange(new [] {
				new IndexRange(
					RangeFieldOffset.AfterLastValue, SqlObject.Null,
					RangeFieldOffset.BeforeFirstValue, value),
				new IndexRange(
					RangeFieldOffset.AfterLastValue, value,
					RangeFieldOffset.LastValue, IndexRange.LastInSet)
			});
		}

		public static IEnumerable<long> SelectGreater(this Index index, SqlObject value) {
			if (value.IsNull)
				return new long[0];

			return index.SelectRange(
				new IndexRange(
					RangeFieldOffset.AfterLastValue, value,
					RangeFieldOffset.LastValue, IndexRange.LastInSet));
		}

		public static IEnumerable<long> SelectLess(this Index index, SqlObject value) {
			if (value.IsNull)
				return new long[0];

			return index.SelectRange(new IndexRange(
				RangeFieldOffset.AfterLastValue, SqlObject.Null,
				RangeFieldOffset.BeforeFirstValue, value));
		}

		public static IEnumerable<long> SelectGreaterOrEqual(this Index index, SqlObject value) {
			if (value.IsNull)
				return new long[0];

			return index.SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, value,
				RangeFieldOffset.LastValue, IndexRange.LastInSet));
		}

		public static IEnumerable<long> SelectLessOrEqual(this Index index, SqlObject value) {
			if (value.IsNull)
				return new long[0];

			return index.SelectRange(new IndexRange(
				RangeFieldOffset.AfterLastValue, SqlObject.Null,
				RangeFieldOffset.LastValue, value));
		}

		public static IEnumerable<long> SelectBetween(this Index index, SqlObject ob1, SqlObject ob2) {
			if (ob1.IsNull || ob2.IsNull)
				return new long[0];

			return index.SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, ob1,
				RangeFieldOffset.BeforeFirstValue, ob2));
		}
	}
}