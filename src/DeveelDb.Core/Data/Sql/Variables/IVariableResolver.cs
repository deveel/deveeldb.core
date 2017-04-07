using System;

namespace Deveel.Data.Sql.Variables {
	public interface IVariableResolver {
		Variable ResolveVariable(string name);
	}
}