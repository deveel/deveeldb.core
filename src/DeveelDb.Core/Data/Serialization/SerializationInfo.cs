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

		public void SetValue<T>(string memberName, T value)
			=> SetValue(memberName, typeof(T), value);

		public bool HasMember(string memberName) {
			return members.ContainsKey(memberName);
		}

		public object GetValue(string memberName, Type type) {
			SerializationMemberInfo memberInfo;
			if (!members.TryGetValue(memberName, out memberInfo))
				return null;

			object value = memberInfo.Value;

			if (type != null && memberInfo.MemberType != type) {
				value = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
			}

			return value;
		}

		public T GetValue<T>(string memberName)
			=> (T) GetValue(memberName, typeof(T));

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
	}
}