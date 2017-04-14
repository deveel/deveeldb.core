using System;
using System.Collections.Generic;

namespace Deveel.Data.Security {
	public interface ISecurityManager : ISecurityResolver {
		bool CreateRole(string role);

		bool DropRole(string role);

		bool GrantToRole(string role, ObjectName objName, Privileges privileges);

		bool RevokeFromRole(string role, ObjectName objName, Privileges privileges);

		bool CreateUser(string user);

		bool DropUser(string user);

		bool AddUserToRole(string user, string role);

		bool RemoveUserFromRole(string user, string role);

		IEnumerable<Role> GetUserRoles(string user);

		bool GrantToUser(string user, ObjectName objName, Privileges privileges);

		bool RevokeFromUser(string user, ObjectName objName, Privileges privileges);
	}
}