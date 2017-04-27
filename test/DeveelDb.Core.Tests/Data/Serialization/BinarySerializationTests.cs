using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Xunit;

namespace Deveel.Data.Serialization {
	public class BinarySerializationTests {
		private static object Serialize(object value) {
			var stream = new MemoryStream();
			var serializer = new BinarySerializer();
			serializer.Serialize(value, stream);

			stream.Seek(0, SeekOrigin.Begin);

			return serializer.Deserialize(value.GetType(), stream);
		}

		[Fact]
		public void SerializeArrayOfPrimitives() {
			var array = new float[2] {2.0f, 0.178f};

			var result = Serialize(array);

			Assert.NotNull(result);
			Assert.IsType<float[]>(result);
			Assert.Equal(2, ((Array)result).Length);
		}

		[Fact]
		public void SerializeGenericListOfPrimitives() {
			var list = new List<long> {
				4533, 911029, 122333433
			};

			var result = Serialize(list);

			Assert.NotNull(result);
			Assert.IsType<List<long>>(result);
			Assert.Equal(list.Count, ((List<long>)result).Count);
		}

		[Fact]
		public void SerializeGenericListOfObject() {
			var list = new List<SerializableClass> {
				new SerializableClass(123, "1234", null)
			};

			var result = Serialize(list);

			Assert.NotNull(result);
			Assert.IsType<List<SerializableClass>>(result);
			Assert.Equal(list.Count, ((List<SerializableClass>)result).Count);
		}

		[Fact]
		public void SerializeOldStyleList() {
			var list = new ArrayList { 1, 673, 892};

			var result = Serialize(list);
			Assert.NotNull(result);
			Assert.IsType<ArrayList>(result);
			Assert.Equal(3, ((ArrayList)result).Count);
		}

		[Fact]
		public void SerializeGenericDictionaryOfPrimitives() {
			var dictionary = new Dictionary<string, long> {
				{"a", 3433},
				{"b", 7832},
				{"a2", 029293}
			};

			var result = Serialize(dictionary);

			Assert.NotNull(result);
			Assert.IsType<Dictionary<string, long>>(result);
			Assert.Equal(3, ((Dictionary<string, long>)result).Count);
		}

		[Fact]
		public void SerializeGenericDictionaryOfPrimitiveAndObject() {
			var dictionary = new Dictionary<string, object> {
				{"a", 3433},
				{"b", 7832},
				{"a2", "foo-bar"}
			};

			var result = Serialize(dictionary);

			Assert.NotNull(result);
			Assert.IsType<Dictionary<string, object>>(result);
			Assert.Equal(3, ((Dictionary<string, object>)result).Count);
			Assert.Equal("foo-bar", ((Dictionary<string, object>)result)["a2"]);
		}

		[Fact]
		public void SerializeGenericDictionaryOfPrimitivesAndObjects() {
			var dictionary = new Dictionary<string, SerializableClass> {
				{"a", new SerializableClass(54, "a", new SerializableClass(01, "a1", null))},
				{"b", new SerializableClass(87, "y", null)}
			};

			var result = Serialize(dictionary);

			Assert.NotNull(result);
			Assert.IsType<Dictionary<string, SerializableClass>>(result);
			Assert.Equal(2, ((Dictionary<string, SerializableClass>)result).Count);
		}

		[Fact]
		public void SerializeOldStyleDictionary() {
			var table = new Hashtable {
				{"a", 443},
				{"b", 78383},
				{"hhe", "b"}
			};

			var result = Serialize(table);

			Assert.NotNull(result);
			Assert.IsType<Hashtable>(result);
			Assert.Equal(3, ((Hashtable)result).Count);
			Assert.Equal("b", ((Hashtable)result)["hhe"]);
		}

		[Fact]
		public void SerializeClassWithSubClass() {
			var c = new SerializableClass(33, "test1", new SerializableClass(783, "sub-1", null));
			var c2 = Serialize(c);

			Assert.NotNull(c2);
			Assert.IsType<SerializableClass>(c2);
		}

		#region SerializableClass

		class SerializableClass : ISerializable {
			private readonly int field1;
			private readonly string field2;
			private readonly SerializableClass subClass;

			public SerializableClass(int field1, string field2, SerializableClass subClass) {
				this.field1 = field1;
				this.field2 = field2;
				this.subClass = subClass;
			}

			private SerializableClass(SerializationInfo info) {
				field1 = info.GetInt32("field1");
				field2 = info.GetString("field2");
				subClass = info.GetValue<SerializableClass>("subClass");
			}

			public void GetObjectData(SerializationInfo info) {
				info.SetValue("field1", field1);
				info.SetValue("field2", field2);
				info.SetValue("subClass", subClass);
			}
		}

		#endregion
	}
}