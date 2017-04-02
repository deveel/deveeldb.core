// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;
using System.Globalization;
using System.IO;
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

		protected override void AppendTo(SqlStringBuilder builder) {
			if (TypeCode == SqlTypeCode.LongVarChar) {
				builder.Append("LONG CHARACTER VARYING");
			} else {
				base.AppendTo(builder);
			}

			if (MaxSize >= 0) {
				if (MaxSize == DefaultMaxSize)
					builder.Append("(MAX)");
				else
					builder.AppendFormat("({0})", MaxSize);
			}

			if (Locale != null) {
				builder.AppendFormat(" COLLATE '{0}'", Locale.Name);
			}
		}

		public override bool IsComparable(SqlType type) {
			// Are we comparing with another string type?
			if (type is SqlStringType) {
				var stringType = (SqlStringType)type;
				// If either locale is null return true
				if (Locale == null || stringType.Locale == null)
					return true;

				//TODO: Check batter on the locale comparison: we could compare
				//      neutral cultures

				// If the locales are the same return true
				return Locale.Equals(stringType.Locale);
			}

			// Only string types can be comparable
			return false;
		}

		public override int Compare(ISqlValue x, ISqlValue y) {
			if (x == null)
				throw new ArgumentNullException(nameof(x));

			if (!(x is ISqlString) ||
			    !(y is ISqlString))
				throw new ArgumentException("Cannot compare objects that are not strings.");

			if (x.IsNull && y.IsNull)
				return 0;
			if (x.IsNull && !y.IsNull)
				return 1;
			if (!x.IsNull && y.IsNull)
				return -1;

			// If lexicographical ordering,
			if (Locale == null)
				return LexicographicalOrder((ISqlString)x, (ISqlString)y);

			return Collator.Compare(x.ToString(), y.ToString());
		}

		private static int LexicographicalOrder(ISqlString str1, ISqlString str2) {
			// If both strings are small use the 'toString' method to compare the
			// strings.  This saves the overhead of having to store very large string
			// objects in memory for all comparisons.
			long str1Size = str1.Length;
			long str2Size = str2.Length;
			if (str1Size < 32 * 1024 &&
			    str2Size < 32 * 1024) {
				return String.Compare(str1.ToString(), str2.ToString(), StringComparison.Ordinal);
			}

			// TODO: pick one of the two encodings?

			// The minimum size
			long size = System.Math.Min(str1Size, str2Size);
			TextReader r1 = str1.GetInput();
			TextReader r2 = str2.GetInput();
			try {
				try {
					while (size > 0) {
						int c1 = r1.Read();
						int c2 = r2.Read();
						if (c1 != c2) {
							return c1 - c2;
						}
						--size;
					}
					// They compare equally up to the limit, so now compare sizes,
					if (str1Size > str2Size) {
						// If str1 is larger
						return 1;
					} else if (str1Size < str2Size) {
						// If str1 is smaller
						return -1;
					}
					// Must be equal
					return 0;
				} finally {
					r1.Dispose();
					r2.Dispose();
				}
			} catch (IOException e) {
				throw new Exception("IO Error: " + e.Message);
			}
		}

		public override bool CanCastTo(SqlType destType) {
			return destType is SqlStringType ||
			       destType is SqlBinaryType ||
			       destType is SqlBooleanType ||
				   destType is SqlNumericType;
		}

		public override ISqlValue Cast(ISqlValue value, SqlType destType) {
			if (!(value is ISqlString))
				throw new ArgumentException("Cannot cast a non-string value using a string type");

			if (value is SqlString) {
				if (destType is SqlBooleanType)
					return ToBoolean((SqlString) value);
				if (destType is SqlNumericType)
					return ToNumber((SqlString) value, (SqlNumericType) destType);
				if (destType is SqlStringType)
					return ToString((SqlString) value, (SqlStringType) destType);
			}

			return base.Cast(value, destType);
		}

		private SqlString ToString(SqlString value, SqlStringType destType) {
			switch (destType.TypeCode) {
				case SqlTypeCode.Char:
					return PadString(value, destType.MaxSize);
				case SqlTypeCode.LongVarChar:
				case SqlTypeCode.Clob:
					throw new NotImplementedException();
				case SqlTypeCode.String:
				case SqlTypeCode.VarChar:
					return LimitString(value, destType.MaxSize);
				default:
					throw new InvalidCastException();
			}
		}

		private SqlString PadString(SqlString value, int size) {
			if (value.Length > size) {
				value = value.Substring(0, size);
			} else if (value.Length < size) {
				value = value.PadRight(size);
			}

			return value;
		}

		private SqlString LimitString(SqlString value, int maxSize) {
			if (value.Length > maxSize)
				value = value.Substring(0, maxSize);

			return value;
		}



		private SqlNumber ToNumber(SqlString value, SqlNumericType destType) {
			try {
				var locale = Locale ?? CultureInfo.InvariantCulture;
				SqlNumber number;
				if (!SqlNumber.TryParse(value.Value, locale, out number))
					throw new InvalidCastException();

				switch (destType.TypeCode) {
					case SqlTypeCode.TinyInt:
					case SqlTypeCode.SmallInt:
					case SqlTypeCode.Integer:
					case SqlTypeCode.BigInt:
						return ToInteger(number, destType);
					case SqlTypeCode.Real:
					case SqlTypeCode.Float:
					case SqlTypeCode.Double:
					case SqlTypeCode.Numeric:
						return ToDecimal(number, destType);
				}
			} catch (InvalidCastException) {
				throw;
			} catch (Exception ex) {
				throw new InvalidCastException("Invalid cast", ex);
			}

			throw new InvalidCastException();
		}

		private SqlNumber ToDecimal(SqlNumber number, SqlNumericType destType) {
			if (SqlNumber.IsNan(number))
				return SqlNumber.NaN;
			if (SqlNumber.IsNegativeInfinity(number))
				return SqlNumber.NegativeInfinity;
			if (SqlNumber.IsPositiveInfinity(number))
				return SqlNumber.PositiveInfinity;

			var precision = number.Precision;
			var scale = number.Scale;
			if (destType.Precision > 0)
				precision = destType.Precision;
			if (destType.Scale > 0)
				scale = destType.Scale;

			return new SqlNumber(number.ToByteArray(), scale, precision);
		}

		private SqlNumber ToInteger(SqlNumber number, SqlNumericType destType) {
			if (!number.CanBeInt32 && !number.CanBeInt64)
				throw new InvalidCastException("Not a valid integer");

			switch (destType.TypeCode) {
				case SqlTypeCode.TinyInt:
					return new SqlNumber(number.ToByte());
				case SqlTypeCode.SmallInt:
					return new SqlNumber(number.ToInt16());
				case SqlTypeCode.Integer:
					return new SqlNumber(number.ToInt32());
				case SqlTypeCode.BigInt:
					return new SqlNumber(number.ToInt64());
				default:
					throw new InvalidCastException();
			}
		}

		private SqlBoolean ToBoolean(SqlString value) {
			if (value == null || value.IsNull)
				return SqlBoolean.Null;

			if (value.Equals(SqlBoolean.TrueString, true))
				return SqlBoolean.True;
			if (value.Equals(SqlBoolean.FalseString, true))
				return SqlBoolean.False;

			throw new InvalidCastException();
		}
	}
}