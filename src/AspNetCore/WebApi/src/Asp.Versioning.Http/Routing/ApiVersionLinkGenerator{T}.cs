// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Routing;

/// <summary>
/// Represents an API version aware <see cref="LinkGenerator">link generator</see> that can
/// be used as a decorator.
/// </summary>
/// <typeparam name="T">The decorated type of <see cref="LinkGenerator">link generator</see>.</typeparam>
/// <remarks>This type is meant to be used as a Decorator when combined with dependency injection.</remarks>
[CLSCompliant( false )]
public sealed class ApiVersionLinkGenerator<T> : ApiVersionLinkGenerator where T : LinkGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionLinkGenerator{T}"/> class.
    /// </summary>
    /// <param name="linkGenerator">The inner <see cref="LinkGenerator">link generator</see>.</param>
    public ApiVersionLinkGenerator( T linkGenerator ) : base( linkGenerator ) { }
}