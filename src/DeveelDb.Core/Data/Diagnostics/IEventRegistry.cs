using System;

namespace Deveel.Data.Diagnostics {
	public interface IEventRegistry {
		Type EventType { get; }

		void Register(IEvent @event);
	}
}