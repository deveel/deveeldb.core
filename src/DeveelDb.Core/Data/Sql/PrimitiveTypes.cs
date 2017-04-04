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
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql {
	/// <summary>
	/// Provides some helper functions for resolving and creating
	/// <see cref="SqlType"/> instances that are primitive to the
	/// system.
	/// </summary>
	public static class PrimitiveTypes {
		static PrimitiveTypes() {
			Resolver = new PrimitiveTypesResolver();
		}

		public static ISqlTypeResolver Resolver { get; }

		#region Boolean Types

		public static SqlBooleanType Boolean() {
			return Boolean(SqlTypeCode.Boolean);
		}

		public static SqlBooleanType Boolean(SqlTypeCode sqlType) {
			return new SqlBooleanType(sqlType);
		}

		public static SqlBooleanType Bit() {
			return Boolean(SqlTypeCode.Bit);
		}
		#endregion

		#region Binary Types

		public static SqlBinaryType Binary(int maxSize) {
			return Binary(SqlTypeCode.Binary, maxSize);
		}

		public static SqlBinaryType Binary(SqlTypeCode sqlType, int maxSize) {
			return new SqlBinaryType(sqlType, maxSize);
		}

		public static SqlBinaryType VarBinary() {
			return VarBinary(-1);
		}

		public static SqlBinaryType VarBinary(int maxSize) {
			return Binary(SqlTypeCode.VarBinary, maxSize);
		}

		public static SqlBinaryType Blob(int size) {
			return Binary(SqlTypeCode.Blob, size);
		}

		#endregion

		#region Numeric Types

		public static SqlNumericType Numeric() {
			return Numeric(SqlTypeCode.Numeric);
		}

		public static SqlNumericType Numeric(SqlTypeCode sqlType) {
			return Numeric(sqlType, -1);
		}

		public static SqlNumericType Numeric(SqlTypeCode sqlType, int precision) {
			return Numeric(sqlType, precision, -1);
		}

		public static SqlNumericType Numeric(int precision) {
			return Numeric(precision, -1);
		}

		public static SqlNumericType Numeric(int precision, int scale) {
			return Numeric(SqlTypeCode.Numeric, precision, scale);
		}

		public static SqlNumericType Numeric(SqlTypeCode sqlType, int precision, int scale) {
			return new SqlNumericType(sqlType, precision, scale);
		}

		public static SqlNumericType TinyInt() {
			return Numeric(SqlTypeCode.TinyInt);
		}

		public static SqlNumericType SmallInt() {
			return Numeric(SqlTypeCode.SmallInt);
		}

		public static SqlNumericType Integer() {
			return Numeric(SqlTypeCode.Integer);
		}

		public static SqlNumericType BigInt() {
			return Numeric(SqlTypeCode.BigInt);
		}

		public static SqlNumericType Float() {
			return Numeric(SqlTypeCode.Float, 32, 2);
		}

		public static SqlNumericType Double() {
			return Numeric(SqlTypeCode.Double, 64, 4);
		}

		#endregion

		#region String Types

		public static SqlCharacterType String() {
			return String(SqlTypeCode.String);
		}

		public static SqlCharacterType String(SqlTypeCode sqlType) {
			return String(sqlType, -1);
		}

		public static SqlCharacterType String(SqlTypeCode sqlType, int maxSize) {
			return String(sqlType, maxSize, null);
		}

		public static SqlCharacterType String(SqlTypeCode sqlType, int maxSize, CultureInfo locale) {
			return new SqlCharacterType(sqlType, maxSize, locale);
		}

		public static SqlCharacterType VarChar(int maxSize) {
			return VarChar(maxSize, null);
		}

		public static SqlCharacterType VarChar() {
			return VarChar(null);
		}

		public static SqlCharacterType VarChar(CultureInfo locale) {
			return VarChar(-1, locale);
		}

		public static SqlCharacterType VarChar(int maxSize, CultureInfo locale) {
			return String(SqlTypeCode.VarChar, maxSize, locale);
		}

		public static SqlCharacterType Char(int size) {
			return Char(size, null);
		}

		public static SqlCharacterType Char(int size, CultureInfo locale) {
			return String(SqlTypeCode.Char, size, locale);
		}

		public static SqlCharacterType Clob(int size) {
			return String(SqlTypeCode.Clob, size);
		}

		#endregion

		#region Date Types

		public static SqlDateTimeType DateTime(SqlTypeCode typeCode) {
			return new SqlDateTimeType(typeCode);
		}

		public static SqlDateTimeType TimeStamp() {
			return DateTime(SqlTypeCode.TimeStamp);
		}

		public static SqlDateTimeType Time() {
			return DateTime(SqlTypeCode.Time);
		}

		public static SqlDateTimeType Date() {
			return DateTime(SqlTypeCode.Date);
		}

		#endregion

		/// <summary>
		/// Checks if the given code represents a primitive type.
		/// </summary>
		/// <param name="sqlType">The type code to check</param>
		/// <returns>
		/// Returns <c>true</c> of the given type code represents
		/// a primitive type, otherwise it returns <c>false</c>.
		/// </returns>
		public static bool IsPrimitive(SqlTypeCode sqlType) {
			if (sqlType == SqlTypeCode.Unknown ||
			    sqlType == SqlTypeCode.Type ||
			    sqlType == SqlTypeCode.QueryPlan ||
			    sqlType == SqlTypeCode.Object)
				return false;

			return true;
		}

		/// <summary>
		/// Checks if the string represents the name of a primitive type
		/// </summary>
		/// <param name="name">The name to check</param>
		/// <returns>
		/// Returns <c>true</c> if the input <paramref name="name"/> represents
		/// a primitive type, otherwise returns <c>false</c>.
		/// </returns>
		public static bool IsPrimitive(string name) {
			if (System.String.IsNullOrEmpty(name))
				return false;

			switch (name.ToUpperInvariant()) {
				case "NULL":
					return true;

				case "BOOLEAN":
				case "BOOL":
				case "BIT":
					return true;

				case "NUMERIC":
				case "INT":
				case "INTEGER":
				case "BIGINT":
				case "TINYINT":
				case "SMALLINT":
				case "REAL":
				case "FLOAT":
				case "DOUBLE":
				case "DECIMAL":
					return true;

				case "STRING":
				case "VARCHAR":
				case "CHAR":
				case "CHARACTER VARYING":
				case "CLOB":
				case "LONGVARCHAR":
				case "LONG VARCHAR":
				case "LONG CHARACTER VARYING":
				case "TEXT":
					return true;

				case "BINARY":
				case "VARBINARY":
				case "LONG VARBINARY":
				case "LONGVARBINARY":
				case "LONG BINARY VARYING":
				case "BLOB":
					return true;

				case "DATE":
				case "DATETIME":
				case "TIME":
				case "TIMESTAMP":
					return true;

				case "YEAR TO MONTH":
				case "DAY TO SECOND":
					return true;
			}

			return false;
		}

		private static string GetTypeName(SqlTypeCode typeCode) {
			if (!IsPrimitive(typeCode))
				throw new ArgumentException($"The type with code {typeCode} is not primitive");

			switch (typeCode) {
				case SqlTypeCode.LongVarChar:
					return "LONG VARCHAR";
				case SqlTypeCode.LongVarBinary:
					return "LONG VARBINARY";
				default:
					return typeCode.ToString().ToUpperInvariant();
			}
		}

		private static SqlType ResolvePrimitive(SqlTypeResolveInfo resolveInfo) {
			if (resolveInfo == null)
				throw new ArgumentNullException(nameof(resolveInfo));

			if (!IsPrimitive(resolveInfo.TypeName))
				return null;

			switch (resolveInfo.TypeName.ToUpperInvariant()) {
				// Booleans
				case "BIT":
					return Bit();
				case "BOOL":
				case "BOOLEAN":
					return Boolean();

				// Numerics
				case "TINYINT":
					return TinyInt();
				case "SMALLINT":
					return SmallInt();
				case "INT":
				case "INTEGER":
					return Integer();
				case "BIGINT":
					return BigInt();
				case "REAL":
				case "FLOAT":
					return Float();
				case "DOUBLE":
					return Double();
				case "NUMBER":
				case "NUMERIC":
				case "DECIMAL": {
					var precision = resolveInfo.Properties.GetValue<int?>("Precision") ?? -1;
					var scale = resolveInfo.Properties.GetValue<int?>("Scale") ?? -1;
					return Numeric(precision, scale);
				}

				// Strings
				case "CHAR": {
					var size = resolveInfo.Properties.GetValue<int?>("Size") ?? SqlCharacterType.DefaultMaxSize;
					var localeString = resolveInfo.Properties.GetValue<string>("Locale");
					var locale = System.String.IsNullOrEmpty(localeString) ? null : new CultureInfo(localeString);
					return Char(size, locale);
				}
				case "VARCHAR":
				case "CHARACTER VARYING":
				case "STRING": {
					var maxSize = resolveInfo.Properties.GetValue<int?>("MaxSize") ?? -1;
					var localeString = resolveInfo.Properties.GetValue<string>("Locale");
					var locale = System.String.IsNullOrEmpty(localeString) ? null : new CultureInfo(localeString);
					return VarChar(maxSize, locale);
				}
				case "LONG VARCHAR":
				case "LONGVARCHAR":
				case "LONG CHARACTER VARYING":
				case "TEXT":
				case "CLOB": {
					var size = resolveInfo.Properties.GetValue<int?>("Size") ?? SqlCharacterType.DefaultMaxSize;
					return Clob(size);
				}

				// Date-Time
				case "DATE":
					return Date();
				case "DATETIME":
				case "TIMESTAMP":
					return TimeStamp();
				case "TIME":
					return Time();

				// Binary
				case "BINARY": {
					var size = resolveInfo.Properties.GetValue<int?>("Size") ?? SqlBinaryType.DefaultMaxSize;
					return Binary(size);
				}
				case "VARBINARY":
				case "BINARY VARYING": {
					var size = resolveInfo.Properties.GetValue<int?>("MaxSize") ?? -1;
					return VarBinary(size);
				}
				case "LONGVARBINARY":
				case "LONG VARBINARY":
				case "LONG BINARY VARYING":
				case "BLOB": {
					var size = resolveInfo.Properties.GetValue<int?>("Size") ?? SqlBinaryType.DefaultMaxSize;
					return Blob(size);
				}

				default:
					return null;
			}
		}

		public static SqlType Type(string typeName) {
			return Type(typeName, null);
		}

		public static SqlType Type(string typeName, IDictionary<string, object> properties) {
			return Resolver.Resolve(new SqlTypeResolveInfo(typeName, properties));
		}

		public static SqlType Type(SqlTypeCode typeCode) {
			return Type(typeCode, null);
		}

		public static SqlType Type(SqlTypeCode typeCode, IDictionary<string, object> propeties) {
			var typeName = GetTypeName(typeCode);
			return Type(typeName, propeties);
		}

		#region PrimitiveTypesResolver

		class PrimitiveTypesResolver : ISqlTypeResolver {
			public SqlType Resolve(SqlTypeResolveInfo resolveInfo) {
				return PrimitiveTypes.ResolvePrimitive(resolveInfo);
			}
		}

		#endregion
	}
}