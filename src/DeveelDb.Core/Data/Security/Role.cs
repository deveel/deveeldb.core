using System;

namespace Deveel.Data.Security {
	public sealed class Role : IPriviledged {
		public Role(string name) {
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			Name = name;
		}

		public string Name { get; }
	}
}