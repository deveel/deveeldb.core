﻿using System;
using System.IO;

using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql {
	public static class SqlTypeUtil {
		public static SqlType FromValue(object value) {
			if (value is SqlNumber) {
				var number = (SqlNumber) value;
				if (number.Scale == 0) {
					if (number.Precision <= 3)
						return PrimitiveTypes.TinyInt();
					if (number.Precision <= 5)
						return PrimitiveTypes.SmallInt();
					if (number.Precision <= 10)
						return PrimitiveTypes.Integer();
					if (number.Precision <= 19)
						return PrimitiveTypes.BigInt();
				} else {
					if (number.Precision <= 8)
						return PrimitiveTypes.Float();
					if (number.Precision <= 12)
						return PrimitiveTypes.Double();

					return PrimitiveTypes.Numeric(number.Precision, number.Scale);
				}
			}

			if (value is SqlBoolean)
				return PrimitiveTypes.Boolean();

			if (value is bool)
				return PrimitiveTypes.Boolean();

			if (value is double)
				return PrimitiveTypes.Double();
			if (value is float)
				return PrimitiveTypes.Float();
			if (value is int)
				return PrimitiveTypes.Integer();
			if (value is long)
				return PrimitiveTypes.BigInt();
			if (value is byte)
				return PrimitiveTypes.TinyInt();
			if (value is short)
				return PrimitiveTypes.SmallInt();

			if (value is string)
				return PrimitiveTypes.String();

			throw new NotSupportedException();
		}

		public static SqlType Serialize(SqlType type, ISqlTypeResolver typeResolver = null) {
			var bytes = SqlType.Serialize(type);

			using (var stream = new MemoryStream(bytes)) {
				return SqlType.Deserialize(stream, typeResolver);
			}
		}
	}
}