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
using System.Threading.Tasks;

namespace Deveel.Data.Security {
	public interface ISecurityManager : ISecurityResolver {
		Task<bool> CreateRoleAsync(string role);

		Task<bool> DropRoleAsync(string role);

		Task<bool> GrantToRoleAsync(string role, ObjectName objName, Privilege privilege);

		Task<bool> RevokeFromRoleAsync(string role, ObjectName objName, Privilege privilege);

		Task<bool> CreateUserAsync(string user);

		Task<bool> DropUserAsync(string user);

		Task<bool> AddUserToRoleAsync(string user, string role);

		Task<bool> RemoveUserFromRoleAsync(string user, string role);

		Task<IEnumerable<Role>> GetUserRolesAsync(string user);

		Task<bool> GrantToUserAsync(string user, ObjectName objName, Privilege privileges);

		Task<bool> RevokeFromUserAsync(string user, ObjectName objName, Privilege privileges);
	}
}