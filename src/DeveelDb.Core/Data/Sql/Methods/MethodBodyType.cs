using System;

namespace Deveel.Data.Sql.Methods {
	/// <summary>
	/// The available kind of <see cref="ISqlMethodBody"/>
	/// </summary>
	public enum MethodBodyType {
		Native = 1,
		CodeBlock = 2,
		ExternalRef = 3
	}
}