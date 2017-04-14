using System;
using System.Collections.Generic;

namespace Deveel.Data.Security {
	public static class RequirementCollectionExtensions {
		public static void Append(this IRequirementCollection requirements, IEnumerable<IRequirement> collection) {
			if (collection == null)
				return;

			foreach (var requirement in collection) {
				requirements.Require(requirement);
			}
		}

		public static void AddRequirement(this IRequirementCollection requirements, Func<IContext, bool> requirement) {
			requirements.Require(new DelegatedRequirement(requirement));
		}

		public static void RequirePrivileges(this IRequirementCollection requirements,
			DbObjectType objectType, ObjectName objName, Privileges privileges) {
			requirements.AddRequirement(context => context.UserHasPrivileges(objectType, objName, privileges));
		}

		public static void RequireCreateInSchema(this IRequirementCollection collection, string schemaName)
			=> collection.AddRequirement(context => context.UserCanCreateInSchema(schemaName));

		public static void RequireSelectPrivilege(this IRequirementCollection requirements, ObjectName tableName)
			=> requirements.AddRequirement(context => context.UserCanSelectFrom(tableName));

		public static void RequireUpdatePrivilege(this IRequirementCollection requirements, ObjectName tableName)
			=> requirements.AddRequirement(context => context.UserCanUpdate(tableName));
	}
}