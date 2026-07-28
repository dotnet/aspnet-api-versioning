// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

/// <summary>
/// Creates the endpoints synthesized by the API versioning <see cref="ApiVersionMatcherPolicy">matcher policy</see> to
/// report a client error.
/// </summary>
/// <remarks>
/// A client error endpoint is always a fallback; it must never win candidate selection over a real endpoint it happens
/// to be grouped with. The routing system sorts the endpoints of a node with <c>EndpointComparer</c>, which orders any
/// endpoint that is not a <see cref="RouteEndpoint"/> before every endpoint that is. A client error endpoint is,
/// therefore, a <see cref="RouteEndpoint"/> with the lowest possible order so that it always sorts last and is only
/// selected when nothing else matched.
/// </remarks>
internal static class ClientErrorEndpoint
{
    private static RoutePattern? empty;

    private static RoutePattern Empty => empty ??= RoutePatternFactory.Parse( string.Empty );

    internal static RouteEndpoint New( RequestDelegate requestDelegate, string displayName ) =>
        new( requestDelegate, Empty, int.MaxValue, EndpointMetadataCollection.Empty, displayName );
}