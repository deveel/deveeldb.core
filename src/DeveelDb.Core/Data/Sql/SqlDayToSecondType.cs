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
using System.IO;

using Deveel.Data.Serialization;

namespace Deveel.Data.Sql {
	public sealed class SqlDayToSecondType : SqlType {
		public SqlDayToSecondType()
			: base(SqlTypeCode.DayToSecond) {
		}

		private SqlDayToSecondType(SerializationInfo info)
			: base(info) {
		}

		public override ISqlValue Add(ISqlValue a, ISqlValue b) {
			if (a is SqlDayToSecond) {
				var x = (SqlDayToSecond)a;

				if (b is SqlDayToSecond) {
					var y = (SqlDayToSecond)b;
					return x.Add(y);
				}
			}

			return base.Add(a, b);
		}

		public override ISqlValue Subtract(ISqlValue a, ISqlValue b) {
			if (a is SqlDayToSecond) {
				var x = (SqlDayToSecond)a;

				if (b is SqlDayToSecond) {
					var y = (SqlDayToSecond)b;
					return x.Subtract(y);
				}
			}

			return base.Subtract(a, b);
		}

		public override bool IsInstanceOf(ISqlValue value) {
			return value is SqlDayToSecond || value is SqlNull;
		}

		public override bool CanCastTo(ISqlValue value, SqlType destType) {
			return destType is SqlCharacterType || destType is SqlBinaryType;
		}

		public override ISqlValue Cast(ISqlValue value, SqlType destType) {
			if (!(value is SqlDayToSecond))
				throw new ArgumentException();

			var dts = (SqlDayToSecond) value;

			if (destType is SqlCharacterType)
				return ToString(dts, (SqlCharacterType) destType);
			if (destType is SqlBinaryType)
				return ToBinary(dts, (SqlBinaryType) destType);

			return base.Cast(value, destType);
		}

		private ISqlValue ToString(SqlDayToSecond dts, SqlCharacterType destType) {
			var s = new SqlString(dts.ToString());
			return destType.NormalizeValue(s);
		}

		private ISqlValue ToBinary(SqlDayToSecond dts, SqlBinaryType destType) {
			var bytes = dts.ToByArray();
			var binary = new SqlBinary(bytes);
			return destType.NormalizeValue(binary);
		}

		protected override void SerializeValue(IContext context, BinaryWriter writer, ISqlValue value) {
			var dts = (SqlDayToSecond) value;
			var bytes = dts.ToByArray();
			writer.Write(bytes.Length);
			writer.Write(bytes);
		}

		protected override ISqlValue DeserializeValue(IContext context, BinaryReader reader) {
			var length = reader.ReadInt32();
			var bytes = reader.ReadBytes(length);

			return new SqlDayToSecond(bytes);
		}
	}
}