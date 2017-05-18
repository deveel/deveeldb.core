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
using System.Text;
using System.Threading.Tasks;

using Deveel.Data.Indexes;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Indexes;
using Deveel.Data.Sql.Tables;
using Deveel.Data.Text;

namespace Deveel.Data.Sql {
	public sealed class SqlDefaultStringSearch : ISqlStringSearch {
		public Task<ITable> SearchLikeAsync(ITable source, int column, string pattern, char escapeChar) {
			var rows = Search(source, column, pattern, escapeChar);
			var result = new VirtualTable(source, rows.ToArray(), column);
			return Task.FromResult<ITable>(result);
		}

		public Task<ITable> SearchNotLikeAsync(ITable source, int column, string pattern, char escapeChar) {
			// How this works:
			//   Find the set or rows that are like the pattern.
			//   Find the complete set of rows in the column.
			//   Sort the 'like' rows
			//   For each row that is in the original set and not in the like set,
			//     add to the result list.
			//   Result is the set of not like rows ordered by the column.

			var columnType = source.TableInfo.Columns[column].ColumnType;
			var likeSet = Search(source, column, pattern, escapeChar).ToBigList();
			// Don't include NULL values
			var nullCell = SqlObject.NullOf(columnType);
			var originalSet = source.SelectRows(column, SqlExpressionType.IsNot, nullCell).ToList();
			var listSize = System.Math.Max(4L, (originalSet.Count - likeSet.Count) + 4);
			var resultSet = new BigList<long>(listSize);
			likeSet.Sort();
			int size = originalSet.Count;
			for (int i = 0; i < size; ++i) {
				var val = originalSet[i];
				// If val not in like set, add to result
				if (likeSet.IndexOf(val) == 0) {
					resultSet.Add(val);
				}
			}

			var result = new VirtualTable(source, resultSet, column);
			return Task.FromResult<ITable>(result);
		}

		public static IEnumerable<long> Search(ITable table, int column, string pattern, char escapeChar) {
			var colType = table.TableInfo.Columns[column].ColumnType;

			// If the column type is not a string type then report an error.
			if (!(colType is SqlCharacterType))
				throw new InvalidOperationException("Unable to perform a pattern search on a non-String type column.");

			// First handle the case that the column has an index that supports text search
			var index = table.GetColumnIndex(column);

			var colStringType = (SqlCharacterType)colType;

			// ---------- Pre Search ----------

			// First perform a 'pre-search' on the head of the pattern.  Note that
			// there may be no head in which case the entire column is searched which
			// has more potential to be expensive than if there is a head.

			var prePattern = new StringBuilder();
			int i = 0;
			bool finished = i >= pattern.Length;
			bool lastIsEscape = false;

			while (!finished) {
				char c = pattern[i];
				if (lastIsEscape) {
					lastIsEscape = true;
					prePattern.Append(c);
				} else if (c == escapeChar) {
					lastIsEscape = true;
				} else if (!PatternSearch.IsWildCard(c)) {
					prePattern.Append(c);

					++i;
					if (i >= pattern.Length) {
						finished = true;
					}

				} else {
					finished = true;
				}
			}

			// This is set with the remaining search.
			string postPattern;

			// This is our initial search row set.  In the second stage, rows are
			// eliminated from this vector.
			IEnumerable<long> searchCase;

			if (i >= pattern.Length) {
				// If the pattern has no 'wildcards' then just perform an EQUALS
				// operation on the column and return the results.

				var cell = new SqlObject(colType, new SqlString(pattern));
				return table.SelectRows(column, SqlExpressionType.Equal, cell);
			}

			if (prePattern.Length == 0 ||
			    colStringType.Locale != null) {

				// No pre-pattern easy search :-(.  This is either because there is no
				// pre pattern (it starts with a wild-card) or the locale of the string
				// is non-lexicographical.  In either case, we need to select all from
				// the column and brute force the search space.

				searchCase = table.SelectAllRows(column);
				postPattern = pattern;
			} else {

				// Criteria met: There is a pre_pattern, and the column locale is
				// lexicographical.

				// Great, we can do an upper and lower bound search on our pre-search
				// set.  eg. search between 'Geoff' and 'Geofg' or 'Geoff ' and
				// 'Geoff\33'

				var lowerBounds = prePattern.ToString();
				int nextChar = prePattern[i - 1] + 1;
				prePattern[i - 1] = (char)nextChar;
				var upperBounds = prePattern.ToString();

				postPattern = pattern.Substring(i);

				var cellLower = new SqlObject(colType, new SqlString(lowerBounds));
				var cellUpper = new SqlObject(colType, new SqlString(upperBounds));

				// Select rows between these two points.

				searchCase = SelectRowsBetween(table, column, cellLower, cellUpper);
			}

			// ---------- Post search ----------

			int preIndex = i;

			// Now eliminate from our 'search_case' any cells that don't match our
			// search pattern.
			// Note that by this point 'post_pattern' will start with a wild card.
			// This follows the specification for the 'PatternMatch' method.
			// EFFICIENCY: This is a brute force iterative search.  Perhaps there is
			//   a faster way of handling this?

			var iList = new BlockIndex<SqlObject, long>(searchCase);
			using (var enumerator = iList.GetEnumerator(0, iList.Count - 1)) {
				while (enumerator.MoveNext()) {
					// Get the expression (the contents of the cell at the given column, row)

					bool patternMatches = false;
					var cell = table.GetValue(enumerator.Current, column);
					// Null values doesn't match with anything
					if (!cell.IsNull) {
						string expression = ((SqlString) cell.Value).ToString();
						// We must remove the head of the string, which has already been
						// found from the pre-search section.
						expression = expression.Substring(preIndex);
						patternMatches = PatternSearch.PatternMatch(postPattern, expression, escapeChar);
					}
					if (!patternMatches) {
						// If pattern does not match then remove this row from the search.
						enumerator.Remove();
					}
				}
			}

			return iList.ToList();
		}

		private static IEnumerable<long> SelectRowsBetween(ITable table, int column, SqlObject minCell, SqlObject maxCell) {
			// Check all the tables are comparable
			var colType = table.TableInfo.Columns[column].ColumnType;
			if (!minCell.Type.IsComparable(colType) ||
			    !maxCell.Type.IsComparable(colType)) {
				// Types not comparable, so return 0
				return new BigList<long>(0);
			}

			return table.GetColumnIndex(column).SelectBetween(new[] { minCell }, new[] { maxCell });
		}
	}
}