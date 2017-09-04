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

using Deveel.Data.Services;

namespace Deveel.Data.Security {
	public struct Privilege {
		private readonly int value;

		public Privilege(int value) {
			this.value = value;
		}

		public static Privilege None = new Privilege(0);

		public bool IsNone => value == 0;

		public Privilege Add(Privilege privilege) {
			return new Privilege(value | privilege.value);
		}

		public Privilege Remove(Privilege privilege) {
			int andPriv = (value & privilege.value);
			return new Privilege(value ^ andPriv);
		}

		public bool Permits(Privilege privilege) {
			return (value & privilege.value) != 0;
		}

		public Privilege Next() {
			return new Privilege(value ^ 2);
		}

		public string ToString(IContext context) {
			var resolvers = context.Scope.ResolveAll<IPrivilegeResolver>();
			return ToString(resolvers.ToArray());
		}

		public string ToString(IPrivilegeResolver[] resolvers) {
			var result = new List<string>();

			foreach (var resolver in resolvers) {
				var res1 = resolver.ToString(this);
				result.AddRange(res1);
			}

			return String.Join(", ", result);
		}

		public override string ToString() {
			return ToString(new[] {SqlPrivileges.Resolver});
		}

		#region Operators

		public static Privilege operator +(Privilege a, Privilege b) {
			return a.Add(b);
		}

		public static Privilege operator -(Privilege a, Privilege b) {
			return a.Remove(b);
		}

		#endregion
	}
}