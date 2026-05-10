// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using System.Collections.Frozen;

internal struct RouteDestination
{
    public readonly int Exit;
    public int Malformed;
    public int Ambiguous;
    public int Unspecified;
    public int Unsupported;
    public int UnsupportedMediaType;
    public int AssumeDefault;
    public int NotAcceptable;
    public IReadOnlyDictionary<ApiVersion, int> IntroducedLater;

    public RouteDestination( int exit )
    {
        Exit = exit;
        Malformed = exit;
        Ambiguous = exit;
        Unspecified = exit;
        Unsupported = exit;
        UnsupportedMediaType = exit;
        AssumeDefault = exit;
        NotAcceptable = exit;
        IntroducedLater = FrozenDictionary<ApiVersion, int>.Empty;
    }
}