using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Variables {
	public interface IVariableManager : IDbObjectManager {
		bool VariableExists(string variableName);

		SqlExpression AssignVariable(string variableName, SqlExpression value, IContext context);
	}
}