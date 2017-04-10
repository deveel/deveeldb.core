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
using System.Net.Http.Headers;

using Deveel.Data.Configuration;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Variables {
	public sealed class VariableManager : IDbObjectManager, IVariableResolver {
		private bool disposed;
		private Dictionary<ObjectName, Variable> variables;

		public VariableManager() {
			variables = new Dictionary<ObjectName, Variable>(ObjectNameComparer.Ordinal);
		}

		~VariableManager() {
			Dispose(false);
		}

		DbObjectType IDbObjectManager.ObjectType => DbObjectType.Variable;

		void IDbObjectManager.CreateObject(IDbObjectInfo objInfo) {
			CreateVariable((VariableInfo)objInfo);
		}

		public void CreateVariable(VariableInfo variableInfo) {
			variables.Add(new ObjectName(variableInfo.Name), new Variable(variableInfo));
		}

		bool IDbObjectManager.RealObjectExists(ObjectName objName) {
			return VariableExists(objName.FullName);
		}

		bool IDbObjectManager.ObjectExists(ObjectName objName) {
			return VariableExists(objName.FullName);
		}

		public bool VariableExists(string name) {
			return variables.ContainsKey(new ObjectName(name));
		}

		IDbObject IDbObjectManager.GetObject(ObjectName objName) {
			return GetVariable(objName.FullName);
		}

		bool IDbObjectManager.AlterObject(IDbObjectInfo objInfo) {
			throw new NotSupportedException();
		}

		bool IDbObjectManager.DropObject(ObjectName objName) {
			return RemoveVariable(objName.FullName);
		}

		ObjectName IDbObjectManager.ResolveName(ObjectName objName, bool ignoreCase) {
			var comparer = ignoreCase ? ObjectNameComparer.IgnoreCase : ObjectNameComparer.Ordinal;
			var dictionary = new Dictionary<ObjectName, Variable>(variables, comparer);
			Variable variable;
			if (!dictionary.TryGetValue(objName, out variable))
				return null;

			return (variable.VariableInfo as IDbObjectInfo).FullName;
		}

		public Variable ResolveVariable(string name, bool ignoreCase) {
			var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
			var dictionary = variables.ToDictionary(x => x.Key.FullName, y => y.Value, comparer);
			Variable variable;
			if (!dictionary.TryGetValue(name, out variable))
				return null;

			return variable;
		}

		public Variable GetVariable(string name) {
			var objName = new ObjectName(name);
			Variable variable;
			if (variables.TryGetValue(objName, out variable))
				return variable;

			return null;
		}

		public SqlExpression AssignVariable(string name, SqlExpression value, IContext context) {
			var variable = FindVariable(name, context);
			if (variable == null) {
				var type = value.GetSqlType(context);
				variable = new Variable(name, type);
				variables.Add(new ObjectName(name),  variable);
			}

			return variable.SetValue(value, context);
		}

		public bool RemoveVariable(string name) {
			return variables.Remove(new ObjectName(name));
		}

		private Variable FindVariable(string name, IContext context) {
			var ignoreCase = context.GetValue("ignoreCase", false);
			return ResolveVariable(name, ignoreCase);
		}

		private void Dispose(bool disposing) {
			if (!disposed) {
				if (disposing)
					variables.Clear();

				variables = null;
				disposed = true;
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}