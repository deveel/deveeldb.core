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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Deveel.Data.Services;

namespace Deveel.Data.Diagnostics {
	public static class ContextExtensions {
		private static IEventSource GetEventSource(this IContext context) {
			var current = context;
			while (current != null) {
				if (current is IEventSource)
					return (IEventSource) current;

				current = current.ParentContext;
			}

			return null;
		}

		private static IEnumerable<IEventRegistry> FindRegistries(this IContext context, Type eventType) {
			return context.Scope.ResolveAll<IEventRegistry>()
				.Where(x => x.EventType.GetTypeInfo().IsAssignableFrom(eventType.GetTypeInfo()));
		}

		public static void RegisterEvent(this IContext context, Type eventType, params object[] args) {
			var source = context.GetEventSource();
			var registries = context.FindRegistries(eventType);
			foreach (var registry in registries) {
				registry.Register(eventType, source, args);
			}
		}

		private static IEnumerable<IEventRegistry<TEvent>> FindRegistries<TEvent>(this IContext context)
			where TEvent : class, IEvent {
			return context.Scope.ResolveAll<IEventRegistry<TEvent>>();
		}

		public static void RegisterEvent<TEvent>(this IContext context, params object[] args)
			where TEvent : class, IEvent {
			var source = context.GetEventSource();
			var registries = context.FindRegistries<TEvent>();
			foreach (var registry in registries) {
				registry.Register(source, args);
			}
		}

		#region LogMessages

		public static void LogMessage(this IContext context, int id, MessageLevel level, string text, Exception error) {
			context.RegisterEvent<MessageEvent>(id, level, text, error);
		}

		public static void LogMessage(this IContext context, int id, MessageLevel level, string text)
			=> context.LogMessage(id, level, text, null);

		public static void Debug(this IContext context, int id, string text)
			=> context.LogMessage(id, MessageLevel.Debug, text);

		public static void Trace(this IContext context, int id, string text)
			=> context.LogMessage(id, MessageLevel.Trace, text, null);

		public static void Information(this IContext context, int id, string text)
			=> context.LogMessage(id, MessageLevel.Information, text, null);

		public static void Warning(this IContext context, int id, string text, Exception error)
			=> context.LogMessage(id, MessageLevel.Warning, text, error);

		public static void Error(this IContext context, int id, string text, Exception error)
			=> context.LogMessage(id, MessageLevel.Error, text, error);

		public static void Fatal(this IContext context, int id, string text, Exception error)
			=> context.LogMessage(id, MessageLevel.Fatal, text, error);

		#endregion
	}
}