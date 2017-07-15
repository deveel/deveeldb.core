using System;

using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables {
	public interface IMutableTable : ITable, ILockable {
		/// <summary>
		/// Persists a new row to the table.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The row to be added must belong to the table context, 
		/// otherwise an exception will be thrown.
		/// </para>
		/// </remarks>
		/// <param name="row">The row to be persisted.</param>
		/// <returns>
		/// Returns a <see cref="RowId"/> that is the pointer to the row
		/// established in the table.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// If the gven <paramref name="row"/> does not belong to 
		/// the table context.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If the given <paramref name="row"/> is <c>null</c>.
		/// </exception>
		RowId AddRow(Row row);

		/// <summary>
		/// Updates the values of a row into the table.
		/// </summary>
		/// <param name="row">The object containing the values to update.</param>
		/// <exception cref="ArgumentNullException">
		/// If the given <paramref name="row"/> is <c>null</c>.
		/// </exception>
		void UpdateRow(Row row);

		/// <summary>
		/// Deletes row identified by the given coordinates from the table.
		/// </summary>
		/// <param name="rowId">The unique identifier of the row to be removed.</param>
		/// <returns>
		/// Returns <c>true</c> if the row identified was found and removed,
		/// <c>false</c> otherwise.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown if the given <paramref name="rowId"/> does not belong
		/// to this table.
		/// </exception>
		bool RemoveRow(RowId rowId);
	}
}
