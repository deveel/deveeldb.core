using System;
using System.Globalization;
using System.Text;

namespace Deveel.Data.Sql {
	public sealed class SqlStringType : SqlType {
		private CompareInfo collator;

		public const int DefaultMaxSize = Int16.MaxValue;

		public SqlStringType(SqlTypeCode typeCode, int maxSize, CultureInfo locale) 
			: base("STRING", typeCode) {
			AssertIsString(typeCode);
			MaxSize = maxSize;
			Locale = locale;
		}

		/// <summary>
		/// Gets the maximum number of characters that strings
		/// handled by this type can handle.
		/// </summary>
		public int MaxSize { get; }

		public bool HasMaxSize => MaxSize > 0;

		/// <summary>
		/// Gets the locale used to compare string values.
		/// </summary>
		/// <remarks>
		/// When this value is not specified, the schema or database locale
		/// is used to compare string values.
		/// </remarks>
		public CultureInfo Locale { get; }

		private CompareInfo Collator {
			get {
				lock (this) {
					if (collator != null) {
						return collator;
					} else {
						//TODO:
						collator = Locale.CompareInfo;
						return collator;
					}
				}
			}
		}

		private static void AssertIsString(SqlTypeCode sqlType) {
			if (!IsStringType(sqlType))
				throw new ArgumentException(String.Format("The type {0} is not a valid STRING type.", sqlType), "sqlType");
		}

		internal static bool IsStringType(SqlTypeCode typeCode) {
			return typeCode == SqlTypeCode.String ||
			       typeCode == SqlTypeCode.VarChar ||
			       typeCode == SqlTypeCode.Char ||
			       typeCode == SqlTypeCode.LongVarChar ||
			       typeCode == SqlTypeCode.Clob;
		}
	}
}