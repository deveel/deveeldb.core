using System;

namespace Deveel.Data.Sql.Statements {
	 public class SqlStatementException : SqlException {
		 public SqlStatementException(string message, Exception innerException)
			 : base(message, innerException) {
		 }

		 public SqlStatementException(string message)
			 : base(message) {
		 }

		 public SqlStatementException() {
		 }
	}
}