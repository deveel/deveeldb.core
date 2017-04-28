using System;

namespace Deveel.Data.Sql.Indexes {
	/// <summary>
	/// The absolute offset of a field in a range of a selection.
	/// </summary>
	/// <seealso cref="IndexRange"/>
	public enum RangeFieldOffset {
		/// <summary>
		/// The offset of the first value of the range. 
		/// </summary>
		FirstValue = 1,

		/// <summary>
		/// The offset of the last value of the range.
		/// </summary>
		LastValue = 2,

		/// <summary>
		/// The offset before the first value of the range.
		/// </summary>
		BeforeFirstValue = 3,

		/// <summary>
		/// The offset after the last value of the range.
		/// </summary>
		AfterLastValue = 4
	}
}