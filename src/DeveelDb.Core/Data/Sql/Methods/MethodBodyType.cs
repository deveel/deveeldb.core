using System;

namespace Deveel.Data.Sql.Methods {
	/// <summary>
	/// The available kind of <see cref="SqlMethodBody"/>
	/// </summary>
	public enum MethodBodyType {
		CodeBlock = 1,
		ExternalRef = 2
	}
}