using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class InitializeContext : Context {
		internal InitializeContext(MethodContext context, SqlExpression input)
			: base(context, $"Seed({context.Method.MethodInfo.MethodName})") {
			Input = input;
		}

		public SqlExpression Input { get; }

		internal SqlExpression Result { get; private set; }

		internal bool Iterate { get; private set; } = true;

		public void SetResult(SqlExpression value, bool iterate = true) {
			Result = value;
			Iterate = iterate;
		}
	}
}