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

using Deveel.Data.Services;

namespace Deveel.Data {
	/// <summary>
	/// The base implementation of a <see cref="IContext"/> that
	/// defines a scope where to services are stored.
	/// </summary>
	/// <remarks>
	/// This object is convenient for the implementation of other
	/// contexts, since it handles the initialization and disposal
	/// of the <see cref="IScope"/> that it wraps.
	/// </remarks>
	public class Context : IContext {
		/// <summary>
		/// Constructs a new context that has no parent.
		/// </summary>
		public Context(string name)
			: this(null, name) {
		}

		/// <summary>
		/// Constructs a context that is the child of the given other context.
		/// </summary>
		/// <param name="parent">The optional parent context.</param>
		/// <param name="name">A name that identifies the context</param>
		/// <remarks>
		/// The <paramref name="parent"/> context is not required to be <c>not null</c>:
		/// if <c>null</c> then this context will have no parent.
		/// </remarks>
		public Context(IContext parent, string name)
			: this(parent, name, null) {
		}

		internal Context(IContext parent, string name, Action<IScope> scopeInit) {
			ParentContext = parent;
			ContextName = name;
			InitScope(scopeInit);
		}

		~Context() {
			Dispose(false);
		}

		/// <summary>
		/// When overridden by a derived class, this property returns
		/// a unique name that identifies the context within a global scope.
		/// </summary>
		protected string ContextName { get; }

		/// <summary>
		/// Gets a scope specific for this context, that is used
		/// to resolve services registered within this context
		/// or parent contexts.
		/// </summary>
		protected IScope ContextScope { get; private set; }

		protected IContext ParentContext { get; private set; }

		IContext IContext.ParentContext => ParentContext;

		IScope IContext.Scope => ContextScope;

		string IContext.ContextName => ContextName;

		private void InitScope(Action<IScope> scopeInit) {
			if (ParentContext != null && ParentContext.Scope != null) {
				ContextScope = ParentContext.Scope.OpenScope(ContextName);

				Action<IScope> configure = Configure;
				if (scopeInit != null)
					configure = (Action<IScope>) Delegate.Combine(configure, scopeInit);

				configure(ContextScope);
				ContextScope = ContextScope.AsReadOnly();
			}
		}

		protected virtual void Configure(IScope scope) {
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (ContextScope != null)
					ContextScope.Dispose();
			}

			ContextScope = null;
			ParentContext = null;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}