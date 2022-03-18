// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

internal enum ODataRouteActionType
{
    Unknown,
    EntitySet,
    BoundOperation,
    UnboundOperation,
    Singleton,
}