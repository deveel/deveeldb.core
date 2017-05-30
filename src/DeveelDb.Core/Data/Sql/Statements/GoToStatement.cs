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
using System.Threading.Tasks;

using Deveel.Data.Serialization;

namespace Deveel.Data.Sql.Statements {
	public sealed class GoToStatement : SqlStatement, IPlSqlStatement {
		public GoToStatement(string label) {
			if (String.IsNullOrWhiteSpace(label))
				throw new ArgumentNullException(nameof(label));

			Label = label;
		}

		private GoToStatement(SerializationInfo info)
			: base(info) {
			Label = info.GetString("label");
		}

		public string Label { get; }

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("label", Label);
			base.GetObjectData(info);
		}

		protected override async Task ExecuteStatementAsync(StatementContext context) {
			try {
				await context.TransferAsync(Label);
			} catch (SqlStatementException) {
				throw;
			} catch (Exception ex) {
				throw new SqlStatementException($"It was not possible to transfer the execution to statement labeled '{Label}' because of an error.", ex);
			}
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.AppendFormat("GOTO '{0}';", Label);
		}
	}
}