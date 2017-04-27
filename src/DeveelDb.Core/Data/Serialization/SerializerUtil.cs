using System;
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
				return ctor.Invoke(null, new object[] { info });
			} catch (TargetInvocationException ex) {
				throw new SerializationException($"Could not construct type {objectType} because of an error", ex.InnerException);
			} catch(SerializationException) {
				throw;
			} catch (Exception ex) {
				throw new SerializationException($"Could not construct type {objectType} because of an error", ex);
			}
		}
	}
}