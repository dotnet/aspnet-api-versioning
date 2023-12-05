// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using static System.UriComponents;
using static System.UriFormat;

/// <summary>
/// Represents an API version writer that writes the value to a path segment in the request URL.
/// </summary>
public sealed class UrlSegmentApiVersionWriter : IApiVersionWriter
{
    private readonly string token;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlSegmentApiVersionWriter"/> class.
    /// </summary>
    /// <param name="token">The replacement token to write the API version to.</param>
    public UrlSegmentApiVersionWriter( string token )
    {
        ArgumentException.ThrowIfNullOrEmpty( token );
        this.token = token;
    }

    /// <inheritdoc />
    public void Write( HttpRequestMessage request, ApiVersion apiVersion )
    {
        ArgumentNullException.ThrowIfNull( request );
        ArgumentNullException.ThrowIfNull( apiVersion );

        if ( request.RequestUri is not Uri url )
        {
            return;
        }

        var path = Uri.UnescapeDataString( url.GetComponents( Path, Unescaped ) );
        var newPath = path.Replace(
            token,
#if NETSTANDARD
            apiVersion.ToString() );
#else
            apiVersion.ToString(),
            StringComparison.Ordinal );
#endif

        if ( path == newPath )
        {
            return;
        }

        var builder = new UriBuilder( url ) { Path = newPath };
        request.RequestUri = builder.Uri;
    }
}