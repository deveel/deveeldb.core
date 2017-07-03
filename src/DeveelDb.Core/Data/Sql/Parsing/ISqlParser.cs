using System.Threading.Tasks;

namespace Deveel.Data.Sql.Parsing {
	public interface ISqlParser {
		string Dialect { get; }


		SqlParseResult Parse(string sql);
	}
}