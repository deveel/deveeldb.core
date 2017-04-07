using System;
using System.Globalization;

namespace Deveel.Data.Sql {
	public interface ISqlStringSearch {
		/// <summary>
		/// This is the pattern match recurrsive method.
		/// </summary>
		/// <remarks>
		/// It recurses on each wildcard expression in the pattern which makes 
		/// for slightly better efficiency than a character recurse algorithm.
		/// However, patterns such as <c>_%_a</c> will result in many recursive 
		/// calls.
		/// <para>
		/// <b>Note</b> That <c>_%_</c> will be less efficient than <c>__%</c> 
		/// and will produce the same result.
		/// </para>
		/// <para>
		/// <b>Note</b> It requires that a wild card character is the first 
		/// character in the expression.
		/// </para>
		/// <para>
		/// <b>Issue</b> Pattern optimiser, we should optimize wild cards of 
		/// type <c>%__</c> to <c>__%</c>, or <c>%__%_%_%</c> to <c>____%</c>. 
		/// Optimised forms are identical in result and more efficient. This 
		/// optimization could be performed by the client during parsing of 
		/// the <i>LIKE</i> statement.
		/// </para>
		/// <para>
		/// <b>Hacking Issue</b> Badly formed wild cards may result in hogging 
		/// of server side resources.
		/// </para>
		/// </remarks>
		bool Matches(ISqlString source, string pattern, char escapeChar);
	}
}