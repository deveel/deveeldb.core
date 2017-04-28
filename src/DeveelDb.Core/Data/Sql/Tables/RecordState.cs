using System;

namespace Deveel.Data.Sql.Tables
{
	/// <summary>
	/// An enumeration that represents the various states of a record.
	/// </summary>
	public enum RecordState {
		///<summary>
		///</summary>
		Uncommitted = 0,
		///<summary>
		///</summary>
		CommittedAdded = 0x010,
		///<summary>
		///</summary>
		CommittedRemoved = 0x020,
		///<summary>
		///</summary>
		Deleted = 0x020000,     // ie. available for reclaimation.

		///<summary>
		/// Denotes an erroneous record state.
		///</summary>
		Error = -1
	}
}
