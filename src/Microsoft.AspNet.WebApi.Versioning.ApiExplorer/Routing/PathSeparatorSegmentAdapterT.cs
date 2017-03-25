﻿namespace Microsoft.Web.Http.Routing
{
    using System;

    sealed class PathSeparatorSegmentAdapter<T> : IPathSeparatorSegment
    {
        readonly T adapted;

        public PathSeparatorSegmentAdapter( T adapted ) => this.adapted = adapted;

        public override string ToString() => adapted.ToString();
    }
}