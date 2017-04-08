using System;

using Xunit.Sdk;

namespace Deveel {
	[AttributeUsage(AttributeTargets.Method)]
	public class TraitAttribute : Attribute, ITraitAttribute {
		public TraitAttribute(string category) {
			Category = category;
		}

		public string Category { get; }
	}
}