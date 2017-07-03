using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Expressions {
	public interface ISqlExpressionParser {
		SqlExpressionParseResult Parse(string expression);
	}
}