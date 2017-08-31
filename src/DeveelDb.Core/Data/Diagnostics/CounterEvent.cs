using System;
using System.Collections.Generic;

namespace Deveel.Data.Diagnostics {
	public class CounterEvent : Event {
		public CounterEvent(IEventSource source, string counterKey)
			: this(source, counterKey, null) {
		}

		public CounterEvent(IEventSource source, string counterKey, object value)
			: base(source, -1) {
			if (String.IsNullOrEmpty(counterKey))
				throw new ArgumentNullException("counterKey");

			CounterKey = counterKey;
			Value = value;
		}

		public string CounterKey { get; private set; }

		public object Value { get; private set; }

		public bool IsIncremental {
			get { return Value == null; }
		}

		protected override void GetEventData(Dictionary<string, object> data) {
			data[CounterKey] = Value;
		}
	}
}