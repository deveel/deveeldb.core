using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Expressions {
	public interface ISqlExpressionParser {
		Task<SqlExpression[]> ParseAsync(string expression);
	}
}