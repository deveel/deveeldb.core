using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public class TransientTableManager : IDbObjectManager {
		private Dictionary<ObjectName, ITable> tables;
		private Dictionary<ObjectName, ObjectName> tableNames;

		public TransientTableManager() {
			tables = new Dictionary<ObjectName, ITable>();
			tableNames = new Dictionary<ObjectName, ObjectName>(ObjectNameComparer.IgnoreCase);
		}


		DbObjectType IDbObjectManager.ObjectType => DbObjectType.Table;

		void IDbObjectManager.CreateObject(IDbObjectInfo objInfo) {
			if (!(objInfo is TableInfo))
				throw new ArgumentException();

			CreateTable((TableInfo)objInfo);
		}

		public void CreateTable(TableInfo tableInfo) {
			if (tables.ContainsKey(tableInfo.TableName))
				throw new ArgumentException();

			var tempTable = new TemporaryTable(tableInfo);
			tables[tableInfo.TableName] = tempTable;
			tableNames[tableInfo.TableName] = tableInfo.TableName;
		}

		bool IDbObjectManager.RealObjectExists(ObjectName objName) {
			return TableExists(objName);
		}

		bool IDbObjectManager.ObjectExists(ObjectName objName) {
			return TableExists(objName);
		}

		public bool TableExists(ObjectName tableName) {
			return tables.ContainsKey(tableName);
		}

		IDbObject IDbObjectManager.GetObject(ObjectName objName) {
			return GetTable(objName);
		}

		public ITable GetTable(ObjectName tableName) {
			ITable table;
			if (!tables.TryGetValue(tableName, out table))
				return null;

			return table;
		}

		bool IDbObjectManager.AlterObject(IDbObjectInfo objInfo) {
			throw new NotImplementedException();
		}

		bool IDbObjectManager.DropObject(ObjectName objName) {
			return DropTable(objName);
		}

		public bool DropTable(ObjectName tableName) {
			ITable table;
			if (!tables.TryGetValue(tableName, out table))
				return false;

			tableNames.Remove(table.TableInfo.TableName);
			return tables.Remove(tableName);
		}

		ObjectName IDbObjectManager.ResolveName(ObjectName objName, bool ignoreCase) {
			return ResolveTableName(objName, ignoreCase);
		}

		public ObjectName ResolveTableName(ObjectName tableName, bool ignoreCase) {
			if (ignoreCase) {
				if (!tableNames.TryGetValue(tableName, out tableName))
					return null;
			}

			if (!tables.ContainsKey(tableName))
				return null;

			return tableName;
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (tables != null)
					tables.Clear();
				if (tableNames != null)
					tableNames.Clear();
			}

			tableNames = null;
			tables = null;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}