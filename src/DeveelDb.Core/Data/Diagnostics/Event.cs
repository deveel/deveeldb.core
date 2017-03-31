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

namespace Deveel.Data.Diagnostics {
	public class Event : IEvent {
		private IDictionary<string, object> metadata;

		public Event(IEventSource source, int id) 
			: this(source, id, DateTimeOffset.UtcNow) {
		}

		public Event(IEventSource source, int id, DateTimeOffset timeStamp) {
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			EventSource = source;
			EventId = id;
			TimeStamp = timeStamp;

			EnsureMetadata();
		}

		private void EnsureMetadata() {
			var meta = new Dictionary<string, object>();
			GetEventData(meta);
			metadata = new Dictionary<string, object>(meta);
		}

		protected virtual void GetEventData(Dictionary<string, object> data) {
		}

		public IEventSource EventSource { get; }

		public int EventId { get; }

		public DateTimeOffset TimeStamp { get; }

		IDictionary<string, object> IEvent.EventData => metadata;
	}
}
