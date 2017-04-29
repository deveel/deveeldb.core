using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public sealed class TableEventHistory : IEnumerable<TableEvent> {
		private readonly List<TableEvent> events;

		public TableEventHistory(ITableSource source)
			: this(source, -1, new List<TableEvent>()) {
		}

		private TableEventHistory(ITableSource source, long commitId, List<TableEvent> events) {
			Table = source;
			CommitId = commitId;
			this.events = events;
		}

		public ITableSource Table { get; }

		public int TableId => Table.TableId;

		public long CommitId { get; }

		public ObjectName TableName => Table.TableInfo.TableName;

		public int EventCount => events.Count;

		internal void RegisterEvent(TableEvent @event) {
			lock (this) {
				events.Add(@event);
			}
		}

		public IList<long> AddedRows {
			get {
				lock (this) {
					var list = new List<long>();

					foreach (var tableEvent in events.OfType<TableRowEvent>()) {
						var eventType = tableEvent.EventType;
						if (eventType == TableRowEventType.Add ||
						    eventType == TableRowEventType.UpdateAdd) {
							list.Add(tableEvent.RowNumber);
						} else if (eventType == TableRowEventType.Remove ||
						           eventType == TableRowEventType.UpdateRemove) {
							var index = list.IndexOf(tableEvent.RowNumber);
							if (index != -1)
								list.RemoveAt(index);
						}
					}

					return list.ToArray();
				}
			}
		}

		public IList<long> RemovedRows {
			get {
				lock (this) {
					var list = new List<long>();

					foreach (var tableEvent in events.OfType<TableRowEvent>()) {
						if (tableEvent.EventType == TableRowEventType.Remove ||
						    tableEvent.EventType == TableRowEventType.UpdateRemove)
							list.Add(tableEvent.RowNumber);
					}

					return list;
				}
			}
		}

		public IEnumerator<TableEvent> GetEnumerator() {
			lock (this) {
				return events.GetEnumerator();
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Rollback(ITransaction transaction, int count) {
			lock (this) {
				if (count > events.Count)
					throw new Exception("Trying to rollback more events than are in the registry.");

				var toAdd = new List<long>();

				// Find all entries and added new rows to the table
				foreach (var tableEvent in events.OfType<TableRowEvent>()) {
					if (tableEvent.EventType == TableRowEventType.Add ||
					    tableEvent.EventType == TableRowEventType.UpdateAdd)
						toAdd.Add(tableEvent.RowNumber);
				}

				events.RemoveRange(0, count);

				// Mark all added entries to deleted.
				for (int i = 0; i < toAdd.Count; ++i) {
					events.Add(new TableRowEvent(transaction, transaction.CommitId, TableId, TableName, toAdd[i], TableRowEventType.Add));
					events.Add(new TableRowEvent(transaction, transaction.CommitId, TableId, TableName, toAdd[i], TableRowEventType.Remove));
				}

			}
		}
	}
}