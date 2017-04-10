using System;

namespace Deveel.Data.Sql.Methods {
	public interface IMethodResolver {
		SqlMethod ResolveMethod(IContext context, Invoke invoke);
	}
}