using System;

namespace Deveel.Data.Sql.Tables.Infrastructure {
    enum TableRowEventType {
        /// <summary>
        /// A new row was added to the table.
        /// </summary>
        Add = 1,

        /// <summary>
        /// A row was removed from a table.
        /// </summary>
        Remove = 2,

        /// <summary>
        /// During an update of values of a row, this was
        /// added again to a table.
        /// </summary>
        UpdateAdd = 3,

        /// <summary>
        /// During an update of values of a row, this
        /// was removed before the value are update.
        /// </summary>
        UpdateRemove = 4,
    }
}