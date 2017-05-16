using System;

namespace Deveel.Data {
	public interface IErrorResolver {
		SystemError ResolveError(string errorName);
	}
}