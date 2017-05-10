namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an API versioning action selection result for which a versioning policy can be applied.
    /// </summary>
    public class ActionSelectionResult
    {
        /// <summary>
        /// Gets the collection of all candidate controller actions for the current route.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of <see cref="ActionDescriptor">actions</see> candidate actions for the current route.</value>
        [CLSCompliant( false )]
        public ICollection<ActionDescriptor> CandidateActions { get; } = new HashSet<ActionDescriptor>();

        /// <summary>
        /// Gets the collection of controller actions matching the current route.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of <see cref="ActionDescriptorMatch">matching actions</see> for the current route.</value>
        [CLSCompliant( false )]
        public ICollection<ActionDescriptorMatch> MatchingActions { get; } = new HashSet<ActionDescriptorMatch>();
    }
}