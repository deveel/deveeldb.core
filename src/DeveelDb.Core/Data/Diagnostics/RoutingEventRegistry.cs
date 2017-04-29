using System;
using System.Linq;
using System.Threading.Tasks;

namespace Deveel.Data.Diagnostics {
	public class RoutingEventRegistry<TEvent> : ThreadedQueue<TEvent>, IEventRegistry<TEvent>
		where TEvent : class, IEvent {
		private readonly IContext context;

		public RoutingEventRegistry(IContext context) 
			: base(context.EventRegistryThreadCount()) {
			this.context = context;
		}

		protected override void Consume(TEvent message) {
			var tasks = context.GetEventRouters()
				.Where(x => x.CanRoute(message))
				.Select(x => x.RouteEventAsync(message))
				.ToArray();

			try {
				Task.WaitAll(tasks);
			} catch (Exception ex) {
				context.Error(-1, "Error while consuming an event", ex);
			}
		}

		public Type EventType => typeof(TEvent);

		public void Register(TEvent @event) {
			Publish(@event);
		}

		void IEventRegistry.Register(IEvent @event) {
			Register((TEvent)@event);
		}
	}
}