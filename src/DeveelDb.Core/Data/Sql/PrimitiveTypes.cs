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
using System.Text;

namespace Deveel.Data.Sql {
	/// <summary>
	/// Provides some helper functions for resolving and creating
	/// <see cref="SqlType"/> instances that are primitive to the
	/// system.
	/// </summary>
	public static class PrimitiveTypes {
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

		public static SqlBinaryType Binary() {
			return Binary(-1);
		}

		public static SqlBinaryType Binary(int maxSize) {
			return Binary(SqlTypeCode.Binary, maxSize);
		}

		public static SqlBinaryType Binary(SqlTypeCode sqlType) {
			return Binary(sqlType, -1);
		}

		public static SqlBinaryType Binary(SqlTypeCode sqlType, int maxSize) {
			return new SqlBinaryType(sqlType, maxSize);
		}

		public static SqlBinaryType VarBinary(int maxSize) {
			return Binary(SqlTypeCode.VarBinary, maxSize);
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

		public static SqlNumericType Real() {
			return Real(-1);
		}

		public static SqlNumericType Real(int precision) {
			return Numeric(SqlTypeCode.Real, precision);
		}

		public static SqlNumericType Real(int precision, int scale) {
			return Numeric(SqlTypeCode.Real, precision, scale);
		}

		public static SqlNumericType Float() {
			return Float(-1);
		}

		public static SqlNumericType Float(int precision) {
			return Float(precision, -1);
		}

		public static SqlNumericType Float(int precision, int scale) {
			return Numeric(SqlTypeCode.Float, precision, scale);
		}

		public static SqlNumericType Double(int precision, int scale) {
			return Numeric(SqlTypeCode.Double, precision, scale);
		}

		public static SqlNumericType Double() {
			return Double(-1);
		}

		public static SqlNumericType Double(int precision) {
			return Double(precision, -1);
		}

		#endregion

		#region String Types

		public static SqlStringType String() {
			return String(SqlTypeCode.String);
		}

		public static SqlStringType String(SqlTypeCode sqlType) {
			return String(sqlType, -1);
		}

		public static SqlStringType String(SqlTypeCode sqlType, int maxSize) {
			return String(sqlType, maxSize, null);
		}

		public static SqlStringType String(SqlTypeCode sqlType, int maxSize, CultureInfo locale) {
			return new SqlStringType(sqlType, maxSize, locale);
		}

		public static SqlStringType VarChar(int maxSize) {
			return VarChar(maxSize, null);
		}

		public static SqlStringType VarChar() {
			return VarChar(null);
		}

		public static SqlStringType VarChar(CultureInfo locale) {
			return VarChar(-1, locale);
		}

		public static SqlStringType VarChar(int maxSize, CultureInfo locale) {
			return String(SqlTypeCode.VarChar, maxSize, locale);
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

			if (System.String.Equals("long varchar", name, StringComparison.OrdinalIgnoreCase))
				name = "longvarchar";
			if (System.String.Equals("long varbinary", name, StringComparison.OrdinalIgnoreCase))
				name = "longvarbinary";

			if (name.EndsWith("%TYPE", StringComparison.OrdinalIgnoreCase) ||
			    name.EndsWith("%ROWTYPE", StringComparison.OrdinalIgnoreCase))
				return true;

			if (name.Equals("NUMERIC", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("STRING", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("DATE", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("BOOLEAN", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("BINARY", StringComparison.OrdinalIgnoreCase))
				return true;

			if (name.Equals("BIT", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("BOOLEAN", StringComparison.OrdinalIgnoreCase))
				return true;

			if (name.Equals("TINYINT", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("SMALLINT", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("INTEGER", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("INT", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("BIGINT", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("REAL", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("FLOAT", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("DOUBLE", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("DECIMAL", StringComparison.OrdinalIgnoreCase))
				return true;

			if (name.Equals("DATE", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("TIME", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("TIMESTAMP", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("DATETIME", StringComparison.OrdinalIgnoreCase))
				return true;

			if (name.Equals("YEAR TO MONTH", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("DAY TO SECOND", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("INTERVAL", StringComparison.OrdinalIgnoreCase))
				return true;

			if (name.Equals("CHAR", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("VARCHAR", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("LONGVARCHAR", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("CLOB", StringComparison.OrdinalIgnoreCase))
				return true;

			if (name.Equals("BINARY", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("VARBINARY", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("LONGVARBINARY", StringComparison.OrdinalIgnoreCase) ||
			    name.Equals("BLOB", StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}
	}
}