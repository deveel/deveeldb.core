using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Deveel.Data.Storage;

namespace Deveel.Data {
	class TableStateStore : IDisposable {
		private IArea headerArea;

		private long delAreaPointer;
		private List<TableState> deleteList;
		private bool delListChange;

		private long visAreaPointer;
		private List<TableState> visibleList;
		private bool visListChange;

		private long currentTableId;

		private const int Magic = 0x0BAC8001;

		public TableStateStore(IStore store) {
			if (store == null)
				throw new ArgumentNullException(nameof(store));

			Store = store;
		}

		~TableStateStore() {
			Dispose(false);
		}

		public IStore Store { get; private set; }

		public IEnumerable<TableState> VisibleTables {
			get {
				lock (this) {
					return visibleList.AsEnumerable();
				}
			}
		}

		public IEnumerable<TableState> DeletedTables {
			get {
				lock (this) {
					return deleteList.AsEnumerable();
				}
			}
		}

		private void ReadStateResourceList(IList<TableState> list, long pointer) {
			using (var reader = new BinaryReader(Store.GetAreaInputStream(pointer), Encoding.Unicode)) {
				reader.ReadInt32(); // version

				int count = (int)reader.ReadInt64();
				for (int i = 0; i < count; ++i) {
					long tableId = reader.ReadInt64();
					string name = reader.ReadString();

					list.Add(new TableState((int)tableId, name));
				}
			}
		}

		private static byte[] SerializeResources(IList<TableState> list) {
			using (var stream = new MemoryStream()) {
				using (var writer = new BinaryWriter(stream, Encoding.Unicode)) {
					writer.Write(1); // version
					int sz = list.Count;
					writer.Write((long)sz);
					foreach (var state in list) {
						writer.Write((long)state.TableId);
						writer.Write(state.SourceName);
					}

					writer.Flush();

					return stream.ToArray();
				}
			}
		}

		private long WriteListToStore(IList<TableState> list) {
			var bytes = SerializeResources(list);

			using (var area = Store.CreateArea(bytes.Length)) {
				long listP = area.Id;
				area.Write(bytes, 0, bytes.Length);
				area.Flush();

				return listP;
			}
		}

		public long Create() {
			lock (this) {
				// Allocate empty visible and deleted tables area
				using (var visTablesArea = Store.CreateArea(12)) {
					using (var delTablesArea = Store.CreateArea(12)) {
						visAreaPointer = visTablesArea.Id;
						delAreaPointer = delTablesArea.Id;

						// Write empty entries for both of these
						visTablesArea.Write(1);
						visTablesArea.Write(0L);
						visTablesArea.Flush();
						delTablesArea.Write(1);
						delTablesArea.Write(0L);
						delTablesArea.Flush();

						// Now allocate an empty state header
						using (var headerWriter = Store.CreateArea(32)) {
							long headerP = headerWriter.Id;
							headerWriter.Write(Magic);
							headerWriter.Write(0);
							headerWriter.Write(0L);
							headerWriter.Write(visAreaPointer);
							headerWriter.Write(delAreaPointer);
							headerWriter.Flush();

							headerArea = Store.GetArea(headerP, false);

							// Reset currentTableId
							currentTableId = 0;

							visibleList = new List<TableState>();
							deleteList = new List<TableState>();

							// Return pointer to the header area
							return headerP;
						}
					}
				}
			}
		}

		public void Open(long offset) {
			lock (this) {
				headerArea = Store.GetArea(offset);
				int magicValue = headerArea.ReadInt32();
				if (magicValue != Magic)
					throw new IOException("Magic value for state header area is incorrect.");

				if (headerArea.ReadInt32() != 0)
					throw new IOException("Unknown version for state header area.");

				currentTableId = (int)headerArea.ReadInt64();
				visAreaPointer = headerArea.ReadInt64();
				delAreaPointer = headerArea.ReadInt64();

				// Setup the visible and delete list
				visibleList = new List<TableState>();
				deleteList = new List<TableState>();

				// Read the table list for the visible and delete list.
				ReadStateResourceList(visibleList, visAreaPointer);
				ReadStateResourceList(deleteList, delAreaPointer);
			}
		}

		public int NextTableId() {
			lock (this) {
				var curCounter = currentTableId;
				++currentTableId;

				try {
					Store.Lock();

					// Update the state in the file
					headerArea.Position = 8;
					headerArea.Write(currentTableId);

					// Check out the change
					headerArea.Flush();
				} finally {
					Store.Unlock();
				}

				return (int) curCounter;
			}
		}

		private static void RemoveState(IList<TableState> list, string name) {
			int sz = list.Count;
			for (int i = 0; i < sz; ++i) {
				var state = list[i];
				if (name.Equals(state.SourceName)) {
					list.RemoveAt(i);
					return;
				}
			}
			throw new Exception("Couldn't find resource '" + name + "' in list.");
		}

		public void AddVisibleTable(TableState table) {
			lock (this) {
				visibleList.Add(table);
				visListChange = true;
			}
		}

		public void RemoveVisibleTable(string name) {
			lock (this) {
				RemoveState(visibleList, name);
				visListChange = true;
			}
		}


		public void AddDeletedTable(TableState table) {
			lock (this) {
				deleteList.Add(table);
				delListChange = true;
			}
		}

		public void RemoveDeletedTable(string name) {
			lock (this) {
				RemoveState(deleteList, name);
				delListChange = true;
			}
		}

		public async Task FlushAsync() {
			lock (this) {
				bool changes = false;
				long newVisP = visAreaPointer;
				long newDelP = delAreaPointer;

				try {
					Store.Lock();

					// If the lists changed, then Write new state areas to the store.
					if (visListChange) {
						newVisP = WriteListToStore(visibleList);
						visListChange = false;
						changes = true;
					}
					if (delListChange) {
						newDelP = WriteListToStore(deleteList);
						delListChange = false;
						changes = true;
					}

					// Commit the changes,
					if (changes) {
						headerArea.Position = 16;
						headerArea.Write(newVisP);
						headerArea.Write(newDelP);
						headerArea.Flush();

						if (visAreaPointer != newVisP) {
							Store.DeleteArea(visAreaPointer);
							visAreaPointer = newVisP;
						}

						if (delAreaPointer != newDelP) {
							Store.DeleteArea(delAreaPointer);
							delAreaPointer = newDelP;
						}
					}
				} finally {
					Store.Unlock();
				}
			}
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (headerArea != null)
					headerArea.Dispose();

				if (visibleList != null)
					visibleList.Clear();

				if (deleteList != null)
					deleteList.Clear();
			}

			headerArea = null;
			Store = null;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}