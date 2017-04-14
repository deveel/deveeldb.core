using System;

namespace Deveel.Data.Security {
	public sealed class DelegatedRequirement : IRequirement {
		public DelegatedRequirement(Func<IContext, bool> body) {
			if (body == null)
				throw new ArgumentNullException(nameof(body));

			Body = body;
		}

		public Func<IContext, bool> Body { get; }
	}
}