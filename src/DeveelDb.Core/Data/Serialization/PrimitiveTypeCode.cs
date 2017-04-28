using System;
using System.Collections.Generic;
using System.Text;

namespace Deveel.Data.Serialization {
	internal enum PrimitiveTypeCode : byte {
		Boolean = 1,
		Byte,
		SByte,
		Int16,
		UInt16,
		Int32,
		UInt32,
		Int64,
		UInt64,
		Single,
		Double,
		Char,
		String
	}
}