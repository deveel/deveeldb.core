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
using System.IO;
using System.Reflection;
using System.Text;

namespace Deveel.Data.Serialization {
	public sealed class BinarySerializer {
		#region Serialize

		private static void WriteSerializable(BinaryWriter writer, Type objectType, object obj) {
			writer.Write(objectType.AssemblyQualifiedName);

			if (obj == null)
				return;

			if (!SerializerUtil.IsInstantiable(objectType)) {
				writer.Write(obj.GetType().AssemblyQualifiedName);
			}

			var info = new SerializationInfo(objectType);
			((ISerializable) obj).GetObjectData(info);

			writer.Write(info.MemberCount);

			foreach (var memberInfo in info) {
				WriteMember(writer, memberInfo);
			}
		}

		private static void WriteEnum(BinaryWriter writer, Type enumType, object obj) {
			writer.Write(enumType.AssemblyQualifiedName);

			var underlyingType = Enum.GetUnderlyingType(enumType);
			var primitiveValue = Convert.ChangeType(obj, underlyingType);

			WritePrimitive(writer, underlyingType, primitiveValue);
		}

		private static void WriteObject(BinaryWriter writer, Type objectType, object obj) {
			var objTypeCode = SerializerUtil.GetObjectTypeCode(objectType, obj);
			writer.Write((byte)objTypeCode);

			writer.Write(obj == null ? (byte)0 : (byte)1);

			switch (objTypeCode) {
				case ObjectTypeCode.Serializable:
					WriteSerializable(writer, objectType, obj);
					break;
				case ObjectTypeCode.Object:
					WriteUnknownObject(writer, obj);
					break;
				case ObjectTypeCode.Primitive:
					WritePrimitive(writer, objectType, obj);
					break;
				case ObjectTypeCode.Enum:
					WriteEnum(writer, objectType, obj);
					break;
				case ObjectTypeCode.Array:
					WriteArray(writer, objectType, obj);
					break;
				case ObjectTypeCode.List:
					WriteList(writer, objectType, obj);
					break;
				case ObjectTypeCode.Dictionary:
					WriteDictionary(writer, objectType, obj);
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private static void WriteUnknownObject(BinaryWriter writer, object obj) {
			if (obj == null)
				return;

			var objType = obj.GetType();
			var typeCode = SerializerUtil.GetObjectTypeCode(objType, obj);
			writer.Write((byte)typeCode);

			switch (typeCode) {
				case ObjectTypeCode.Serializable:
					WriteSerializable(writer, objType, obj);
					break;
				case ObjectTypeCode.Primitive:
					WritePrimitive(writer, objType, obj);
					break;
				case ObjectTypeCode.Enum:
					WriteEnum(writer, objType, obj);
					break;
				case ObjectTypeCode.Array:
					WriteArray(writer, objType, obj);
					break;
				case ObjectTypeCode.List:
					WriteList(writer, objType, obj);
					break;
				case ObjectTypeCode.Dictionary:
					WriteDictionary(writer, objType, obj);
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private static void WriteGenericTypeArgument(BinaryWriter writer, Type type) {
			var objTypeCode = SerializerUtil.GetObjectTypeCode(type, null);
			writer.Write((byte)objTypeCode);

			switch (objTypeCode) {
				case ObjectTypeCode.Serializable:
					writer.Write(type.AssemblyQualifiedName);
					break;
				case ObjectTypeCode.Primitive: {
					var primitiveCode = SerializerUtil.GetPrimitiveCode(type);
					writer.Write(SerializerUtil.IsNullable(type));
					writer.Write((byte)primitiveCode);
					break;
				}
				case ObjectTypeCode.Object:
					break;
				default:
					throw new SerializationException($"The type {type} is not a supported generic agument type");
			}
		}

		private static void WriteDictionary(BinaryWriter writer, Type objectType, object obj) {
			if (objectType.IsGenericTypeOf(typeof(IDictionary<,>))) {
				writer.Write(objectType.GetGenericTypeDefinition().AssemblyQualifiedName);

				var parameters = objectType.GenericTypeArguments;

				WriteGenericTypeArgument(writer, parameters[0]);
				WriteGenericTypeArgument(writer, parameters[1]);

				if (obj == null)
					return;

				var count = (int)obj.GetType().GetRuntimeProperty("Count").GetValue(obj, null);
				writer.Write(count);

				var enumerable = (IEnumerable)obj;
				foreach (var pair in enumerable) {
					var key = pair.GetType().GetRuntimeProperty("Key").GetValue(pair);
					var value = pair.GetType().GetRuntimeProperty("Value").GetValue(pair);

					WriteObject(writer, parameters[0], key);
					WriteObject(writer, parameters[1], value);
				}
			} else if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo())) {
				writer.Write(objectType.AssemblyQualifiedName);

				if (obj == null)
					return;

				var dictionary = (IDictionary)obj;
				writer.Write(dictionary.Count);

				foreach (DictionaryEntry entry in dictionary) {
					WriteObject(writer, entry.Key.GetType(), entry.Key);

					var valueType = entry.Value == null ? typeof(object) : entry.Value.GetType();
					WriteObject(writer, valueType, entry.Value);
				}
			} else {
				throw new NotSupportedException();
			}
		}

		private static void WriteList(BinaryWriter writer, Type objectType, object obj) {
			if (objectType.IsGenericTypeOf(typeof(IList<>))) {
				writer.Write(objectType.GetGenericTypeDefinition().AssemblyQualifiedName);

				var parameterType = objectType.GenericTypeArguments[0];
				WriteGenericTypeArgument(writer, parameterType);

				if (obj == null)
					return;

				var count = (int) obj.GetType().GetRuntimeProperty("Count").GetValue(obj);
				writer.Write(count);

				var enumerable = (IEnumerable) obj;

				foreach (var item in enumerable) {
					WriteObject(writer, parameterType, item);
				}
			} else if (typeof(IList).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo())) {
				writer.Write(objectType.AssemblyQualifiedName);

				if (obj == null)
					return;

				var list = (IList) obj;

				var count = list.Count;
				writer.Write(count);

				for (int i = 0; i < count; i++) {
					var item = list[i];
					var valueType = item == null ? typeof(object) : item.GetType();
					WriteObject(writer, valueType, item);
				}
			} else {
				throw new NotSupportedException();
			}
		}

		private static void WriteArray(BinaryWriter writer, Type objectType, object obj) {
			var elementType = objectType.GetElementType();
			var elemTypeCode = SerializerUtil.GetObjectTypeCode(elementType, obj);

			writer.Write((byte)elemTypeCode);

			switch (elemTypeCode) {
				case ObjectTypeCode.Primitive:
					var primitiveTypeCode = SerializerUtil.GetPrimitiveCode(elementType);
					writer.Write((byte)primitiveTypeCode);
					break;
				case ObjectTypeCode.Serializable:
					var name = elementType.AssemblyQualifiedName;

					writer.Write(name);
					break;
				case ObjectTypeCode.Object:
					break;
				default:
					throw new NotSupportedException($"Array of type {elementType} is not supported");
			}

			if (obj == null)
				return;

			var array = (Array)obj;

			if (array.Rank > 1)
				throw new NotSupportedException("Multi-dimensional arrays not supported");

			writer.Write(array.Length);

			for (int i = 0; i < array.Length; i++) {
				var value = array.GetValue(i);
				WriteObject(writer, elementType, value);
			}
		}

		private static void WritePrimitive(BinaryWriter writer, Type objectType, object value) {
			var code = SerializerUtil.GetPrimitiveCode(objectType);
			var nullable = SerializerUtil.IsNullable(objectType);
			writer.Write(nullable);
			writer.Write((byte) code);

			if (value == null)
				return;

			switch (code) {
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

		private static void WriteMember(BinaryWriter writer, SerializationMemberInfo memberInfo) {
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

		#endregion

		#region Deserialize

		public object Deserialize(Type type, Stream inputStream, Encoding encoding) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (inputStream == null)
				throw new ArgumentNullException(nameof(inputStream));

			if (!inputStream.CanRead)
				throw new ArgumentException("The stream cannot be read", nameof(inputStream));

			var reader = new BinaryReader(inputStream, encoding);
			Type resultType;
			var value = ReadObject(reader, out resultType);
			if (resultType != type)
				throw new SerializationException($"The read type was {resultType}: expected {type}");

			return value;
		}

		public object Deserialize(Type type, Stream inputStream)
			=> Deserialize(type, inputStream, Encoding.Unicode);


		private static object ReadObject(BinaryReader reader, out Type objectType) {
			var objTypeCode = (ObjectTypeCode) reader.ReadByte();
			var nullState = reader.ReadByte() == 0;

			switch (objTypeCode) {
				case ObjectTypeCode.Primitive:
					return ReadPrimitive(reader, nullState, out objectType);
				case ObjectTypeCode.Enum:
					return ReadEnum(reader, nullState, out objectType);
				case ObjectTypeCode.Serializable:
					return ReadSerialized(reader, nullState, out objectType);
				case ObjectTypeCode.Array:
					return ReadArray(reader, nullState, out objectType);
				case ObjectTypeCode.List:
					return ReadList(reader, nullState, out objectType);
				case ObjectTypeCode.Dictionary:
					return ReadDictionary(reader, nullState, out objectType);
				case ObjectTypeCode.Object: {
					return ReadUnknownObject(reader, nullState, out objectType);
				}
				default:
					throw new SerializationException("Corrupted serialization");
			}
		}

		private static object ReadUnknownObject(BinaryReader reader, bool nullState, out Type objectType) {
			if (nullState) {
				objectType = typeof(object);
				return null;
			}

			var typeCode = (ObjectTypeCode) reader.ReadByte();

			switch (typeCode) {
				case ObjectTypeCode.Enum:
					return ReadEnum(reader, false, out objectType);
				case ObjectTypeCode.Array:
					return ReadArray(reader, false, out objectType);
				case ObjectTypeCode.Serializable:
					return ReadSerialized(reader, false, out objectType);
				case ObjectTypeCode.Primitive:
					return ReadPrimitive(reader, false, out objectType);
				case ObjectTypeCode.List:
					return ReadList(reader, false, out objectType);
				case ObjectTypeCode.Dictionary:
					return ReadDictionary(reader, false, out objectType);
				default:
					throw new SerializationException();
			}
		}

		private static Type ReadGenericArgumentType(BinaryReader reader) {
			var objTypeCode = (ObjectTypeCode) reader.ReadByte();
			switch (objTypeCode) {
				case ObjectTypeCode.Primitive: {
					var nullable = reader.ReadBoolean();
					var primitiveTypeCode = (PrimitiveTypeCode) reader.ReadByte();
					var primitiveType = SerializerUtil.GetObjectType(primitiveTypeCode);
					return nullable ? typeof(Nullable<>).MakeGenericType(primitiveType) : primitiveType;
				}
				case ObjectTypeCode.Serializable:
					return Type.GetType(reader.ReadString(), true, true);
				case ObjectTypeCode.Object:
					return typeof(object);
				default:
					throw new NotSupportedException();
			}
		}

		private static object ReadDictionary(BinaryReader reader, bool isNull, out Type objectType) {
			var typeName = reader.ReadString();
			objectType = Type.GetType(typeName, true, true);

			PropertyInfo itemProp;

			if (objectType.IsGenericTypeOf(typeof(IDictionary<,>))) {
				var argType1 = ReadGenericArgumentType(reader);
				var argType2 = ReadGenericArgumentType(reader);

				objectType = objectType.MakeGenericType(argType1, argType2);

				itemProp = objectType.GetRuntimeProperty("Item");
			} else if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo())) {
				itemProp = objectType.GetRuntimeProperty("Item");
			} else {
				throw new NotSupportedException();
			}

			var dictionary = Activator.CreateInstance(objectType);
			var count = reader.ReadInt32();

			for (int i = 0; i < count; i++) {
				Type keyType;
				var key = ReadObject(reader, out keyType);

				Type valueType;
				var value = ReadObject(reader, out valueType);

				itemProp.SetValue(dictionary, value, new object[]{key});
			}

			return dictionary;
		}

		private static object ReadList(BinaryReader reader, bool isNull, out Type objectType) {
			var typeName = reader.ReadString();
			objectType = Type.GetType(typeName, true, true);

			MethodInfo addMethod;

			if (objectType.IsGenericTypeOf(typeof(IList<>))) {
				var parameterType = ReadGenericArgumentType(reader);
				var listType = objectType.MakeGenericType(parameterType);

				objectType = listType;

				if (isNull)
					return null;

				addMethod = objectType.GetRuntimeMethod("Add", new Type[] {parameterType});
			} else if (typeof(IList).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo())) {
				if (isNull)
					return null;

				addMethod = objectType.GetRuntimeMethod("Add", new Type[] {typeof(object)});
			} else {
				throw new NotSupportedException();
			}

			var list = Activator.CreateInstance(objectType);

			var count = reader.ReadInt32();
			for (int i = 0; i < count; i++) {
				Type itemType;
				var item = ReadObject(reader, out itemType);

				addMethod.Invoke(list, new object[] {item});
			}

			return list;
		}

