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