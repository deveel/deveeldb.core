using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data.Serialization {
	public sealed class BinarySerializer {
		private void WriteObject(BinaryWriter writer, Type objectType, object obj) {
			var objTypeCode = GetObjectTypeCode(objectType);
			writer.Write((byte)objTypeCode);

			if (typeof(ISerializable).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo())) {
				writer.Write(objectType.AssemblyQualifiedName);

				var info = new SerializationInfo(objectType);
				if (obj != null) {
					((ISerializable)obj).GetObjectData(info);

					writer.Write(info.MemberCount);

					foreach (var memberInfo in info) {
						WriteMember(writer, memberInfo);
					}
				}
			} else if (IsPrimitive(objectType)) {
				WritePrimitive(writer, objectType, obj);
			} else if (objectType.IsArray) {
				WriteArray(writer, objectType, obj);
			}

			// TODO: Lists and Dictionaries
		}

		private void WriteArray(BinaryWriter writer, Type objectType, object obj) {
			writer.Write(obj == null ? (byte)0 : (byte)1);

			if (obj == null)
				return;

			var array = (Array)obj;
			var elementType = obj.GetType().GetElementType();
			var elemTypeCode = GetObjectTypeCode(elementType);

			writer.Write((byte)elemTypeCode);

			if (elemTypeCode == ObjectTypeCode.Primitive) {
				var primitiveTypeCode = GetPrimitiveCode(objectType);
				if (primitiveTypeCode == null)
					throw new NotSupportedException();

				writer.Write((byte)primitiveTypeCode.Value);
			} else if (elemTypeCode == ObjectTypeCode.Object) {
				var name = elementType.AssemblyQualifiedName;

				writer.Write(name);
			} else {
				throw new NotSupportedException($"Array of type {elementType} is not supported");
			}
			
			if (array.Rank > 1)
				throw new NotSupportedException("Multi-dimensional arrays not supported");

			writer.Write(array.Length);

			for (int i = 0; i < array.Length; i++) {
				var value = array.GetValue(i);
				WriteObject(writer, elementType, value);
			}
		}

		private void WritePrimitive(BinaryWriter writer, Type objectType, object value) {
			var code = GetPrimitiveCode(objectType);
			if (code == null)
				throw new SerializationException($"The type {objectType} is not primitive");

			writer.Write((byte) code.Value);
			writer.Write(value == null ? (byte)0 : (byte)1);

			if (value == null)
				return;

			switch (code.Value) {
				case PrimitiveTypeCode.Boolean:
					writer.Write((bool)value);
					break;
				case PrimitiveTypeCode.Byte:
					writer.Write((byte)value);
					break;
				case PrimitiveTypeCode.SByte:
					writer.Write((sbyte)value);
					break;
				case PrimitiveTypeCode.Int16:
					writer.Write((short)value);
					break;
				case PrimitiveTypeCode.UInt16:
					writer.Write((ushort)value);
					break;
				case PrimitiveTypeCode.Int32:
					writer.Write((int)value);
					break;
				case PrimitiveTypeCode.UInt32:
					writer.Write((uint)value);
					break;
				case PrimitiveTypeCode.Int64:
					writer.Write((long)value);
					break;
				case PrimitiveTypeCode.UInt64:
					writer.Write((ulong)value);
					break;
				case PrimitiveTypeCode.Single:
					writer.Write((float)value);
					break;
				case PrimitiveTypeCode.Double:
					writer.Write((double)value);
					break;
				case PrimitiveTypeCode.Char:
					writer.Write((char)value);
					break;
				case PrimitiveTypeCode.String:
					writer.Write((string)value);
					break;
			}
		}

		private static bool IsPrimitive(Type type) {
			return type.GetTypeInfo().IsPrimitive ||
			       type == typeof(string);
		}

		private static ObjectTypeCode GetObjectTypeCode(Type objecType) {
			if (objecType.IsArray)
				return ObjectTypeCode.Array;
			if (IsPrimitive(objecType))
				return ObjectTypeCode.Primitive;
			// TODO: support for lists and dictionaries

			if (typeof(ISerializable).GetTypeInfo().IsAssignableFrom(objecType.GetTypeInfo()))
				return ObjectTypeCode.Object;

			throw new NotSupportedException();
		}

		private static PrimitiveTypeCode? GetPrimitiveCode(Type type) {
			if (type == typeof(bool))
				return PrimitiveTypeCode.Boolean;
			if (type == typeof(byte))
				return PrimitiveTypeCode.Byte;
			if (type == typeof(sbyte))
				return PrimitiveTypeCode.SByte;
			if (type == typeof(short))
				return PrimitiveTypeCode.Int16;
			if (type == typeof(ushort))
				return PrimitiveTypeCode.UInt16;
			if (type == typeof(int))
				return PrimitiveTypeCode.Int32;
			if (type == typeof(uint))
				return PrimitiveTypeCode.UInt32;
			if (type == typeof(long))
				return PrimitiveTypeCode.Int64;
			if (type == typeof(ulong))
				return PrimitiveTypeCode.UInt64;
			if (type == typeof(float))
				return PrimitiveTypeCode.Single;
			if (type == typeof(double))
				return PrimitiveTypeCode.Double;
			if (type == typeof(char))
				return PrimitiveTypeCode.Char;
			if (type == typeof(string))
				return PrimitiveTypeCode.String;

			return null;
		}

		private void WriteMember(BinaryWriter writer, SerializationMemberInfo memberInfo) {
			writer.Write(memberInfo.MemberName);
			WriteObject(writer, memberInfo.MemberType, memberInfo.Value);
		}

		public void Serialize(object obj, Stream outputStream) {
			Serialize(obj, outputStream, Encoding.Unicode);
		}

		public void Serialize(object obj, Stream outputStream, Encoding encoding) {
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			var objType = obj.GetType();
			var writer = new BinaryWriter(outputStream, encoding);
			WriteObject(writer, objType, obj);

			writer.Flush();
		}

		public object Deserialize(Type type, Stream inputStream, Encoding encoding) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (inputStream == null)
				throw new ArgumentNullException(nameof(inputStream));

			if (!inputStream.CanRead)
				throw new ArgumentException("The stream cannot be read", nameof(inputStream));

			var reader = new BinaryReader(inputStream, encoding);
			return ReadObject(reader, type);
		}

		private object ReadObject(BinaryReader reader, Type objectType) {
			var objTypeCode = GetObjectTypeCode(objectType);

			switch (objTypeCode) {
				case ObjectTypeCode.Primitive:
					return ReadPrimitive(reader);
				case ObjectTypeCode.Object:
					return ReadSerialized(reader);
				case ObjectTypeCode.Array:
					return ReadArray(reader);
				case ObjectTypeCode.List:
					return ReadList(reader);
				case ObjectTypeCode.Dictionary:
					return ReadDictionary(reader);
				default:
					throw new NotSupportedException($"Cannot deserialize {objectType}");
			}
		}

		private object ReadDictionary(BinaryReader reader) {
			throw new NotImplementedException();
		}

		private object ReadList(BinaryReader reader) {
			throw new NotImplementedException();
		}

		private object ReadArray(BinaryReader reader) {
			throw new NotImplementedException();
		}

		private object ReadSerialized(BinaryReader reader) {
			throw new NotImplementedException();
		}

		private object ReadPrimitive(BinaryReader reader) {
			throw new NotImplementedException();
		}

		public object Deserialize(Type type, Stream inputStream)
			=> Deserialize(type, inputStream, Encoding.Unicode);

		#region ObjectTypeCode

		enum ObjectTypeCode {
			Primitive = 1,
			Object,
			Array,
			List,
			Dictionary
		}

		#endregion

		#region PrimitiveTypeCode

		enum PrimitiveTypeCode : byte {
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

		#endregion
	}
}