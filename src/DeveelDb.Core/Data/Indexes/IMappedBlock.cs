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

using Deveel.Data.Sql;
using Deveel.Data.Storage;

namespace Deveel.Data.Indexes {
	public interface IMappedBlock : IIndexBlock<SqlObject, long> {
		long FirstEntry { get; }

		long LastEntry { get; }

		long BlockPointer { get; }

		byte CompactType { get; }


		long CopyTo(IStore destStore);

		long Flush();
	}
}