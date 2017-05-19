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
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Indexes {
	public static class IndexExtensions {
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

		//public static IEnumerable<long> SelectNotNull(this Index index)
		//	=> index.SelectRange(new IndexRange(
		//		RangeFieldOffset.AfterLastValue, SqlObject.Null,
		//		RangeFieldOffset.LastValue, IndexRange.LastInSet));

		public static IEnumerable<long> SelectEqual(this Index index, SqlObject[] values) {
			if (values.Any(x => x.IsNull))
				return new long[0];

			return index.SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, new IndexKey(values),
				RangeFieldOffset.LastValue, new IndexKey(values)));
		}

		public static IEnumerable<long> SelectNotEqual(this Index index, SqlObject[] values) {
			if (values.Any(x => x.IsNull))
				return new long[0];

			var nullValues = new IndexKey(values.Select(x => SqlObject.Null).ToArray());
			return index.SelectRange(new [] {
				new IndexRange(
					RangeFieldOffset.AfterLastValue, nullValues,
					RangeFieldOffset.BeforeFirstValue, new IndexKey(values)),
				new IndexRange(
					RangeFieldOffset.AfterLastValue, new IndexKey(values),
					RangeFieldOffset.LastValue, IndexRange.LastInSet)
			});
		}

		public static IEnumerable<long> SelectGreater(this Index index, SqlObject[] values) {
			if (values.Any(x => x.IsNull))
				return new long[0];

			return index.SelectRange(
				new IndexRange(
					RangeFieldOffset.AfterLastValue, new IndexKey(values),
					RangeFieldOffset.LastValue, IndexRange.LastInSet));
		}

		public static IEnumerable<long> SelectLess(this Index index, SqlObject[] values) {
			if (values.Any(x => x.IsNull))
				return new long[0];

			var nullValues = new IndexKey(values.Select(x => SqlObject.Null).ToArray());
			return index.SelectRange(new IndexRange(
				RangeFieldOffset.AfterLastValue, nullValues,
				RangeFieldOffset.BeforeFirstValue, new IndexKey(values)));
		}

		public static IEnumerable<long> SelectGreaterOrEqual(this Index index, SqlObject[] values) {
			if (values.Any(x => x.IsNull))
				return new long[0];

			return index.SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, new IndexKey(values),
				RangeFieldOffset.LastValue, IndexRange.LastInSet));
		}

		public static IEnumerable<long> SelectLessOrEqual(this Index index, SqlObject[] values) {
			if (values.Any(x => x.IsNull))
				return new long[0];

			var nullValues = new IndexKey(values.Select(x => SqlObject.Null).ToArray());
			return index.SelectRange(new IndexRange(
				RangeFieldOffset.AfterLastValue, nullValues,
				RangeFieldOffset.LastValue, new IndexKey(values)));
		}

		public static IEnumerable<long> SelectBetween(this Index index, SqlObject[] ob1, SqlObject[] ob2) {
			if (ob1.Any(x => x.IsNull) ||
			    ob2.Any(x => x.IsNull))
				return new long[0];

			return index.SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, new IndexKey(ob1), 
				RangeFieldOffset.BeforeFirstValue, new IndexKey(ob2)));
		}
	}
}