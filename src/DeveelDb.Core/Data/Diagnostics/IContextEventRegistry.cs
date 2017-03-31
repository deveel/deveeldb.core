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

namespace Deveel.Data.Diagnostics {
	/// <summary>
	/// A version of <see cref="IEventRegistry"/> that is contained
	/// in a context that also exposes a <see cref="IEventSource"/>.
	/// </summary>
	public interface IContextEventRegistry : IEventRegistry {
		/// <summary>
		/// Gets a reference to the <see cref="IEventSource"/> that
		/// is encapsulated in the parent context
		/// </summary>
		IEventSource EventSource { get; }
	}
}