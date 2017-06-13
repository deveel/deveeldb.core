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
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Sql;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public abstract class QueryPlanNodeBase : IQueryPlanNode {
		private Dictionary<string, object> metadata;

		protected QueryPlanNodeBase(SerializationInfo info) {
			metadata = info.GetValue<Dictionary<string, object>>("meta");
		}

		protected QueryPlanNodeBase() {
		}

		int IComparable.CompareTo(object obj) {
			throw new NotSupportedException();
		}

		int IComparable<ISqlValue>.CompareTo(ISqlValue other) {
			throw new NotSupportedException();
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			return false;
		}

		string IQueryPlanNode.NodeName => NodeName;

		protected virtual string NodeName {
			get {
				var typeName = GetType().Name;
				if (typeName.EndsWith("Node", StringComparison.OrdinalIgnoreCase)) {
					typeName = typeName.Substring(0, typeName.Length - 4);
				}

				return typeName;
			}
		}

		protected virtual void GetData(IDictionary<string, object> data) {
			
		}

		void ISerializable.GetObjectData(SerializationInfo info) {
			info.SetValue("meta", metadata);
			GetObjectData(info);
		}

		protected virtual void GetObjectData(SerializationInfo info) {
		}

		IDictionary<string, object> IQueryPlanNode.Data {
			get {
				metadata = new Dictionary<string, object>();
				GetData(metadata);
				return metadata;
			}
		}

		protected virtual IQueryPlanNode[] ChildNodes => new IQueryPlanNode[0];

		IQueryPlanNode[] IQueryPlanNode.ChildNodes => ChildNodes;

		public abstract Task<ITable> ReduceAsync(IContext context);
	}
}