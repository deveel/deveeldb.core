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

using Deveel.Data.Sql;

namespace Deveel.Data {
	public class SystemException : Exception {
		public SystemException(string message, Exception innerException)
			: this(ErrorCodes.System.Unknown, message, innerException) {
		}

		public SystemException(int code, string message, Exception innerException)
			: this(ErrorClasses.System, code, message, innerException) {
		}

		public SystemException(int @class, int code, string message, Exception innerException)
			: base(SystemError.Message(@class, code, message), innerException) {
			Code = code;
			Class = @class;
		}

		public SystemException(string message)
			: this(ErrorCodes.System.Unknown, message) {
		}

		public SystemException(int code, string message)
			: this(ErrorClasses.System, code, message) {
		}

		public SystemException(int @class, int code, string message)
			: base(SystemError.Message(@class, code, message)) {
			Code = Code;
			Class = @class;
		}

		public SystemException()
			: this(ErrorCodes.System.Unknown) {
		}

		public SystemException(int code)
			: this(ErrorClasses.System, code) {
		}

		public SystemException(int @class, int code)
			: this(@class, code, null) {
		}

		public int Class { get; }

		public int Code { get; }

		public override string Message => $"DDB-{Class}{Code} - {base.Message}";

		internal static string ResolveMessage(int @class, int code, string message, params object[] args) {
			return SystemError.Message(@class, code, message, args);
		}
	}
}