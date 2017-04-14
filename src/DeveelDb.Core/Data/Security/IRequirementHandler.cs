using System;

namespace Deveel.Data.Security {
	public interface IRequirementHandler<T> where T : IRequirement {
		void HandleRequirement(IContext context, T requirement);
	}
}