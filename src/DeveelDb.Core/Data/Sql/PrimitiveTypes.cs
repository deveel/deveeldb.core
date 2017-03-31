using System;

namespace Deveel.Data.Sql {
	/// <summary>
	/// Provides some helper functions for resolving and creating
	/// <see cref="SqlType"/> instances that are primitive to the
	/// system.
	/// </summary>
	public static class PrimitiveTypes {
		public static SqlBooleanType Boolean() {
			return Boolean(SqlTypeCode.Boolean);
		}

		public static SqlBooleanType Boolean(SqlTypeCode sqlType) {
			return new SqlBooleanType(sqlType);
		}

		public static SqlBooleanType Bit() {
			return Boolean(SqlTypeCode.Bit);
		}

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