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
using System.Threading.Tasks;

using Deveel.Data.Services;

namespace Deveel.Data.Diagnostics {
	public static class ScopeExtensions {
		public static void AddEventRegistry<TRegistry>(this IScope scope)
			where TRegistry : class, IEventRegistry {
			scope.Register<IEventRegistry, TRegistry>();
		}

		public static void AddRoutingEventRegistry<TEvent>(this IScope scope)
			where TEvent : class, IEvent {
			scope.AddEventRegistry<RoutingEventRegistry<TEvent>>();
			scope.Register<IEventRegistry<TEvent>, RoutingEventRegistry<TEvent>>();
		}

		public static void AddEventRouter<TRouter>(this IScope scope)
			where TRouter : class, IEventRouter {
			scope.Register<IEventRouter, TRouter>();
		}

		public static void AddEventRouter<TEvent>(this IScope scope, Func<TEvent, Task> handler)
			where TEvent : class, IEvent {
			scope.RegisterInstance<IEventRouter>(new DelegatedEventRouter<TEvent>(handler));
		}

		public static void AddEventRouter<TEvent>(this IScope scope, Action<TEvent> handler)
			where TEvent : class, IEvent
			=> scope.AddEventRouter<TEvent>(e => {
				handler(e);
				return Task.CompletedTask;
			});

		#region DelegatedEventRouter

		class DelegatedEventRouter<TEvent> : IEventRouter where TEvent : class, IEvent {
			private readonly Func<TEvent, Task> handler;

			public DelegatedEventRouter(Func<TEvent, Task> handler) {
				this.handler = handler;
			}

			public bool CanRoute(IEvent @event) {
				return @event is TEvent;
			}

			public Task RouteEventAsync(IEvent e) {
				return handler((TEvent) e);
			}
		}

		#endregion
	}
}