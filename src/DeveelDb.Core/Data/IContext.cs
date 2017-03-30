using System;

using Deveel.Data.Services;

namespace Deveel.Data {
	/// <summary>
	/// Provides context for a given state of the system
	/// </summary>
	/// <remarks>
	/// Several components of a database system are providing contexts,
	/// to handle configurations, services, variables and a scope.
	/// <para>
	/// The most common context hierarchy is the following:
	/// <list type="bullet">
	///		<listheader>
	///			<term>Context Name</term>
	///			<description></description>
	///     </listheader>
	///		<item>
	///			<term>System</term>
	///			<description>The root level context that all other contexts inherit.</description>
	///		</item>
	///		<item>
	///			<term>Database</term>
	///			<description>The context specific to a single database within a system.</description>
	///		</item>
	///		<item>
	///			<term>Session</term>
	///			<description>The context of a session between the user and the database.</description>
	///		</item>
	///		<item>
	///			<term>Transaction</term>
	///			<description>The context of a single transaction within a session.</description>
	///		</item>
	///		<item>
	///			<term>Query</term>
	///			<description>The context of a single command/query within a transaction.</description>
	///		</item>
	///		<item>
	///			<term>Block</term>
	///         <description>The context of a single execution block within a query execution plan.s</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// A context wraps a <see cref="IScope"/> instance that is disposed at the end of the context.
	/// </para>
	/// </remarks>
	public interface IContext : IDisposable {
		/// <summary>
		/// Gets the parent context of this instance, if any.
		/// </summary>
		/// <remarks>
		/// The only case in which this value is <c>null</c> is when
		/// this is the context of a system, that is the root.
		/// </remarks>
		IContext Parent { get; }

		/// <summary>
		/// Gets the name of the context.
		/// </summary>
		/// <remarks>
		/// The name of a context is important for the definition of the wrapped
		/// <see cref="Scope"/>, that is named after this value.
		/// </remarks>
		/// <seealso cref="IScope"/>
		string Name { get; }

		/// <summary>
		/// Gets a named scope for this context.
		/// </summary>
		/// <seealso cref="IScope"/>
		IScope Scope { get; }
	}
}
