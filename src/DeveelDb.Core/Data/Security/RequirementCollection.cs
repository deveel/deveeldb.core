using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Data.Security {
	public class RequirementCollection : IRequirementCollection {
		private readonly List<IRequirement> requirements;

		public RequirementCollection() {
			requirements = new List<IRequirement>();
		}

		public void Require(IRequirement requirement) {
			requirements.Add(requirement);
		}

		public IEnumerator<IRequirement> GetEnumerator() {
			return requirements.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}