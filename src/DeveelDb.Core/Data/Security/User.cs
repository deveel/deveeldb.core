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

namespace Deveel.Data.Security {
	public sealed class User : IPriviledged {
		public const string SystemName = "@SYSTEM";
		private static readonly char[] InvalidChars = "'@\"".ToCharArray();

		private User(string name, bool validate) {
			if (validate && !IsValidName(name))
				throw new ArgumentException($"User name {name} is invalid");

			Name = name;
		}

		public User(string name)
			: this(name, true) { 
		}

		public string Name { get; }

		public bool IsSystem => String.Equals(Name, SystemName, StringComparison.OrdinalIgnoreCase);

		public static User System = new User(SystemName, false);

		public static bool IsValidName(string name) {
			if (String.IsNullOrWhiteSpace(name))
				return false;

			return name.IndexOfAny(InvalidChars) < 0;
		}
	}
}