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