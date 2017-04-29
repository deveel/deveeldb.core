using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Sequences {
	public static class ContextExtensions {
		public static async Task<bool> SequenceExistsAync(this IContext context, ObjectName sequenceName) {
			var handlers = context.ResolveAllServices<ISequenceHandler>();
			foreach (var handler in handlers) {
				if (await handler.HandlesSequence(sequenceName))
					return true;
			}

			return false;
		}
	}
}