using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Diagnostics;
using Deveel.Data.Indexes;
using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Indexes;
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

		private short sRunFileHits = Int16.MaxValue;

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

		public IIndexSet<SqlObject, long> CreateIndexSet() {
			return indexSetStore.GetSnapshotIndex();
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
				return (RecordState) blockArea.ReadInt32();
			}
		}

		public void BuildIndexes() {
			lock (this) {
				var indexSet = CreateIndexSet();

				var indexSetInfo = IndexSetInfo;

				var rowCount = RawRowCount;

				// Master index is always on index position 0
				var masterIndex = indexSet.GetIndex(0);

				// First, update the master index
				for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex) {
					// If this row isn't deleted, set the index information for it,
					if (!IsRecordDeleted(rowIndex)) {
						// First add to master inde
						if (!masterIndex.InsertSort(rowIndex))
							throw new Exception("Master index entry was duplicated.");
					}
				}

				// Commit the master index
				CommitIndexSet(indexSet);

				// Now go ahead and build each index in this table
				int indexCount = indexSetInfo.IndexCount;
				for (int i = 0; i < indexCount; ++i) {
					BuildIndex(i);
				}
			}
		}

		private void BuildIndex(int offset) {
			lock (this) {
				var indexSet = CreateIndexSet();

				// Master index is always on index position 0
				var masterIndex = indexSet.GetIndex(0);

				// A minimal ITable for constructing the indexes
				var minTable = new MinimalTable(this, masterIndex);

				// Set up schemes for the index,
				var index = CreateColumnIndex(indexSet, minTable, offset);

				// Rebuild the entire index
				var rowCount = RawRowCount;
				for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex) {
					// If this row isn't deleted, set the index information for it,
					if (!IsRecordDeleted(rowIndex))
						index.Insert(rowIndex);
				}

				// Commit the index
				CommitIndexSet(indexSet);
			}
		}

		private Index CreateColumnIndex(IIndexSet<SqlObject, long> indexSet, ITable table, int columnOffset) {
			lock (this) {
				var column = TableInfo.Columns[columnOffset];
				if (!column.ColumnType.IsIndexable) {
					var indexInfo = new IndexInfo(FormColumnIndexName(table, column), table.TableInfo.TableName, column.ColumnName);
					var index = new BlindSearchIndex(indexInfo);
					index.AttachTo(table);
					return index;
				} else {
					var indexInfo = FindIndexForColumns(table, new[] {column.ColumnName});
					return CreateIndexAt(indexSet, table, indexInfo);
				}
			}
		}

		private ObjectName FormColumnIndexName(ITable table, ColumnInfo column) {
			var tableName = table.TableInfo.TableName.Name;
			var schemaName = table.TableInfo.TableName.Parent;
			var indexName = $"{tableName}_{column.ColumnName}_IDX";
			return new ObjectName(schemaName, indexName);
		}

		private Index CreateIndexAt(IIndexSet<SqlObject, long> indexSet, ITable table, IndexInfo indexInfo) {
			lock (this) {
				try {
					// Get the Index object
					Index index = SystemContext.GetIndex(indexInfo.IndexName);

					if (index != null)
						return index;

					string[] cols = indexInfo.ColumnNames;
					var tableInfo = TableInfo;
					if (cols.Length == 1) {
						// If a single column
						var colIndex = tableInfo.Columns.IndexOf(cols[0]);

						if (indexInfo.IndexType.Equals(DefaultIndexTypes.InsertSearch)) {
							var rows = indexSet.GetIndex(colIndex);

							// Get the index from the index set and set up the new InsertSearch
							// scheme.
							index = new InsertSearchIndex(indexInfo, rows);
							index.AttachTo(table);
							return index;
						}
					}

					return SystemContext.CreateIndex(indexInfo);
				} catch (Exception ex) {
					throw new InvalidOperationException(
						$"An error occurred while creating a colummn for table {table.TableInfo.TableName}", ex);
				}
			}
		}

		private IndexInfo FindIndexForColumns(ITable table, string[] columnNames) {
			return SystemContext.FindIndex(table.TableInfo.TableName, columnNames);
		}

		private void CommitIndexSet(IIndexSet<SqlObject, long> indexSet) {
			throw new NotImplementedException();
		}

		public RecordState WriteRecordState(long rowNumber, RecordState state) {
			lock (recordList) {
				if (HasShutdown)
					throw new IOException("IO operation while shutting down.");

				// Find the record entry input the block list.
				var blockArea = recordList.GetRecord(rowNumber);
				var pos = blockArea.Position;
				// Get the status.
				var oldStatus = (RecordState) blockArea.ReadInt32();

				// Write the new status
				try {
					Store.Lock();

					blockArea.Position = pos;
					blockArea.Write((int) state);
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
				SystemContext.Increment(DeleteCounterKey);
			}
		}

		private void OnDeleteRow(int rowIndex) {
			lock (recordList) {
				if (HasShutdown)
					throw new IOException("IO operation while shutting down.");

				// Find the record entry input the block list.
				var blockArea = recordList.GetRecord(rowIndex);
				var p = blockArea.Position;
				var status = (RecordState) blockArea.ReadInt32();

				// Check it is not already deleted
				if (status == RecordState.Deleted)
					throw new IOException("Record is already marked as deleted.");

				long recordPointer = blockArea.ReadInt64();

				// Update the status record.
				try {
					Store.Lock();

					blockArea.Position = p;
					blockArea.Write((int) RecordState.Deleted);
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
			recordArea.ReadInt32(); // reserved

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
					recordArea.ReadInt32(); // (reserved)

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
			var version = headerArea.ReadInt32(); // version
			if (version != 1)
				throw new IOException("Incorrect version identifier.");

			TableId = headerArea.ReadInt32(); // table_id
			sequenceId = headerArea.ReadInt64(); // sequence id
			long infoPointer = headerArea.ReadInt64(); // pointer to TableInfo
			long indexInfoPointer = headerArea.ReadInt64(); // pointer to IndexSetInfo
			indexHeaderOffset = headerArea.ReadInt64(); // pointer to index header
			listHeaderOffset = headerArea.ReadInt64(); // pointer to list header

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

		public TableIndexSetInfo IndexSetInfo { get; private set; }

		private TableIndexSetInfo ReadIndexSetInfo(Stream stream) {
			var reader = new BinaryReader(stream, Encoding.Unicode);

			var version = reader.ReadInt32();
			if (version != 2)
				throw new FormatException("Invalid version number of the Index-Set Info");

			var catName = reader.ReadString();
			var schemaName = reader.ReadString();
			var tableName = reader.ReadString();

			ObjectName objSchemaName;
			if (String.IsNullOrEmpty(catName)) {
				objSchemaName = new ObjectName(schemaName);
			} else {
				objSchemaName = new ObjectName(new ObjectName(catName), schemaName);
			}

			var objTableName = new ObjectName(objSchemaName, tableName);

			var indexCount = reader.ReadInt32();

			var indices = new List<TableIndexInfo>();
			for (int i = 0; i < indexCount; i++) {
				var indexInfo = ReadIndexInfo(stream);
				indices.Add(indexInfo);
			}

			return new TableIndexSetInfo(objTableName, indices.ToArray(), false);
		}

		private TableIndexInfo ReadIndexInfo(Stream stream) {
			var reader = new BinaryReader(stream, Encoding.Unicode);

			var version = reader.ReadInt32();
			if (version != 3)
				throw new FormatException("Invalid version number for Index-Info");

			var indexName = reader.ReadString();
			var offset = reader.ReadInt32();

			var colCount = reader.ReadInt32();

			var columnNames = new string[colCount];
			for (int i = 0; i < colCount; i++) {
				var columnName = reader.ReadString();
				columnNames[i] = columnName;
			}

			return new TableIndexInfo(ObjectName.Parse(indexName), offset);
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
				columnInfo.DefaultValue =
					(SqlExpression) new BinarySerializer().Deserialize(typeof(SqlExpression), reader.BaseStream);

			return columnInfo;
		}

		public SqlObject GetValue(long rowIndex, int columnOffset) {
			// NOTES:
			// This is called *A LOT*.  It's a key part of the 20% of the program
			// that's run 80% of the time.
			// This performs very nicely for rows that are completely contained within
			// 1 sector.  However, rows that contain large cells (eg. a large binary
			// or a large string) and spans many sectors will not be utilizing memory
			// as well as it could.
			// The reason is because all the data for a row is Read from the store even
			// if only 1 cell of the column is requested.  This will have a big
			// impact on column scans and searches.  The cell cache takes some of this
			// performance bottleneck away.
			// However, a better implementation of this method is made difficult by
			// the fact that sector spans can be compressed.  We should perhaps
			// revise the low level data storage so only sectors can be compressed.

			// First check if this is within the cache before we continue.
			SqlObject cell;
			if (CellCaching) {
				if (CellCache.TryGetValue(Database.Name, TableId, rowIndex, columnOffset, out cell))
					return cell;
			}

			// We maintain a cache of byte[] arrays that contain the rows Read input
			// from the file.  If consecutive reads are made to the same row, then
			// this will cause lots of fast cache hits.

			long recordPointer = -1;
			try {
				lock (recordList) {
					// Increment the file hits counter
					++sRunFileHits;

					if (sRunFileHits >= 100) {
						SystemContext.Count(FileHitsCounterKey, sRunFileHits);
						sRunFileHits = 0;
					}

					// Get the node for the record
					var listBlock = recordList.GetRecord(rowIndex);
					var status = (RecordState)listBlock.ReadInt32();
					// Check it's not deleted
					if (status == RecordState.Deleted)
						throw new InvalidOperationException(String.Format("Record {0} was deleted: unable to read.", rowIndex));

					// Get the pointer to the record we are reading
					recordPointer = listBlock.ReadInt64();
				}

				// Open a stream to the record
				using (var stream = new AreaStream(Store.GetArea(recordPointer))) {
					var reader = new BinaryReader(stream);

					stream.Seek(4 + (columnOffset * 8), SeekOrigin.Current);

					int cellType = reader.ReadInt32();
					int cellOffset = reader.ReadInt32();

					int curAt = 8 + 4 + (columnOffset * 8);
					int beAt = 4 + (TableInfo.Columns.Count * 8);
					int skipAmount = (beAt - curAt) + cellOffset;

					stream.Seek(skipAmount, SeekOrigin.Current);

					// Get the TType for this column
					// NOTE: It's possible this call may need optimizing?
					var type = TableInfo.Columns[columnOffset].ColumnType;

					ISqlValue ob;
					if (cellType == 1) {
						// If standard object type
						ob = type.DeserializeObject(stream);
					} else if (cellType == 2) {
						// If reference to a blob input the BlobStore
						int fType = reader.ReadInt32();
						int fReserved = reader.ReadInt32();
						long refId = reader.ReadInt64();

						if (fType == 0) {
							// Resolve the reference
							var objRef = ObjectStore.GetObject(refId);
							ob = type.CreateFromLargeObject(objRef);
						} else if (fType == 1) {
							ob = null;
						} else {
							throw new Exception("Unknown blob type.");
						}
					} else {
						throw new Exception("Unrecognised cell type input data.");
					}

					// Wrap it around a TObject
					cell = new SqlObject(type, ob);

					// And close the reader.
					reader.Dispose();
				}
			} catch (IOException e) {
				throw new Exception(String.Format("Error getting cell at ({0}, {1}) pointer = " + recordPointer + ".", rowIndex,
					columnOffset), e);
			}

			// And write input the cache and return it.
			if (CellCaching) {
				CellCache.Set(Database.Name, TableId, rowIndex, columnOffset, cell);
			}

			return cell;
		}

		#region MinimalTable

		class MinimalTable : ITable {
			private TableSource source;
			private IIndex<SqlObject, long> masterIndex;

			public MinimalTable(TableSource source, IIndex<SqlObject, long> masterIndex) {
				this.source = source;
				this.masterIndex = masterIndex;
			}

			public IEnumerator<Row> GetEnumerator() {
				// NOTE: Returns iterator across master index before journal entry
				//   changes.
				var iterator = masterIndex.GetEnumerator();
				// Wrap it around a IRowEnumerator object.
				return new RowEnumerator(this, iterator);
			}

			private class RowEnumerator : IEnumerator<Row> {
				private MinimalTable table;
				private IIndexEnumerator<long> enumerator;

				public RowEnumerator(MinimalTable table, IIndexEnumerator<long> enumerator) {
					this.table = table;
					this.enumerator = enumerator;
				}

				~RowEnumerator() {
					Dispose(false);
				}

				private void Dispose(bool disposing) {
					table = null;
					enumerator = null;

				}

				public void Dispose() {
					Dispose(true);
					System.GC.SuppressFinalize(this);
				}

				public bool MoveNext() {
					return enumerator.MoveNext();
				}

				public void Reset() {
					enumerator.Reset();
				}

				public Row Current {
					get { return new Row(table, enumerator.Current); }
				}

				object IEnumerator.Current {
					get { return Current; }
				}
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			public TableInfo TableInfo {
				get { return source.TableInfo; }
			}

			IDbObjectInfo IDbObject.ObjectInfo {
				get { return TableInfo; }
			}

			public long RowCount {
				get {
					// NOTE: Returns the number of rows in the master index before journal
					//   entries have been made.
					return masterIndex.Count;
				}
			}

			public Task<SqlObject> GetValueAsync(long rowNumber, int columnOffset) {
				return source.GetValue(rowNumber, columnOffset);
			}

			public Index GetColumnIndex(int column) {
				throw new NotImplementedException();
			}

			int IComparable.CompareTo(object obj) {
				throw new NotSupportedException();
			}

			int IComparable<ISqlValue>.CompareTo(ISqlValue other) {
				throw new NotSupportedException();
			}

			void ISerializable.GetObjectData(SerializationInfo info) {
			}

			bool ISqlValue.IsComparableTo(ISqlValue other) {
				return false;
			}
		}

		#endregion
	}
}