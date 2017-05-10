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

using Deveel.Data.Configuration;

namespace Deveel.Data.Security {
	public sealed class UserInfo {
		public UserInfo(string name, IConfiguration authConfiguration) {
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));
			if (authConfiguration == null)
				throw new ArgumentNullException(nameof(authConfiguration));

			Name = name;
			AuthConfiguration = authConfiguration;
		}

		public string Name { get; }

		public IConfiguration AuthConfiguration { get; }
	}
}