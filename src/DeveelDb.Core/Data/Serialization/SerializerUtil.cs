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

		public static ObjectTypeCode GetObjectTypeCode(Type objecType) {
			if (objecType.IsArray)
				return ObjectTypeCode.Array;
			if (IsPrimitive(objecType))
				return ObjectTypeCode.Primitive;
			if (IsList(objecType))
				return ObjectTypeCode.List;
			if (IsDictionary(objecType))
				return ObjectTypeCode.Dictionary;

			if (typeof(ISerializable).GetTypeInfo().IsAssignableFrom(objecType.GetTypeInfo()))
				return ObjectTypeCode.Serializable;

			if (objecType == typeof(object))
				return ObjectTypeCode.Object;

			throw new NotSupportedException($"The type {objecType} is not serializable");
		}

		public static bool IsPrimitive(Type type) {
			return type.GetTypeInfo().IsPrimitive ||
			       type == typeof(string);
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