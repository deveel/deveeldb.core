using System;

namespace Deveel.Data.Serialization {
	public struct SerializationMemberInfo {
		public SerializationMemberInfo(string memberName, Type memberType, object value) {
			if (String.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));
			if (memberType == null)
				throw new ArgumentNullException(nameof(memberType));

			MemberName = memberName;
			MemberType = memberType;
			Value = value;
		}

		public string MemberName { get; }

		public Type MemberType { get; }

		public object Value { get; }
	}
}