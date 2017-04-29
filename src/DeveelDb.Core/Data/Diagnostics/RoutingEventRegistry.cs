// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


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