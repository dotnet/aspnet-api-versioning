// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Defines the behavior of an endpoint inspector.
/// </summary>
[CLSCompliant( false )]
public interface IEndpointInspector
{
    /// <summary>
    /// Determines whether the specified endpoint is a controller action.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint">endpoint</see> to inspect.</param>
    /// <returns>True if the <paramref name="endpoint"/> is for a controller action; otherwise, false.</returns>
    bool IsControllerAction( Endpoint endpoint );
}