		private static object ReadArray(BinaryReader reader, bool isNull, out Type objectType) {
			var elemObjType = (ObjectTypeCode) reader.ReadByte();
			Type elementType;

			if (elemObjType == ObjectTypeCode.Primitive) {
				var primitiveTypeCode = (PrimitiveTypeCode) reader.ReadByte();
				elementType = SerializerUtil.GetObjectType(primitiveTypeCode);
			} else if (elemObjType == ObjectTypeCode.Serializable) {
				var typeName = reader.ReadString();
				elementType = Type.GetType(typeName, true, true);
			} else {
				throw new NotSupportedException();
			}

			objectType = elementType.MakeArrayType();

			if (isNull)
				return null;

			var length = reader.ReadInt32();
			var array = Array.CreateInstance(elementType, length);

			for (int i = 0; i < length; i++) {
				Type itemType;
				var item = ReadObject(reader, out itemType);

				if (!elementType.GetTypeInfo().IsAssignableFrom(itemType.GetTypeInfo()))
					throw new SerializationException();

				array.SetValue(item, i);
			}

			return array;
		}

		private static object ReadSerialized(BinaryReader reader, bool isNull, out Type objectType) {
			var typeName = reader.ReadString();
			objectType = Type.GetType(typeName, true, true);

			if (isNull)
				return null;

