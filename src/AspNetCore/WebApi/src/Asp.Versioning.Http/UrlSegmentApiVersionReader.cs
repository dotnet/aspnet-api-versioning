// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;

/// <content>
/// Provides the implementation for ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class UrlSegmentApiVersionReader
{
    /// <inheritdoc />
    public virtual IReadOnlyList<string> Read( HttpRequest request )
    {
        ArgumentNullException.ThrowIfNull( request );

        if ( reentrant )
        {
            return Array.Empty<string>();
        }

        reentrant = true;
        var feature = request.HttpContext.ApiVersioningFeature();
        var versions = feature.RawRequestedApiVersions;
        reentrant = false;

        return versions;
    }
}