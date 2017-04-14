using System;

namespace Deveel.Data.Security {
	public interface ISecurityContext : IContext {
		User User { get; }
	}
}