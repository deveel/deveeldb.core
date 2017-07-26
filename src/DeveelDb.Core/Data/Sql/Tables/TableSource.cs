using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Diagnostics;
using Deveel.Data.Indexes;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Types;
using Deveel.Data.Storage;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables {
	public sealed class TableSource : ITableSource {
		private bool isClosed;

		private FixedRecordList recordList;
		private long indexHeaderOffset;
		private long listHeaderOffset;
		private IArea headerArea;
		private long firstDeleteChainRecord;

		private IndexSetStore indexSetStore;

		private VersionedTableEventRegistry registries;

		private long sequenceId;

		private int lockCount;

		public TableSource(IContext systemContext, IStoreSystem storeSystem, int tableId, string sourceName) {
			SystemContext = systemContext;
			TableId = tableId;
			SourceName = sourceName;

			StoreSystem = storeSystem;

			GC = new TableSourceGC(this);

			FieldCache = systemContext.Scope.Resolve<ITableFieldCache>();
			Configuration = systemContext.Scope.Resolve<IConfiguration>();
			LobManager = systemContext.Scope.Resolve<ILargeObjectManager>();

			StoreIdentity = MakeSourceIdentity(systemContext, tableId, sourceName);
		}

		public int TableId { get; private set; }

		public string SourceName { get; }

		public IConfiguration Configuration { get; }

		public IContext SystemContext { get; }

		private TableSourceGC GC { get; }

		public TableInfo TableInfo { get; private set; }

		public IStoreSystem StoreSystem { get; }

		public IStore Store { get; private set; }

		public string StoreIdentity { get; }

		public bool HasShutdown { get; private set; }

		public ILargeObjectManager LobManager { get; }

		private ITableFieldCache FieldCache { get; }

		private string InsertCounterKey { get; set; }

		private string UpdateCounterKey { get; set; }

		private string DeleteCounterKey { get; set; }

		private string FileHitsCounterKey { get; set; }


		public bool HasChangesPending {
			get {
				lock (this) {
					return registries.HasChanges;
				}
			}
		}


		public long RawRowCount {
			get {
				lock (recordList) {
					return recordList.NodeCount;
				}
			}
		}

		public bool IsClosed {
			get {
				lock (this) {
					return isClosed;
				}
			}
			private set {
				lock (this) {
					isClosed = value;
				}
			}
		}

		public bool IsRootLocked {
			get {
				lock (this) {
					return lockCount > 0;
				}
			}
		}


		private void ClearLocks() {
			lock (this) {
				lockCount = 0;
			}
		}

		public void AddLock() {
			lock (this) {
				// TODO: Emit the stat to the system
				++lockCount;
			}
		}

		public void RemoveLock() {
			lock (this) {
				if (!isClosed) {
					// TODO: Emit the event to the system

					if (lockCount == 0)
						throw new InvalidOperationException("Too many root locks removed!");

					--lockCount;

					// If the last Lock is removed, schedule a possible collection.
					if (lockCount == 0)
						CheckForCleanup();
				}
			}
		}

		private void CheckForCleanup() {
			lock (this) {
				GC.Collect(false);
			}
		}

		private static string MakeSourceIdentity(IContext context, int tableId, string tableName) {
			string str = tableName.Replace('.', '_').ToLower();

			// Go through each character and remove each non a-z,A-Z,0-9,_ character.
			// This ensure there are no strange characters in the file name that the
			// underlying OS may not like.
			var osifiedName = new StringBuilder();
			int count = 0;
			for (int i = 0; i < str.Length || count > 64; ++i) {
				char c = str[i];
				if ((c >= 'a' && c <= 'z') ||
				    (c >= 'A' && c <= 'Z') ||
				    (c >= '0' && c <= '9') ||
				    c == '_') {
					osifiedName.Append(c);
					++count;
				}
			}

			return $"{tableId}_{osifiedName}";
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

		public long GetCurrentUniqueId() {
			lock (recordList) {
				return sequenceId - 1;
			}
		}

		public void SetUniqueId(long value) {
			lock (recordList) {
				sequenceId = value;
				if (HasShutdown)
					throw new Exception("IO operation while shutting down.");

				try {
					try {
						Store.Lock();
						headerArea.Position = 4 + 4;
						headerArea.Write(sequenceId);
						headerArea.Flush();
					} finally {
						Store.Unlock();
					}
				} catch (IOException e) {
					throw new InvalidOperationException("IO Error: " + e.Message, e);
				}
			}
		}

		public long GetNextUniqueId() {
			lock (recordList) {
				long v = sequenceId;
				++sequenceId;
				if (HasShutdown)
					throw new Exception("IO operation while shutting down.");

				try {
					try {
						Store.Lock();
						headerArea.Position = 4 + 4;
						headerArea.Write(sequenceId);
						headerArea.Flush();
					} finally {
						Store.Unlock();
					}
				} catch (IOException e) {
					throw new InvalidOperationException("IO Error: " + e.Message);
				}

				return v;
			}
		}

		public IMutableTable CreateTableAtCommit(ITransaction transaction) {
			throw new NotImplementedException();
		}

		public RecordState ReadRecordState(long rowNumber) {
			lock (recordList) {
				// Find the record entry input the block list.
				var blockArea = recordList.GetRecord(rowNumber);
				// Get the status.
				return (RecordState)blockArea.ReadInt32();
			}
		}

		public RecordState WriteRecordState(long rowNumber, RecordState state) {
			lock (recordList) {
				if (HasShutdown)
					throw new IOException("IO operation while shutting down.");

				// Find the record entry input the block list.
				var blockArea = recordList.GetRecord(rowNumber);
				var pos = blockArea.Position;
				// Get the status.
				var oldStatus = (RecordState)blockArea.ReadInt32();

				// Write the new status
				try {
					Store.Lock();

					blockArea.Position = pos;
					blockArea.Write((int)state);
					blockArea.Flush();
				} finally {
					Store.Unlock();
				}

				return oldStatus;
			}
		}

		private bool IsRecordDeleted(int rowNumber) {
			var state = ReadRecordState(rowNumber);
			return state == RecordState.Deleted;
		}


		internal void HardRemoveRow(int rowIndex) {
			lock (this) {
				// ASSERTION: We are not under a root Lock.
				if (IsRootLocked)
					throw new InvalidOperationException("Cannot remove row, table is locked");

				var typeKey = ReadRecordState(rowIndex);
				// Check this record is marked as committed removed.
				if (typeKey != RecordState.CommittedRemoved)
					throw new InvalidOperationException($"The row {rowIndex} is not marked as committed removed");

				DoHardRowRemove(rowIndex);
			}
		}

		private void DoHardRowRemove(int rowNumber) {
			lock (this) {
				// Internally delete the row,
				OnDeleteRow(rowNumber);

				// Update stats
				SystemContext.RegisterEvent(new CounterEvent(DeleteCounterKey));
			}
		}

		private void OnDeleteRow(int rowIndex) {
			lock (recordList) {
				if (HasShutdown)
					throw new IOException("IO operation while shutting down.");

				// Find the record entry input the block list.
				var blockArea = recordList.GetRecord(rowIndex);
				var p = blockArea.Position;
				var status = (RecordState)blockArea.ReadInt32();

				// Check it is not already deleted
				if (status == RecordState.Deleted)
					throw new IOException("Record is already marked as deleted.");

				long recordPointer = blockArea.ReadInt64();

				// Update the status record.
				try {
					Store.Lock();

					blockArea.Position = p;
					blockArea.Write((int)RecordState.Deleted);
					blockArea.Write(firstDeleteChainRecord);
					blockArea.Flush();
					firstDeleteChainRecord = rowIndex;

					// Update the first_delete_chain_record field input the header
					recordList.WriteDeleteHead(firstDeleteChainRecord);

					// If the record contains any references to blobs, remove the reference
					// here.
					ReleaseRowObjects(recordPointer);

					// Free the record from the store
					Store.DeleteArea(recordPointer);
				} finally {
					RemoveRowFromCache(rowIndex);
					Store.Unlock();
				}
			}
		}

		private void RemoveRowFromCache(int rowIndex) {
			if (FieldCache != null) {
				var colCount = TableInfo.Columns.Count;
				for (int i = 0; i < colCount; i++) {
					FieldCache.Remove(TableId, rowIndex, i);
				}
			}
		}

		private void ReleaseRowObjects(long recordPointer) {
			// NOTE: Does this need to be optimized?
			IArea recordArea = Store.GetArea(recordPointer);
			recordArea.ReadInt32();  // reserved

			// Look for any blob references input the row
			foreach (ColumnInfo column in TableInfo.Columns) {
				int ctype = recordArea.ReadInt32();
				int cellOffset = recordArea.ReadInt32();

				if (ctype == 1) {
					// Type 1 is not a large object
				} else if (ctype == 2) {
					var curP = recordArea.Position;
					recordArea.Position = cellOffset + 4 + (TableInfo.Columns.Count * 8);

					int btype = recordArea.ReadInt32();
					recordArea.ReadInt32();    // (reserved)

					if (btype == 0) {
						long blobRefId = recordArea.ReadInt64();

						// Release this reference
						LobManager.ReleaseObject(TableId, blobRefId);
					}

					// Revert the area pointer
					recordArea.Position = curP;
				} else {
					throw new Exception("Unrecognised type.");
				}
			}
		}

		internal bool HardCheckAndReclaimRow(int recordIndex) {
			lock (this) {
				// ASSERTION: We are not under a root Lock.
				if (IsRootLocked)
					throw new InvalidOperationException("Assertion failed: Can't remove row, table is under a root Lock.");

				// Row already deleted?
				if (IsRecordDeleted(recordIndex))
					return false;

				var typeKey = ReadRecordState(recordIndex);

				// Check this record is marked as committed removed.
				if (typeKey != RecordState.CommittedRemoved)
					return false;

				DoHardRowRemove(recordIndex);
				return true;
			}
		}

		private async Task<bool> OpenTableAsync() {
			// Open the store.
			Store = await StoreSystem.OpenStoreAsync(StoreIdentity, Configuration);
			bool needCheck = Store.State == StoreState.Broken;

			// Setup the list structure
			recordList = new FixedRecordList(Store, 12);

			// Read and setup the pointers
			ReadStoreHeaders();

			return needCheck;
		}

		private void ReadStoreHeaders() {
			// Read the fixed header
			var fixedArea = Store.GetArea(-1);

			// Set the header area
			headerArea = Store.GetArea(fixedArea.ReadInt64());

			// Open a stream to the header
			var version = headerArea.ReadInt32();              // version
			if (version != 1)
				throw new IOException("Incorrect version identifier.");

			TableId = headerArea.ReadInt32();                  // table_id
			sequenceId = headerArea.ReadInt64();               // sequence id
			long infoPointer = headerArea.ReadInt64();         // pointer to TableInfo
			long indexInfoPointer = headerArea.ReadInt64();    // pointer to IndexSetInfo
			indexHeaderOffset = headerArea.ReadInt64();       // pointer to index header
			listHeaderOffset = headerArea.ReadInt64();        // pointer to list header

			// Read the table info
			using (var stream = new AreaStream(Store.GetArea(infoPointer))) {
				var reader = new BinaryReader(stream, Encoding.Unicode);
				version = reader.ReadInt32();
				if (version != 1)
					throw new IOException("Incorrect TableInfo version identifier.");

				var userTypeResolver = SystemContext.Scope.Resolve<ISqlTypeResolver>();
				TableInfo = ReadTableInfo(stream, userTypeResolver);
			}

			// Read the data index set info
			using (var stream = new AreaStream(Store.GetArea(indexInfoPointer))) {
				var reader = new BinaryReader(stream, Encoding.Unicode);
				version = reader.ReadInt32();
				if (version != 1)
					throw new IOException("Incorrect IndexSetInfo version identifier.");

				IndexSetInfo = ReadIndexSetInfo(stream);
			}

			// Read the list header
			recordList.Open(listHeaderOffset);
			firstDeleteChainRecord = recordList.ReadDeleteHead();

			// Init the index store
			indexSetStore = new IndexSetStore(Store);
			try {
				indexSetStore.Open(indexHeaderOffset);
			} catch (IOException) {
				// If this failed try writing output a new empty index set.
				// ISSUE: Should this occur here?  This is really an attempt at repairing
				//   the index store.
				indexSetStore = new IndexSetStore(Store);
				indexHeaderOffset = indexSetStore.Create();
				indexSetStore.PrepareIndexes(TableInfo.Columns.Count + 1, 1, 1024);
				headerArea.Position = 32;
				headerArea.Write(indexHeaderOffset);
				headerArea.Position = 0;
				headerArea.Flush();
			}
		}

		private TableInfo ReadTableInfo(Stream stream, ISqlTypeResolver typeResolver) {
			var reader = new BinaryReader(stream, Encoding.Unicode);
			return ReadTableInfo(reader, typeResolver);
		}

		private static TableInfo ReadTableInfo(BinaryReader reader, ISqlTypeResolver typeResolver) {
			var version = reader.ReadInt32();
			if (version != 3)
				throw new FormatException("Invalid version of the table info.");

			var catName = reader.ReadString();
			var schemName = reader.ReadString();
			var tableName = reader.ReadString();

			var objSchemaName = !String.IsNullOrEmpty(catName)
				? new ObjectName(new ObjectName(catName), schemName)
				: new ObjectName(schemName);

			var objTableName = new ObjectName(objSchemaName, tableName);

			var tableInfo = new TableInfo(objTableName);

			var colCount = reader.ReadInt32();
			for (int i = 0; i < colCount; i++) {
				var columnInfo = ReadColumnInfo(reader, typeResolver);

				if (columnInfo != null)
					tableInfo.Columns.Add(columnInfo);
			}

			return tableInfo;
		}

		private static ColumnInfo ReadColumnInfo(BinaryReader reader, ISqlTypeResolver typeResolver) {
			var version = reader.ReadInt32();
			if (version != 3)
				throw new FormatException("Invalid version of the Column-Info");

			var columnName = reader.ReadString();
			var columnType = SqlType.Deserialize(reader, typeResolver);

			var columnInfo = new ColumnInfo(columnName, columnType);

			var hasDefault = reader.ReadByte() == 1;
			if (hasDefault)
				columnInfo.DefaultValue = SqlExpression.Deserialize(reader);

			return columnInfo;
		}
	}
}