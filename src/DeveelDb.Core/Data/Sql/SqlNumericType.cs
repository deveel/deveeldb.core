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

namespace Deveel.Data.Sql {
	public class SqlNumericType : SqlType {
		public SqlNumericType(SqlTypeCode typeCode, int precision, int scale)
			: base("NUMERIC", typeCode) {
			AssertIsNumeric(typeCode);
			Precision = precision;
			Scale = scale;
		}

		public int Precision { get; }

		public int Scale { get; }

		private static void AssertIsNumeric(SqlTypeCode typeCode) {
			if (!IsNumericType(typeCode))
				throw new ArgumentException(String.Format("The type '{0}' is not a valid NUMERIC type.", typeCode));
		}

		internal static bool IsNumericType(SqlTypeCode typeCode) {
			return typeCode == SqlTypeCode.TinyInt ||
			       typeCode == SqlTypeCode.SmallInt ||
			       typeCode == SqlTypeCode.Integer ||
			       typeCode == SqlTypeCode.BigInt ||
			       typeCode == SqlTypeCode.Real ||
			       typeCode == SqlTypeCode.Float ||
			       typeCode == SqlTypeCode.Double ||
			       typeCode == SqlTypeCode.Decimal ||
			       typeCode == SqlTypeCode.Numeric;
		}

		public override SqlBoolean Greater(ISqlValue a, ISqlValue b) {
			return Compare(a, b) > 0;
		}

		public override SqlBoolean GreaterOrEqual(ISqlValue a, ISqlValue b) {
			return Compare(a, b) >= 0;
		}

		public override SqlBoolean Smaller(ISqlValue a, ISqlValue b) {
			return Compare(a, b) < 0;
		}

		public override SqlBoolean SmallerOrEqual(ISqlValue a, ISqlValue b) {
			return Compare(a, b) <= 0;
		}
	}
}