// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning.OData.Batch;

/// <summary>
/// Provides extension methods for <see cref="IApplicationBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IApplicationBuilderExtensions
{
    /// <summary>
    /// Uses API versioned OData batch middleware.
    /// </summary>
    /// <param name="app">The current <see cref="IApplicationBuilder"/>.</param>
    /// <returns>The original <see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseVersionedODataBatching( this IApplicationBuilder app )
    {
        ArgumentNullException.ThrowIfNull( app );
        return app.UseMiddleware<VersionedODataBatchMiddleware>();
    }
}