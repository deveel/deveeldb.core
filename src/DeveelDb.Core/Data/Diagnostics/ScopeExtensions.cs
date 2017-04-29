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