using System;
using System.Collections.Generic;
using System.Linq;

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
			throw new NotImplementedException();
		}

		bool IDbObjectManager.ObjectExists(ObjectName objName) {
			return variables.ContainsKey(objName);
		}

		IDbObject IDbObjectManager.GetObject(ObjectName objName) {
			throw new NotImplementedException();
		}

		bool IDbObjectManager.AlterObject(IDbObjectInfo objInfo) {
			throw new NotSupportedException();
		}

		bool IDbObjectManager.DropObject(ObjectName objName) {
			throw new NotImplementedException();
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

		public SqlExpression AssignVariable(string name, SqlExpression value, IContext context) {
			var variable = FindVariable(name, context);
			if (variable == null) {
				var type = value.ReturnType(context);
				variable = new Variable(name, type);
				variables.Add(new ObjectName(name),  variable);
			}

			return variable.SetValue(value, context);
		}

		private Variable FindVariable(string name, IContext context) {
			// TODO: find out the configurations and see if it is ignoreCase
			return ResolveVariable(name, true);
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