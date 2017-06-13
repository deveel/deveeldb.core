﻿// 
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

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	/// <summary>
	/// An interface to an object that describes characteristics of a table based
	/// object in the database.
	/// </summary>
	/// <remarks>
	/// This can represent anything that evaluates to a <see cref="ITable"/>
	/// when the query plan is evaluated. It is used to represent data tables and views.
	/// <para>
	/// This object is used by the planner to see ahead of time what sort of table
	/// we are dealing with. 
	/// </para>
	/// <para>
	/// For example, a view is stored with a <see cref="TableInfo"/>
	/// describing the resultant columns, and the <see cref="IQueryPlanNode"/>
	/// to produce the view result. The query planner requires the information in 
	/// <see cref="TableInfo"/> to resolve references in the query, and the 
	/// <see cref="IQueryPlanNode"/> to add into the resultant plan tree.
	/// </para>
	/// </remarks>
	public interface ITableQueryInfo {
		/// <summary>
		/// Gets a read-only <see cref="TableInfo"/> that describes 
		/// the columns in this table source, and the name of the table.
		/// </summary>
		TableInfo TableInfo { get; }

		/// <summary>
		/// Gets a <see cref="IQueryPlanNode"/> that can be put into a plan 
		/// tree and can be evaluated to find the result of the table.
		/// </summary>
		/// <remarks>
		/// This property should always return a new object representing 
		/// the query plan.
		/// </remarks>
		Task<IQueryPlanNode> GetQueryPlanAsync();
	}
}