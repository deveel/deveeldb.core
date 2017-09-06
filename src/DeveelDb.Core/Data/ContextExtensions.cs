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
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Services;

namespace Deveel.Data {
	public static class ContextExtensions {
		#region Configuration

		public static bool IgnoreCase(this IContext context) {
			return context.GetValue("ignoreCase", true);
		}

		public static string CurrentSchema(this IContext context) {
			return context.GetValue<string>("currentSchema");
		}

		public static int LockTimeout(this IContext context) {
			return context.GetValue("lock.timeout", 1500);
		}

		public static string StoraeSystem(this IContext context) {
			return context.GetValue<string>("store.system");
		}

		#endregion

		public static IContext Create(this IContext context, string name, Action<IScope> scope = null) {
			return  new Context(context, name, scope);
		}

		public static IDbObjectManager GetObjectManager(this IContext context, DbObjectType objectType) {
			return context.Scope.Resolve<IDbObjectManager>(objectType);
		}

		public static TManager GetObjectManager<TManager>(this IContext context, DbObjectType objectType)
			where TManager : class, IDbObjectManager {
			return (TManager) context.GetObjectManager(objectType);
		}

		public static IEnumerable<IDbObjectManager> GetObjectManagers(this IContext context) {
			return context.Scope.ResolveAll<IDbObjectManager>();

		}

		public static async Task<IDbObject> GetObjectAsync(this IContext context, DbObjectType objectType, ObjectName objectName) {
			var manager = context.GetObjectManager(objectType);
			if (manager == null)
				return null;

			return await manager.GetObjectAsync(objectName);
		}

		public static async Task<DbObjectType> GetObjectType(this IContext context, ObjectName objectName) {
			var managers = context.GetObjectManagers();
			foreach (var manager in managers) {
				if (await manager.ObjectExistsAsync(objectName))
					return manager.ObjectType;
			}

			throw new InvalidOperationException($"Object {objectName} not found in this context");
		}

		public static async Task<bool> ObjectExistsAsync(this IContext context, DbObjectType objectType, ObjectName objectName) {
			var manager = context.GetObjectManager(objectType);
			if (manager == null)
				return false;

			return await manager.ObjectExistsAsync(objectName);
		}

		public static async Task<IDbObjectInfo> GetObjectInfoAsync(this IContext context, DbObjectType objectType, ObjectName objectName) {
			var manager = context.GetObjectManager(objectType);
			if (manager == null)
				return null;

			return await manager.GetObjectInfoAsync(objectName);
		}

		public static async Task<IDbObject> CreateObjectAsync(this IContext context, IDbObjectInfo objectInfo) {
			if (objectInfo == null) throw new ArgumentNullException(nameof(objectInfo));

			var manager = context.GetObjectManager(objectInfo.ObjectType);
			if (manager == null)
				throw new InvalidOperationException($"No manager for type {objectInfo.ObjectType} was defined in the context");

			await manager.CreateObjectAsync(objectInfo);

			return await context.GetObjectAsync(objectInfo.ObjectType, objectInfo.FullName);
		}

		public static ObjectName QualifyName(this IContext context, ObjectName objectName) {
			if (objectName.Parent == null) {
				var currentSchema = context.CurrentSchema();
				if (String.IsNullOrWhiteSpace(currentSchema))
					throw new InvalidOperationException("None schema was set in context");

				objectName = new ObjectName(new ObjectName(currentSchema), objectName.Name);
			}

			return objectName;
		}

		public static Task<ObjectName> ResolveNameAsync(this IContext context, ObjectName objectName) {
			var ignoreCase = context.IgnoreCase();

			var managers = context.GetObjectManagers();
			foreach (var manager in managers) {
				var resolved = manager.ResolveNameAsync(objectName, ignoreCase);
				if (resolved != null)
					return resolved;
			}

			return null;
		}
	}
}