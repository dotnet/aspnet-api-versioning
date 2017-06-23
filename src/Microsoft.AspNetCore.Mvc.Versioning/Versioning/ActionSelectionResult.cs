namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static System.Threading.Interlocked;

    /// <summary>
    /// Represents an API versioning action selection result for which a versioning policy can be applied.
    /// </summary>
    public class ActionSelectionResult
    {
        readonly Dictionary<int, ICollection<ActionDescriptor>> candidateActions = new Dictionary<int, ICollection<ActionDescriptor>>();
        readonly Dictionary<int, ICollection<ActionDescriptorMatch>> matchingActions = new Dictionary<int, ICollection<ActionDescriptorMatch>>();
        ActionDescriptorMatch bestMatch;

        /// <summary>
        /// Gets the number of action selection iterations that have occurred.
        /// </summary>
        /// <value>The total number of action selection iterations that have occurred. The default value is zero.</value>
        public int Iterations { get; private set; }

        /// <summary>
        /// Gets a value indicating whether are higher precendence matches in previous iterations.
        /// </summary>
        /// <value>True if there are higher precendence matches in one or more previous iterations;
        /// otherwise, false. The default value is <c>false</c>.</value>
        public bool HasMatchesInPreviousIterations { get; private set; }

        /// <summary>
        /// Gets the best action descriptor match.
        /// </summary>
        /// <value>The best <see cref="ActionDescriptorMatch">action descriptor match</see> or <c>null</c>.</value>
        /// <remarks>This property returns the first occurrence of a single match in the earliest iteration. If
        /// no matches exist in any iteration or multiple matches exist, this property returns <c>null</c>.</remarks>
        [CLSCompliant( false )]
        public ActionDescriptorMatch BestMatch => bestMatch;

        /// <summary>
        /// Gets a collection of candidate actions grouped by action selection iteration.
        /// </summary>
        /// <value>A <see cref="IReadOnlyDictionary{TKey, TValue}">read-only dictionary</see> of candidate
        /// <see cref="ActionDescriptor">actions</see> per action selection iteration.</value>
        [CLSCompliant( false )]
        public IReadOnlyDictionary<int, ICollection<ActionDescriptor>> CandidateActions => candidateActions;

        /// <summary>
        /// Gets a collection of matching actions grouped by action selection iteration.
        /// </summary>
        /// <value>A <see cref="IReadOnlyDictionary{TKey, TValue}">read-only dictionary</see> of
        /// <see cref="ActionDescriptorMatch">matching actions</see> per action selection iteration.</value>
        [CLSCompliant( false )]
        public IReadOnlyDictionary<int, ICollection<ActionDescriptorMatch>> MatchingActions => matchingActions;

        /// <summary>
        /// Adds the specified candidate actions to the selection result.
        /// </summary>
        /// <param name="actions">The array of <see cref="ActionDescriptor">actions</see> to add to the selection result.</param>
        [CLSCompliant( false )]
        public void AddCandidates( params ActionDescriptor[] actions )
        {
            Arg.NotNull( actions, nameof( actions ) );

            var key = Iterations;

            if ( !candidateActions.TryGetValue( key, out var collection ) )
            {
                candidateActions[key] = collection = new HashSet<ActionDescriptor>();
            }

            collection.AddRange( actions );
        }

        /// <summary>
        /// Adds the specified candidate actions to the selection result.
        /// </summary>
        /// <param name="actions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ActionDescriptor">actions</see>
        /// to add to the selection result.</param>
        [CLSCompliant( false )]
        public void AddCandidates( IEnumerable<ActionDescriptor> actions )
        {
            Arg.NotNull( actions, nameof( actions ) );

            var key = Iterations;

            if ( !candidateActions.TryGetValue( key, out var collection ) )
            {
                candidateActions[key] = collection = new HashSet<ActionDescriptor>();
            }

            collection.AddRange( actions );
        }

        /// <summary>
        /// Adds the specified matching actions to the selection result.
        /// </summary>
        /// <param name="matches">The array of <see cref="ActionDescriptorMatch">matching actions</see> to add to the selection result.</param>
        [CLSCompliant( false )]
        public void AddMatches( params ActionDescriptorMatch[] matches )
        {
            Arg.NotNull( matches, nameof( matches ) );

            var key = Iterations;

            if ( !matchingActions.TryGetValue( key, out var collection ) )
            {
                matchingActions[key] = collection = new HashSet<ActionDescriptorMatch>();
            }

            collection.AddRange( matches );
        }

        /// <summary>
        /// Adds the specified matching actions to the selection result.
        /// </summary>
        /// <param name="matches">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ActionDescriptorMatch">matching actions</see>
        /// to add to the selection result.</param>
        [CLSCompliant( false )]
        public void AddMatches( IEnumerable<ActionDescriptorMatch> matches )
        {
            Arg.NotNull( matches, nameof( matches ) );

            var key = Iterations;

            if ( !matchingActions.TryGetValue( key, out var collection ) )
            {
                matchingActions[key] = collection = new HashSet<ActionDescriptorMatch>();
            }

            collection.AddRange( matches );
        }

        /// <summary>
        /// Ends the current action selection iteration.
        /// </summary>
        public void EndIteration()
        {
            var key = Iterations;

            if ( !candidateActions.ContainsKey( key ) )
            {
                candidateActions.Add( key, new ActionDescriptor[0] );
            }

            if ( !matchingActions.ContainsKey( key ) )
            {
                matchingActions.Add( key, new ActionDescriptorMatch[0] );
            }

            ++Iterations;
            HasMatchesInPreviousIterations |= matchingActions[key].Count > 0;
        }

        /// <summary>
        /// Attempts to update the best match.
        /// </summary>
        /// <param name="match">The <see cref="ActionDescriptorMatch">match</see> to attempt to set as the best match.
        /// This value can be <c>null</c>.</param>
        /// <returns>True if the match is successfully set; otherwise, false.</returns>
        [CLSCompliant( false )]
        public bool TrySetBestMatch( ActionDescriptorMatch match )
        {
            if ( match == null || HasMatchesInPreviousIterations )
            {
                return false;
            }

            const ActionDescriptorMatch NoMatch = default( ActionDescriptorMatch );
            return CompareExchange( ref bestMatch, match, NoMatch ) == NoMatch;
        }
    }
}