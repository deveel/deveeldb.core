using System;

namespace Deveel {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class FeatureAttribute : TraitAttribute {
		public FeatureAttribute(string name)
			: base("feature") {
			Name = name;
		}

		public string Name { get; }
	}
}