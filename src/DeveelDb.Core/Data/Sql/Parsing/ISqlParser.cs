using System.Threading.Tasks;

namespace Deveel.Data.Sql.Parsing {
	public interface ISqlParser {
		string Dialect { get; }


		Task<SqlParseResult> ParseAsync(string sql);
	}
}