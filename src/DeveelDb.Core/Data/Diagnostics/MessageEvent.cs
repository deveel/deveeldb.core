using System;
using System.Collections.Generic;

namespace Deveel.Data.Diagnostics {
	public class MessageEvent : Event {
		public MessageEvent(IEventSource source, int id, MessageLevel level, string text) 
			: this(source, id, level, text, null) {
		}

		public MessageEvent(IEventSource source, int id, MessageLevel level, string text, Exception error) 
			: base(source, id) {
			Level = level;
			Text = text;
			Error = error;
		}

		public MessageLevel Level { get; }

		public string Text { get; }

		// TODO: Probably in the future I will implement a lightweight object for carrying the error
		//        Exceptions are too heavy for being transported and carried by memory, plus they cannot
		//        be serialized into messages
		public Exception Error { get; }

		protected override void GetEventData(Dictionary<string, object> data) {
			data["message.level"] = Level.ToString().ToLowerInvariant();
			data["message.text"] = Text;
			data["message.error.message"] = Error?.Message;
			data["message.error.stackTrace"] = Error?.StackTrace;
		}
	}
}