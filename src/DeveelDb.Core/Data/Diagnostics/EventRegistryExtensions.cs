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
using System.Reflection;

using Deveel.Util;

namespace Deveel.Data.Diagnostics {
	public static class EventRegistryExtensions {
		private static IEvent CreateEvent(Type eventType, IEventSource source, params object[] args) {
			var ctorArgs = ArrayUtil.Introduce(source, args);
			return Activator.CreateInstance(eventType, ctorArgs) as IEvent;
		}

		public static void Register(this IEventRegistry registry, Type type, IEventSource source, params object[] args) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			if (!typeof(IEvent).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
				throw new ArgumentException($"The type '{type}' is not assignable from '{typeof(IEvent)}'.");

			var @event = CreateEvent(type, source, args);
			registry.Register(@event);
		}

		public static void Register<TEvent>(this IEventRegistry registry, IEventSource source, params object[] args)
			where TEvent : class, IEvent {
			registry.Register(typeof(TEvent), source, args);
		}

		public static void Register(this IContextEventRegistry registry, Type type, params object[] args)
			=> (registry as IEventRegistry).Register(type, registry.EventSource, args);

		public static void Register<TEvent>(this IContextEventRegistry registry, params object[] args)
			where TEvent : class, IEvent
			=> ((IEventRegistry) registry).Register(typeof(TEvent), registry.EventSource, args);

		#region Messages

		public static void RegisterMessage(this IEventRegistry registry,
			IEventSource source,
			int eventId,
			MessageLevel level,
			string text)
			=> RegisterMessage(registry, source, eventId, level, text, null);

		public static void RegisterMessage(this IEventRegistry registry,
			IEventSource source,
			int eventId,
			MessageLevel level,
			string text,
			Exception error)
			=> registry.Register<MessageEvent>(source, eventId, level, text, error);

		public static void Trace(this IEventRegistry registry, IEventSource source, int eventId, string message)
			=> registry.RegisterMessage(source, eventId, MessageLevel.Trace, message);

		public static void Information(this IEventRegistry registry, IEventSource source, int eventId, string message)
			=> registry.RegisterMessage(source, eventId, MessageLevel.Information, message);

		public static void Debug(this IEventRegistry registry, IEventSource source, int eventId, string message)
			=> registry.RegisterMessage(source, eventId, MessageLevel.Debug, message);

		public static void Warning(this IEventRegistry registry, IEventSource source, int eventId, Exception error) {
			Warning(registry, source, eventId, null, error);
		}

		public static void Warning(this IEventRegistry registry, IEventSource source, int eventId, string message) {
			Warning(registry, source, eventId, message, null);
		}

		public static void Warning(this IEventRegistry registry,
			IEventSource source,
			int eventId,
			string message,
			Exception error)
			=> registry.RegisterMessage(source, eventId, MessageLevel.Warning, message, error);

		public static void Error(this IEventRegistry registry, IEventSource source, int eventId, Exception error) {
			Error(registry, source, eventId, null, error);
		}

		public static void Error(this IEventRegistry registry, IEventSource source, int eventId, string message) {
			Error(registry, source, eventId, message, null);
		}

		public static void Error(this IEventRegistry registry,
			IEventSource source,
			int eventId,
			string message,
			Exception error)
			=> registry.RegisterMessage(source, eventId, MessageLevel.Error, message, error);

		public static void Fatal(this IEventRegistry registry, IEventSource source, int eventId, Exception error) {
			Fatal(registry, source, eventId, null, error);
		}

		public static void Fatal(this IEventRegistry registry, IEventSource source, int eventId, string message) {
			Fatal(registry, source, eventId, message, null);
		}

		public static void Fatal(this IEventRegistry registry, IEventSource source, int eventId, string message, Exception error)
			=> registry.RegisterMessage(source, eventId, MessageLevel.Fatal, message, error);


		// withouth source

		public static void RegisterMessage(this IContextEventRegistry registry, int eventId, MessageLevel level, string text)
			=> registry.RegisterMessage(eventId, level, text, null);

		public static void RegisterMessage(this IContextEventRegistry registry,
			int eventId,
			MessageLevel level,
			string text,
			Exception error)
			=> (registry as IEventRegistry).Register<MessageEvent>(registry.EventSource, eventId, level, text, error);

		public static void Trace(this IContextEventRegistry registry, int eventId, string message)
			=> registry.RegisterMessage(eventId, MessageLevel.Trace, message);

		public static void Information(this IContextEventRegistry registry, int eventId, string message)
			=> registry.RegisterMessage(eventId, MessageLevel.Information, message);

		public static void Debug(this IContextEventRegistry registry, int eventId, string message)
			=> registry.RegisterMessage(eventId, MessageLevel.Debug, message);

		public static void Warning(this IContextEventRegistry registry, int eventId, Exception error)
			=> registry.Warning(eventId, null, error);

		public static void Warning(this IContextEventRegistry registry, int eventId, string message)
			=> registry.Warning(eventId, message, null);

		public static void Warning(this IContextEventRegistry registry, int eventId, string message, Exception error)
			=> registry.RegisterMessage(eventId, MessageLevel.Warning, message, error);

		public static void Error(this IContextEventRegistry registry, int eventId, Exception error) {
			Error(registry, eventId, null, error);
		}

		public static void Error(this IContextEventRegistry registry, int eventId, string message) {
			Error(registry, eventId, message, null);
		}

		public static void Error(this IContextEventRegistry registry, int eventId, string message, Exception error)
			=> registry.RegisterMessage(eventId, MessageLevel.Error, message, error);

		public static void Fatal(this IContextEventRegistry registry, int eventId, Exception error) {
			Fatal(registry, eventId, null, error);
		}

		public static void Fatal(this IContextEventRegistry registry, int eventId, string message) {
			Fatal(registry, eventId, message, null);
		}

		public static void Fatal(this IContextEventRegistry registry, int eventId, string message, Exception error)
			=> registry.RegisterMessage(eventId, MessageLevel.Fatal, message, error);


		#endregion
	}
}