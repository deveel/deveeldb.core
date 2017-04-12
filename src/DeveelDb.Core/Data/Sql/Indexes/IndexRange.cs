using System;
using System.Text;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Indexes {
	/// <summary>
	/// Describes the range of values to select from an index.
	/// </summary>
	/// <remarks>
	/// A range has a start value, an end value, and whether we should pick 
	/// inclusive or exclusive of the end value. The start value may be a 
	/// concrete value from the set or it may be a flag that represents the 
	/// start or end of the list.
	/// <para>
	/// Note that the start value may not compare less than the end value.
	/// For example, start can not be <see cref="RangeFieldOffset.LastValue"/> 
	/// and end can not be  <see cref="RangeFieldOffset.FirstValue"/>.
	/// </para>
	/// </remarks>
	public struct IndexRange : IEquatable<IndexRange> {
		/// <summary>
		/// 
		/// </summary>
		public static readonly SqlObject FirstInSet = new SqlObject(new SpecialType(), new SqlString("FirstInSet"));

		/// <summary>
		/// 
		/// </summary>
		public static readonly SqlObject LastInSet = new SqlObject(new SpecialType(), new SqlString("LastInSet"));

		/// <summary>
		/// Constructs the range given a start and an end location
		/// </summary>
		/// <param name="startOffset">The offset of the first value of the range.</param>
		/// <param name="firstValue">The first value of the range</param>
		/// <param name="lastOffset">The offset within the range of the last value.</param>
		/// <param name="endValue">The last value of the range.</param>
		public IndexRange(RangeFieldOffset startOffset, SqlObject firstValue, RangeFieldOffset lastOffset, SqlObject endValue)
			: this(false) {
			StartOffset = startOffset;
			StartValue = firstValue;
			EndOffset = lastOffset;
			EndValue = endValue;
		}

		private IndexRange(bool isNull)
			: this() {
			IsNull = isNull;
		}

		public bool IsNull { get; private set; }

		/// <summary>
		/// The entire range of values in an index (including <c>NULL</c>)
		/// </summary>
		public static readonly IndexRange FullRange = 
			new IndexRange(RangeFieldOffset.FirstValue, FirstInSet, RangeFieldOffset.LastValue, LastInSet);

		/// <summary>
		/// The entire range of values in an index (not including <c>NULL</c>)
		/// </summary>
		public static readonly IndexRange FullRangeNotNull = 
			new IndexRange(RangeFieldOffset.AfterLastValue, SqlObject.Null, RangeFieldOffset.LastValue, LastInSet);

		public static readonly IndexRange Null = new IndexRange(true);

		/// <summary>
		/// Gets the offset of the first value of the range.
		/// </summary>
		public RangeFieldOffset StartOffset { get; private set; }

		/// <summary>
		/// Gets the first value of the range.
		/// </summary>
		public SqlObject StartValue { get; private set; }

		/// <summary>
		/// Gets the offset of the last value of the range.
		/// </summary>
		public RangeFieldOffset EndOffset { get; private set; }

		/// <summary>
		/// Gets the last value of the range.
		/// </summary>
		public SqlObject EndValue { get; private set; }

		public bool Equals(IndexRange other) {
			if (IsNull && other.IsNull)
				return true;
			if (IsNull && !other.IsNull)
				return false;
			if (!IsNull && other.IsNull)
				return false;

			return (StartValue.Value.Equals(other.StartValue.Value) &&
			        EndValue.Value.Equals(other.EndValue.Value) &&
			        StartOffset == other.StartOffset &&
			        EndOffset == other.EndOffset);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj) {
			if (!(obj is IndexRange))
				return false;

			return Equals((IndexRange) obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		/// <inheritdoc/>
		public override string ToString() {
			var sb = new StringBuilder();
			if (StartOffset == RangeFieldOffset.FirstValue) {
				sb.Append("FIRST_VALUE ");
			} else if (StartOffset == RangeFieldOffset.AfterLastValue) {
				sb.Append("AFTER_LAST_VALUE ");
			}

			sb.Append(StartValue.Value);
			sb.Append(" -> ");
			if (EndOffset == RangeFieldOffset.LastValue) {
				sb.Append("LAST_VALUE ");
			} else if (EndOffset == RangeFieldOffset.BeforeFirstValue) {
				sb.Append("BEFORE_FIRST_VALUE ");
			}
			sb.Append(EndValue.Value);
			return sb.ToString();
		}

		public static bool operator ==(IndexRange a, IndexRange b) {
			return a.Equals(b);
		}

		public static bool operator !=(IndexRange a, IndexRange b) {
			return !(a == b);
		}

		#region SpecialType

		class SpecialType : SqlType {
			public SpecialType()
				: base((SqlTypeCode) 255) {
			}

			public override bool IsInstanceOf(ISqlValue value) {
				return value is SqlString;
			}
		}

		#endregion
	}
}