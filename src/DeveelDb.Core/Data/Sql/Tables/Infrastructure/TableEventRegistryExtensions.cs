using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deveel.Data.Sql.Tables.Infrastructure {
    static class TableEventRegistryExtensions {
        public static ICollection<long> AddedRows(this ITableEventRegistry registry) {
            lock (registry) {
                var list = new List<long>();

                foreach (var tableEvent in registry.OfType<TableRowEvent>()) {
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

                return list.AsReadOnly();
            }
        }

        public static ICollection<long> RemovedRows(this ITableEventRegistry registry) {
            lock (registry) {
                var list = new List<long>();

                foreach (var tableEvent in registry.OfType<TableRowEvent>()) {
                    if (tableEvent.EventType == TableRowEventType.Remove ||
                        tableEvent.EventType == TableRowEventType.UpdateRemove)
                        list.Add(tableEvent.RowNumber);
                }

                return list.AsReadOnly();
            }
        }


        public static ITableEventRegistry AsReadOnly(this ITableEventRegistry registry) {
            if (registry.IsReadOnly)
                return registry;

            return new TableEventRegistry(registry.CommitId, registry, true);
        }
    }
}