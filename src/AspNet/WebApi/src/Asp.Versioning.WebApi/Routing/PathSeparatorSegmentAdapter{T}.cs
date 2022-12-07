// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

internal sealed class PathSeparatorSegmentAdapter<T> : IPathSeparatorSegment where T : notnull
{
    private readonly T adapted;

    public PathSeparatorSegmentAdapter( T adapted ) => this.adapted = adapted;

    public override string ToString() => adapted.ToString();
}