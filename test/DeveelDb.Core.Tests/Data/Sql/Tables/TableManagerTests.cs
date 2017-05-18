using System;

using Deveel.Data.Transactions;

using Moq;

namespace Deveel.Data.Sql.Tables {
	public class TableManagerTests : IDisposable {
		private ITransaction context;

		public TableManagerTests() {
			var mock = new Mock<ITransaction>();

			context = mock.Object;
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}