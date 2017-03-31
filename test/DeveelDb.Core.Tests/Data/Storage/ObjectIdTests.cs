using System;

using Xunit;

namespace Deveel.Data.Storage {
	public class ObjectIdTests {
		[Theory]
		[InlineData(1, 9200L, 1, 9200L, true)]
		[InlineData(3, 10025L, 3, 10022L, false)]
		public void IdEquals(int store1, long obj1, int store2, long obj2, bool expected) {
			var objId1 = new ObjectId(store1, obj1);
			var objId2 = new ObjectId(store2, obj2);

			Assert.Equal(expected, objId1.Equals(objId2));
		}

		[Theory]
		[InlineData(3, 8299L, "0x00000003:0x0000206B")]
		public void GetString(int store, long id, string expected) {
			var objId = new ObjectId(store, id);

			Assert.Equal(store, objId.StoreId);
			Assert.Equal(id, objId.Id);
			Assert.Equal(expected, objId.ToString());
		}
	}
}