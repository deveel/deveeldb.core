using System;

namespace Deveel.Data.Security {
	public sealed class DelegatedRequirementHandler : IRequirementHandler<DelegatedRequirement> {
		public void HandleRequirement(IContext context, DelegatedRequirement requirement) {
			if (!requirement.Body(context))
				throw new UnauthorizedAccessException();
		}
	}
}