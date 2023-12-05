// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;
using System.Web.Http;

/// <content>
/// Provides the implementation for ASP.NET Web API.
/// </content>
public partial class UrlSegmentApiVersionReader
{
    /// <inheritdoc />
    public virtual IReadOnlyList<string> Read( HttpRequestMessage request )
    {
        ArgumentNullException.ThrowIfNull(request);

        if ( reentrant )
        {
            return Array.Empty<string>();
        }

        reentrant = true;
        var versions = request.ApiVersionProperties().RawRequestedApiVersions;
        reentrant = false;

        return versions;
    }
}