namespace Microsoft.Web.Http.Routing
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the behavior of a path content segment.
    /// </summary>
    public interface IPathContentSegment : IPathSegment
    {
        /// <summary>
        /// Gets a value indicating whether the segment represents a "catch all".
        /// </summary>
        /// <value>True if the segment represents a "catch all" (*); otherwise, false.</value>
        bool IsCatchAll { get; }

        /// <summary>
        /// Gets a read-only list of subsegments.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="IPathSubsegment">subsegments</see>.</value>
        IReadOnlyList<IPathSubsegment> Subsegments { get; }
    }
}