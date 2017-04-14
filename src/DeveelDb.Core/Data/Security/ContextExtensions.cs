using System;
using System.Linq;

using Deveel.Data.Services;

namespace Deveel.Data.Security {
	public static class ContextExtensions {
		public static User User(this IContext context) {
			var current = context;
			while (current != null) {
				if (current is ISecurityContext)
					return ((ISecurityContext) current).User;

				current = current.ParentContext;
			}

			return null;
		}

		public static bool UserHasPrivileges(this IContext context, DbObjectType objectType, ObjectName objectName, Privileges privileges) {
			var user = context.User();
			if (user == null)
				return false;

			// if no security resolver was registered this means no security
			// checks are required
			var resolver = context.Scope.Resolve<ISecurityResolver>();
			if (resolver == null)
				return true;

			if (!resolver.HasPrivileges(user.Name, objectType, objectName, privileges)) {
				var securityManager = context.Scope.Resolve<ISecurityManager>();
				if (securityManager == null)
					return false;

				var roles = securityManager.GetUserRoles(user.Name);
				return roles.Any(x => resolver.HasPrivileges(x.Name, objectType, objectName, privileges));
			}

			return true;
		}

		public static bool UserCanCreateInSchema(this IContext context, string schemaName) {
			return context.UserHasPrivileges(DbObjectType.Schema, new ObjectName(schemaName), Privileges.Create);
		}

		public static bool UserCanCreateSchema(this IContext context) {
			var user = context.User();
			if (user.IsSystem)
				return true;

			// TODO: check if the user is in a role that can create
			throw new NotImplementedException();
		}

		public static bool UserCanSelectFrom(this IContext context, ObjectName tableName)
			=> context.UserHasPrivileges(DbObjectType.Table, tableName, Privileges.Select);

		public static bool UserCanUpdate(this IContext context, ObjectName tableName)
			=> context.UserHasPrivileges(DbObjectType.Table, tableName, Privileges.Update);

		public static bool UserCanDelete(this IContext context, ObjectName tableName)
			=> context.UserHasPrivileges(DbObjectType.Table, tableName, Privileges.Delete);

		public static bool UserCanInsert(this IContext context, ObjectName tableName)
			=> context.UserHasPrivileges(DbObjectType.Table, tableName, Privileges.Insert);

		public static bool UserCanDrop(this IContext context, DbObjectType objectType, ObjectName objectName)
			=> context.UserHasPrivileges(objectType, objectName, Privileges.Drop);

		public static bool UserCanExecute(this IContext context, ObjectName methodName)
			=> context.UserHasPrivileges(DbObjectType.Method, methodName, Privileges.Execute);

		public static bool UserCanAlter(this IContext context, DbObjectType objectType, ObjectName objectName)
			=> context.UserHasPrivileges(objectType, objectName, Privileges.Alter);

		public static bool UserCanReference(this IContext context, ObjectName tableName)
			=> context.UserHasPrivileges(DbObjectType.Table, tableName, Privileges.References);
	}
}