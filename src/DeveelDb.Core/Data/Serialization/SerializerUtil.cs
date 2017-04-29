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
using System.Linq;
using System.Reflection;

namespace Deveel.Data.Serialization {
	static class SerializerUtil {
		public static object CreateObject(Type objectType, SerializationInfo info) {
			var ctor = objectType.GetTypeInfo()
				.DeclaredConstructors.Where(x => {
					var parameters = x.GetParameters();
					return parameters.Length == 1 && parameters[0].ParameterType == typeof(SerializationInfo);
				}).FirstOrDefault();
			if (ctor == null)
				throw new SerializationException($"The type {objectType} has no constructor for the deserialization");

			try {
				return ctor.Invoke(new object[] { info });
			} catch (TargetInvocationException ex) {
				throw new SerializationException($"Could not construct type {objectType} because of an error", ex.InnerException);
			} catch(SerializationException) {
				throw;
			} catch (Exception ex) {
				throw new SerializationException($"Could not construct type {objectType} because of an error", ex);
			}
		}

		public static PrimitiveTypeCode GetPrimitiveCode(Type type) {
			var nullableType = Nullable.GetUnderlyingType(type);
			if (nullableType != null)
				return GetPrimitiveCode(nullableType);

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

			throw new NotSupportedException($"The type {type} is not primitive");
		}

		public static ObjectTypeCode GetObjectTypeCode(Type objectType, object obj) {
			if (objectType.IsArray)
				return ObjectTypeCode.Array;
			if (IsPrimitive(objectType))
				return ObjectTypeCode.Primitive;
			if (IsNullable(objectType)) {
				var underlyingType = Nullable.GetUnderlyingType(objectType);
				return GetObjectTypeCode(underlyingType, obj);
			}
			if (objectType.GetTypeInfo().IsEnum)
				return ObjectTypeCode.Enum;
			if (IsList(objectType))
				return ObjectTypeCode.List;
			if (IsDictionary(objectType))
				return ObjectTypeCode.Dictionary;

			if (IsSerializable(objectType, obj))
				return ObjectTypeCode.Serializable;

			if (objectType == typeof(object))
				return ObjectTypeCode.Object;

			throw new NotSupportedException($"The type {objectType} is not serializable");
		}

		public static bool IsSerializable(Type objectType, object obj) {
			if (typeof(ISerializable).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo()))
				return true;

			if (!IsInstantiable(objectType)) {
				if (obj == null)
					return false;

				return IsSerializable(obj.GetType(), null);
			}

			return false;
		}

		public static bool IsInstantiable(Type objectType) {
			return !objectType.GetTypeInfo().IsAbstract &&
			       !objectType.GetTypeInfo().IsInterface;
		}

		public static bool IsPrimitive(Type type) {
			return type.GetTypeInfo().IsPrimitive ||
			       type == typeof(string);
		}

		public static bool IsNullable(Type type) {
			return Nullable.GetUnderlyingType(type) != null;
		}

		public static Type GetObjectType(PrimitiveTypeCode typeCode) {
			switch (typeCode) {
				case PrimitiveTypeCode.Boolean:
					return typeof(bool);
				case PrimitiveTypeCode.Byte:
					return typeof(byte);
				case PrimitiveTypeCode.SByte:
					return typeof(sbyte);
				case PrimitiveTypeCode.Int16:
					return typeof(short);
				case PrimitiveTypeCode.UInt16:
					return typeof(ushort);
				case PrimitiveTypeCode.Int32:
					return typeof(int);
				case PrimitiveTypeCode.UInt32:
					return typeof(uint);
				case PrimitiveTypeCode.Int64:
					return typeof(long);
				case PrimitiveTypeCode.UInt64:
					return typeof(ulong);
				case PrimitiveTypeCode.Single:
					return typeof(float);
				case PrimitiveTypeCode.Double:
					return typeof(double);
				case PrimitiveTypeCode.Char:
					return typeof(char);
				case PrimitiveTypeCode.String:
					return typeof(string);
				default:
					throw new NotSupportedException();
			}
		}

		public static bool IsList(Type type) {
			return type.IsGenericTypeOf(typeof(IList<>)) ||
			       typeof(IList).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
		}

		public static bool IsDictionary(Type type) {
			return type.IsGenericTypeOf(typeof(IDictionary<,>)) ||
			       typeof(IDictionary).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
		}

		public static bool IsGenericTypeOf(this Type t, Type genericDefinition) {
			if (!genericDefinition.GetTypeInfo().IsGenericType) {
				return false;
			}

			var isMatch = t.GetTypeInfo().IsGenericType && 
				t.GetGenericTypeDefinition() == genericDefinition.GetGenericTypeDefinition();
			if (!isMatch && t.GetTypeInfo().BaseType != null) {
				isMatch = IsGenericTypeOf(t.GetTypeInfo().BaseType, genericDefinition);
			}
			if (!isMatch && genericDefinition.GetTypeInfo().IsInterface && 
				t.GetTypeInfo().ImplementedInterfaces.Any()) {
				foreach (var i in t.GetTypeInfo().ImplementedInterfaces) {
					if (i.IsGenericTypeOf(genericDefinition)) {
						isMatch = true;
						break;
					}
				}
			}

			return isMatch;
		}
	}
}