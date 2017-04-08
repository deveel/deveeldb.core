using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query {
	public sealed class JoinPart {
		private JoinPart(JoinType joinType, ObjectName tableName, SqlQueryExpression query, SqlExpression onExpression) {
			JoinType = joinType;
			TableName = tableName;
			Query = query;
			OnExpression = onExpression;
		}

		internal JoinPart(JoinType joinType, ObjectName tableName, SqlExpression onExpression)
			: this(joinType, tableName, null, onExpression) {
			if (tableName == null)
				throw new ArgumentNullException(nameof(tableName));
		}

		internal JoinPart(JoinType joinType, SqlQueryExpression query, SqlExpression onExpression)
			: this(joinType, null, query, onExpression) {
			if (query == null)
				throw new ArgumentNullException(nameof(query));
		}

		public SqlExpression OnExpression { get; }

		public ObjectName TableName { get; }

		public SqlQueryExpression Query { get; }

		public JoinType JoinType { get; }

		public bool IsTable => TableName != null;

		public bool IsQuery => Query != null;
	}
}