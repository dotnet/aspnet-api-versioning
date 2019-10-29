#pragma warning disable CA1812

namespace Microsoft.Web.Http.Routing
{
    using System;

    sealed class PathSeparatorSegmentAdapter<T> : IPathSeparatorSegment where T : notnull
    {
        readonly T adapted;

        public PathSeparatorSegmentAdapter( T adapted ) => this.adapted = adapted;

        public override string ToString() => adapted.ToString();
    }
}