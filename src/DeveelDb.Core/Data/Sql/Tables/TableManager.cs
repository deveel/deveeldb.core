using System;
using System.Threading.Tasks;

using Deveel.Data.Diagnostics;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables {
	public sealed class TableManager : IDbObjectManager, IEventRegistry {
		private readonly IContext context;

		public TableManager(IContext context) {
			this.context = context;
		}

		~TableManager() {
			Dispose(false);
		}

		DbObjectType IDbObjectManager.ObjectType => DbObjectType.Table;

		Task IDbObjectManager.CreateObjectAsync(IDbObjectInfo objInfo) {
			throw new NotImplementedException();
		}

		Task<bool> IDbObjectManager.ObjectExistsAsync(ObjectName objName) {
			throw new NotImplementedException();
		}

		Task<IDbObjectInfo> IDbObjectManager.GetObjectInfoAsync(ObjectName objectName) {
			throw new NotImplementedException();
		}

		Task<IDbObject> IDbObjectManager.GetObjectAsync(ObjectName objName) {
			throw new NotImplementedException();
		}

		Task<bool> IDbObjectManager.AlterObjectAsync(IDbObjectInfo objInfo) {
			throw new NotImplementedException();
		}

		Task<bool> IDbObjectManager.DropObjectAsync(ObjectName objName) {
			throw new NotImplementedException();
		}

		public Task<ObjectName> ResolveNameAsync(ObjectName objName, bool ignoreCase) {
			throw new NotImplementedException();
		}

		private void Dispose(bool disposing) {
			
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		Type IEventRegistry.EventType => typeof(TransactionEvent);

		void IEventRegistry.Register(IEvent @event) {
			var te = (TransactionEvent) @event;
			if (te is TransactionBeginEvent) {
				// TODO: get all visible tables at this commit
			}
		}
	}
}