using System;

namespace Deveel.Data.Sql.Variables {
	public interface IVariableScope {
		VariableManager Variables { get; }
	}
}