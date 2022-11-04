// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

internal struct RouteDestination
{
    public readonly int Exit;
    public int Malformed;
    public int Ambiguous;
    public int Unspecified;
    public int UnsupportedMediaType;
    public int AssumeDefault;
    public int NotAcceptable;

    public RouteDestination( int exit )
    {
        Exit = exit;
        Malformed = exit;
        Ambiguous = exit;
        Unspecified = exit;
        UnsupportedMediaType = exit;
        AssumeDefault = exit;
        NotAcceptable = exit;
    }
}