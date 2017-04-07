using System;
using System.Globalization;

using Deveel.Data.Text;

namespace Deveel.Data.Sql {
	public sealed class SqlDefaultStringSearch : ISqlStringSearch {
		public bool Matches(ISqlString source, string pattern, char escapeChar) {
			if (!(source is SqlString))
				throw new ArgumentException("This implementation of the string search does not support long strings");

			var s = ((SqlString) source).Value;
			return PatternSearch.PatternMatch(pattern, s, escapeChar);
		}
	}
}