			var type = objectType;
			if (!SerializerUtil.IsInstantiable(objectType)) {
				var realTypeName = reader.ReadString();
				type = Type.GetType(realTypeName, true, true);
			}

			var memberCount = reader.ReadInt32();
			var info = new SerializationInfo(objectType);

			for (int i = 0; i < memberCount; i++) {
				ReadMember(reader, info);
			}

			return SerializerUtil.CreateObject(type, info);
		}

		private static void ReadMember(BinaryReader reader, SerializationInfo info) {
			var memberName = reader.ReadString();
			Type memberType;
			var value = ReadObject(reader, out memberType);

			info.SetValue(memberName, memberType, value);
		}

		private static object ReadEnum(BinaryReader reader, bool isNull, out Type objectType) {
			var typeName = reader.ReadString();
			objectType = Type.GetType(typeName, true, true);

			Type primitiveType;
			var primitive = ReadPrimitive(reader, isNull, out primitiveType);

			return Enum.ToObject(objectType, primitive);
		}

		private static object ReadPrimitive(BinaryReader reader, bool isNull, out Type objectType) {
			var nullable = reader.ReadBoolean();
			var typeCode = (PrimitiveTypeCode) reader.ReadByte();
			objectType = SerializerUtil.GetObjectType(typeCode);

			if (nullable)
				objectType = typeof(Nullable<>).MakeGenericType(objectType);

			if (isNull)
				return null;

			switch (typeCode) {
				case PrimitiveTypeCode.Boolean:
					return reader.ReadBoolean();
				case PrimitiveTypeCode.Byte:
					return reader.ReadByte();
				case PrimitiveTypeCode.SByte:
					return reader.ReadSByte();
				case PrimitiveTypeCode.Int16:
					return reader.ReadInt16();
				case PrimitiveTypeCode.UInt16:
					return reader.ReadUInt16();
				case PrimitiveTypeCode.Int32:
					return reader.ReadInt32();
				case PrimitiveTypeCode.UInt32:
					return reader.ReadUInt32();
				case PrimitiveTypeCode.Int64:
					return reader.ReadInt64();
				case PrimitiveTypeCode.UInt64:
					return reader.ReadUInt64();
				case PrimitiveTypeCode.Single:
					return reader.ReadSingle();
				case PrimitiveTypeCode.Double:
					return reader.ReadDouble();
				case PrimitiveTypeCode.Char:
					return reader.ReadChar();
				case PrimitiveTypeCode.String:
					return reader.ReadString();
				default:
					throw new SerializationException();
			}
		}

		#endregion
	}
}