using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Deveel.Data.Storage;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	class TableStateStore : IDisposable {
		private IStore store;

		private IArea headerArea;

		private long delAreaPointer;
		private List<TableStateInfo> deleteList;
		private bool delListChange;

		private long visAreaPointer;
		private List<TableStateInfo> visibleList;
		private bool visListChange;

		private int currentTableId;

		private const int Magic = 0x0BAC8001;


		public TableStateStore(IStore store) {
			this.store = store;
		}

		~TableStateStore() {
			Dispose(false);
		}

		public ICollection<TableStateInfo> VisibleTables {
			get {
				lock (this) {
					return visibleList.AsReadOnly();
				}
			}
		}

		public ICollection<TableStateInfo> DeletedResources {
			get {
				lock (this) {
					return deleteList.AsReadOnly();
				}
			}
		}


		private void ReadResources(IList<TableStateInfo> list, long pointer) {
			using (var stream = new AreaStream(store.GetArea(pointer, true))) {
				using (var reader = new BinaryReader(stream, Encoding.Unicode)) {
					reader.ReadInt32(); // version

					int count = (int) reader.ReadInt64();
					for (int i = 0; i < count; ++i) {
						var tableId = reader.ReadInt64();
						var name = reader.ReadString();
						var systemId = reader.ReadString();

						list.Add(new TableStateInfo((int) tableId, name, systemId));
					}
				}
			}
		}

		private static byte[] WriteResources(IList<TableStateInfo> list) {
			using (var stream = new MemoryStream()) {
				using (var writer = new BinaryWriter(stream, Encoding.Unicode)) {
					writer.Write(1); // version

					int sz = list.Count;
					writer.Write((long) sz);

					foreach (var state in list) {
						writer.Write((long) state.Id);
						writer.Write(state.SourceName);
						writer.Write(state.SystemId);
					}

					writer.Flush();

					return stream.ToArray();
				}
			}
		}

		private long WriteListToStore(IList<TableStateInfo> list) {
			var bytes = WriteResources(list);

			using (var area = store.CreateArea(bytes.Length)) {
				long listP = area.Id;
				area.Write(bytes, 0, bytes.Length);
				area.Flush();

				return listP;
			}
		}

		public long Create() {
			lock (this) {
				// Allocate empty visible and deleted tables area
				using (var visTablesArea = store.CreateArea(12)) {
					using (var delTablesArea = store.CreateArea(12)) {
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
						using (var headerWriter = store.CreateArea(32)) {
							long headerP = headerWriter.Id;
							headerWriter.Write(Magic);
							headerWriter.Write(0);
							headerWriter.Write(0L);
							headerWriter.Write(visAreaPointer);
							headerWriter.Write(delAreaPointer);
							headerWriter.Flush();

							headerArea = store.GetArea(headerP, false);

							// Reset currentTableId
							currentTableId = 0;

							visibleList = new List<TableStateInfo>();
							deleteList = new List<TableStateInfo>();

							// Return pointer to the header area
							return headerP;
						}
					}
				}
			}
		}

		public void Open(long offset) {
			lock (this) {
				headerArea = store.GetArea(offset);
				int magicValue = headerArea.ReadInt32();
				if (magicValue != Magic)
					throw new IOException("Magic value for state header area is incorrect.");

				if (headerArea.ReadInt32() != 0)
					throw new IOException("Unknown version for state header area.");

				currentTableId = (int) headerArea.ReadInt64();
				visAreaPointer = headerArea.ReadInt64();
				delAreaPointer = headerArea.ReadInt64();

				// Setup the visible and delete list
				visibleList = new List<TableStateInfo>();
				deleteList = new List<TableStateInfo>();

				// Read the resource list for the visible and delete list.
				ReadResources(visibleList, visAreaPointer);
				ReadResources(deleteList, delAreaPointer);
			}
		}

		public int NextTableId() {
			lock (this) {
				int curCounter = currentTableId;
				++currentTableId;

				try {
					store.Lock();

					// Update the state in the file
					headerArea.Position = 8;
					headerArea.Write((long) currentTableId);

					// Check out the change
					headerArea.Flush();
				} finally {
					store.Unlock();
				}

				return curCounter;
			}
		}

		public void AddVisibleResource(TableStateInfo resource) {
			lock (this) {
				visibleList.Add(resource);
				visListChange = true;
			}
		}

		public void AddDeleteResource(TableStateInfo resource) {
			lock (this) {
				deleteList.Add(resource);
				delListChange = true;
			}
		}

		private static void RemoveState(IList<TableStateInfo> list, String name) {
			int sz = list.Count;

			for (int i = 0; i < sz; ++i) {
				var state = list[i];
				if (name.Equals(state.SourceName)) {
					list.RemoveAt(i);
					return;
				}
			}

			throw new Exception($"Couldn't find resource '{name}' in list.");
		}

		public void RemoveVisibleResource(string name) {
			lock (this) {
				RemoveState(visibleList, name);
				visListChange = true;
			}
		}

		public void RemoveDeleteResource(string name) {
			lock (this) {
				RemoveState(deleteList, name);
				delListChange = true;
			}
		}

		public void Flush() {
			lock (this) {
				bool changes = false;
				long newVisP = visAreaPointer;
				long newDelP = delAreaPointer;

				try {
					store.Lock();

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
							store.DeleteArea(visAreaPointer);
							visAreaPointer = newVisP;
						}

						if (delAreaPointer != newDelP) {
							store.DeleteArea(delAreaPointer);
							delAreaPointer = newDelP;
						}
					}
				} finally {
					store.Unlock();
				}
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
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

			visibleList = null;
			deleteList = null;
			headerArea = null;
			store = null;
		}
	}
}