using System;
using System.Collections.Generic;

namespace Deveel.Data.Security {
	public interface IRequirementCollection : IEnumerable<IRequirement> {
		void Require(IRequirement requirement);
	}
}