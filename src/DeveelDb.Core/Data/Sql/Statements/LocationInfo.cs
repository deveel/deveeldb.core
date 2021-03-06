﻿// 
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

using Deveel.Data.Serialization;

namespace Deveel.Data.Sql.Statements {
	public sealed class LocationInfo : ISerializable, IEquatable<LocationInfo> {
		public LocationInfo(int line, int column) {
			Line = line;
			Column = column;
		}

		private LocationInfo(SerializationInfo info) {
			Line = info.GetInt32("line");
			Column = info.GetInt32("column");
		}

		public int Line { get; }

		public int Column { get; }

		void ISerializable.GetObjectData(SerializationInfo info) {
			info.SetValue("line", Line);
			info.SetValue("column", Column);
		}

		public bool Equals(LocationInfo other) {
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;

			return Line == other.Line && Column == other.Column;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;

			return obj is LocationInfo && Equals((LocationInfo) obj);
		}

		public override int GetHashCode() {
			unchecked {
				return (Line * 397) ^ Column;
			}
		}
	}
}