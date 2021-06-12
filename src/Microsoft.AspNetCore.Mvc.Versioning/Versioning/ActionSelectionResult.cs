namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents an API versioning action selection result for which a versioning policy can be applied.
    /// </summary>
    [CLSCompliant( false )]
    public class ActionSelectionResult
    {
        HashSet<ActionDescriptor>? candidateActions;
        HashSet<ActionDescriptor>? matchingActions;

        /// <summary>
        /// Gets the best action descriptor match.
        /// </summary>
        /// <value>The best <see cref="ActionDescriptor">action descriptor</see> match or <c>null</c>.</value>
        /// <remarks>This property returns the first occurrence of a single match in the earliest iteration. If
        /// no matches exist in any iteration or multiple matches exist, this property returns <c>null</c>.</remarks>
        [CLSCompliant( false )]
        public ActionDescriptor? BestMatch => matchingActions?.FirstOrDefault();

        /// <summary>
        /// Gets a read-only collection of candidate actions.
        /// </summary>
        /// <value>A <see cref="IReadOnlyCollection{T}">read-only collection</see> of candidate <see cref="ActionDescriptor">actions</see>.</value>
        [CLSCompliant( false )]
        public IReadOnlyCollection<ActionDescriptor> CandidateActions => candidateActions ??= new();

        /// <summary>
        /// Gets a read-only collection of matching actions.
        /// </summary>
        /// <value>A <see cref="IReadOnlyCollection{T}">read-only collection</see> of <see cref="ActionDescriptor">matching actions</see>.</value>
        [CLSCompliant( false )]
        public IReadOnlyCollection<ActionDescriptor> MatchingActions => matchingActions ??= new();

        /// <summary>
        /// Adds the specified candidate actions to the selection result.
        /// </summary>
        /// <param name="actions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ActionDescriptor">actions</see>
        /// to add to the selection result.</param>
        [CLSCompliant( false )]
        public void AddCandidates( IEnumerable<ActionDescriptor> actions ) =>
            ( candidateActions ??= new() ).AddRange( actions ?? throw new ArgumentNullException( nameof( actions ) ) );

        /// <summary>
        /// Adds the specified matching actions to the selection result.
        /// </summary>
        /// <param name="matches">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ActionDescriptor">matching actions</see>
        /// to add to the selection result.</param>
        [CLSCompliant( false )]
        public void AddMatches( IEnumerable<ActionDescriptor> matches ) =>
            ( matchingActions ??= new() ).AddRange( matches ?? throw new ArgumentNullException( nameof( matches ) ) );

        /// <summary>
        /// Clears the selection result.
        /// </summary>
        /// <remarks>The selection result should only ever be cleared if the routing middleware will be re-executed.</remarks>
        public void Clear()
        {
            candidateActions?.Clear();
            matchingActions?.Clear();
        }
    }
}