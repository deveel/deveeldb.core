using System;

using Xunit.Sdk;

namespace Deveel {
	[AttributeUsage(AttributeTargets.Method)]
	public class BugAttribute : TraitAttribute {
		public BugAttribute(string id)
			: base("bug") {
			Id = id;
		}

		public string Id { get; }
	}
}