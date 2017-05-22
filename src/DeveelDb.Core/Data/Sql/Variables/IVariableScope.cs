using System;

namespace Deveel.Data.Sql.Variables {
	public interface IVariableScope {
		IVariableManager Variables { get; }
	}
}