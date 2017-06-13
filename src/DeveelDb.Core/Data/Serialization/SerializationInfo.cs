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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Deveel.Data.Serialization {
	public sealed class SerializationInfo : IEnumerable<SerializationMemberInfo> {

		private readonly Dictionary<string, SerializationMemberInfo> members;

		public SerializationInfo(Type objectType) {
			if (objectType == null)
				throw new ArgumentNullException(nameof(objectType));

			ObjectType = objectType;
			members = new Dictionary<string, SerializationMemberInfo>();
		}

		public Type ObjectType { get; }

		public int MemberCount => members.Count;

		public IEnumerator<SerializationMemberInfo> GetEnumerator() {
			return members.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void SetValue(string memberName, Type memberType, object value) {
			if (String.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));

			if (memberType == null) {
				if (value == null)
					throw new ArgumentNullException(nameof(memberType), "A member type is required when the value is null");

				memberType = value.GetType();
			}

			members[memberName] = new SerializationMemberInfo(memberName, memberType, value);
		}

		public void SetValue(string memberName, bool value)
			=> SetValue(memberName, typeof(bool), value);

		public void SetValue(string memberName, byte value)
			=> SetValue(memberName, typeof(byte), value);

		public void SetValue(string memberName, short value)
			=> SetValue(memberName, typeof(short), value);

		public void SetValue(string memberName, ushort value)
			=> SetValue(memberName, typeof(ushort), value);

		public void SetValue(string memberName, int value)
			=> SetValue(memberName, typeof(int), value);

		public void SetValue(string memberName, uint value)
			=> SetValue(memberName, typeof(uint), value);

		public void SetValue(string memberName, long value)
			=> SetValue(memberName, typeof(long), value);

		public void SetValue(string memberName, ulong value)
			=> SetValue(memberName, typeof(ulong), value);

		public void SetValue(string memberName, float value)
			=> SetValue(memberName, typeof(float), value);

		public void SetValue(string memberName, double value)
			=> SetValue(memberName, typeof(double), value);

		public void SetValue(string memberName, char value)
			=> SetValue(memberName, typeof(char), value);

		public void SetValue(string memberName, string value)
			=> SetValue(memberName, typeof(string), value);

		public void SetValue<T>(string memberName, T value)
			=> SetValue(memberName, typeof(T), value);

		public bool HasMember(string memberName) {
			return members.ContainsKey(memberName);
		}

		public object GetValue(string memberName, Type type) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			SerializationMemberInfo memberInfo;
			if (!members.TryGetValue(memberName, out memberInfo))
				return null;

			object value = memberInfo.Value;

			if (memberInfo.MemberType != type) {
				var nullableType = Nullable.GetUnderlyingType(type);
				if (nullableType != null) {
					value = Convert.ChangeType(value, nullableType, CultureInfo.InvariantCulture);
				} else {
					value = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
				}
			}

			return value;
		}

		public T GetValue<T>(string memberName) {
			var value = GetValue(memberName, typeof(T));
			if (value == null)
				return default(T);

			return (T)value;
		}

		public byte GetByte(string memberName)
			=> GetValue<byte>(memberName);

		public sbyte GetSByte(string memberName)
			=> GetValue<sbyte>(memberName);

		public bool GetBoolean(string memberName)
			=> GetValue<bool>(memberName);

		public short GetInt16(string memberName)
			=> GetValue<short>(memberName);

		public ushort GetUInt16(string memberName)
			=> GetValue<ushort>(memberName);

		public int GetInt32(string memberName)
			=> GetValue<int>(memberName);

		public uint GetUInt32(string memberName)
			=> GetValue<uint>(memberName);

		public long GetInt64(string memberName)
			=> GetValue<long>(memberName);

		public ulong GetUInt64(string memberName)
			=> GetValue<ulong>(memberName);

		public float GetSingle(string memberName)
			=> GetValue<float>(memberName);

		public double GetDouble(string memberName)
			=> GetValue<double>(memberName);

		public string GetString(string memberName)
			=> GetValue<string>(memberName);
	}
}