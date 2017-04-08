using System;

using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Variables {
	public class VariableManagerTests : IDisposable {
		private VariableManager manager;
		private IContext context;

		public VariableManagerTests() {
			var mock = new Mock<IContext>();

			context = mock.Object;
			manager = new VariableManager();
		}

		[Theory]
		[InlineData("a", SqlTypeCode.Boolean, -1, -1, true)]
		[InlineData("b", SqlTypeCode.Numeric, 120, 33, 2003933.2293)]
		public void AssignVariable(string name, SqlTypeCode typeCode, int p, int s, object value) {
			var type = PrimitiveTypes.Type(typeCode, new {precision = p, scale = s, maxSize = p});
			var obj = new SqlObject(type, SqlValueUtil.FromObject(value));

			manager.AssignVariable(name, SqlExpression.Constant(obj), context);
		}

		[Theory]
		[InlineData("a", SqlTypeCode.Boolean, -1, -1, true, "A", true)]
		[InlineData("ab", SqlTypeCode.Numeric, 120, 33, 2003933.2293, "aB", true)]
		[InlineData("ab", SqlTypeCode.Numeric, 120, 33, 2003933.2293, "ab", false)]
		public void ObjectManager_ResolveName(string name, SqlTypeCode typeCode, int p, int s, object value, string resolveName, bool ignoreCase) {
			var type = PrimitiveTypes.Type(typeCode, new { precision = p, scale = s, maxSize = p });
			var obj = new SqlObject(type, SqlValueUtil.FromObject(value));

			manager.AssignVariable(name, SqlExpression.Constant(obj), context);

			var objManager = (manager as IDbObjectManager);
			var result = objManager.ResolveName(new ObjectName(resolveName), ignoreCase);

			Assert.Equal(new ObjectName(name), result);
		}
 
		public void Dispose() {
			manager = new VariableManager();
		}
	}
}