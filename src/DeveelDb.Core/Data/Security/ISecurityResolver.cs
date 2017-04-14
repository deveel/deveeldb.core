using System;

namespace Deveel.Data.Security {
	public interface ISecurityResolver {
		bool HasPrivileges(string grantee, DbObjectType objType, ObjectName objName, Privileges privileges);
	}
}