﻿// 
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

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Transactions {
	public class TransactionEvent : Event {
		public TransactionEvent(IEventSource source, int eventId, long commitId) 
			: base(source, eventId) {
			CommitId = commitId;
		}

		public long CommitId { get; }

		protected override void GetEventData(Dictionary<string, object> data) {
			data["commit.id"] = CommitId;
		}
	}
}