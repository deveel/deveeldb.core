using System;

namespace Deveel.Data {
	public interface IRequest : IContext {
		IQuery Query { get; }
	}
}