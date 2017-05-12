using System;
using System.Threading.Tasks;

using Deveel.Data.Services;

namespace Deveel.Data.Sql.Parsing {
	public static class SqlParser {
		// TODO:
		[ThreadStatic]
		public static ISqlParser Default = null;

		public static Task<SqlParseResult> ParseAsync(string sql) {
			return ParseAsync(null, sql);
		}

		public static Task<SqlParseResult> ParseAsync(IContext context, string sql) {
			var parser = context?.Scope.Resolve<ISqlParser>() ?? Default;
			return parser.ParseAsync(sql);
		}
	}
}