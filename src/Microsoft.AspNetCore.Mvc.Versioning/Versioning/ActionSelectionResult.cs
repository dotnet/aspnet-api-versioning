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
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionSelectionResult"/> class.
        /// </summary>
        /// <param name="candidateActions">A <see cref="IReadOnlyList{T}">read-only list</see> of candidate <see cref="ActionDescriptor">actions</see>.</param>
        /// <param name="matchingActions">A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ActionDescriptor">matching actions</see>.</param>
        public ActionSelectionResult( IEnumerable<ActionDescriptor> candidateActions, IEnumerable<ActionDescriptor> matchingActions )
        {
            Arg.NotNull( candidateActions, nameof( candidateActions ) );
            Arg.NotNull( matchingActions, nameof( matchingActions ) );

            CandidateActions = candidateActions.ToArray();
            MatchingActions = matchingActions.ToArray();
        }

        /// <summary>
        /// Gets a read-only list of candidate actions.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of candidate <see cref="ActionDescriptor">actions</see>.</value>
        [CLSCompliant( false )]
        public IReadOnlyList<ActionDescriptor> CandidateActions { get; }

        /// <summary>
        /// Gets a read-only list of matching actions.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ActionDescriptor">matching actions</see>.</value>
        [CLSCompliant( false )]
        public IReadOnlyList<ActionDescriptor> MatchingActions { get; }
    }
}