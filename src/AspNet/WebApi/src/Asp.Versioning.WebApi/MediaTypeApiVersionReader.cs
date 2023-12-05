// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <content>
/// Provides the implementation for ASP.NET Web API.
/// </content>
public partial class MediaTypeApiVersionReader
{
    /// <inheritdoc />
    public virtual IReadOnlyList<string> Read( HttpRequestMessage request )
    {
        ArgumentNullException.ThrowIfNull( request );

        var contentType = request.Content?.Headers.ContentType;
        var version = contentType is null ? default : ReadContentTypeHeader( contentType );
        var accept = request.Headers.Accept;

        if ( accept is null || ReadAcceptHeader( accept ) is not string otherVersion )
        {
            return version is null ? [] : [version];
        }

        var comparer = StringComparer.OrdinalIgnoreCase;

        if ( version is null || comparer.Equals( version, otherVersion ) )
        {
            return [otherVersion];
        }

        return comparer.Compare( version, otherVersion ) <= 0
               ? [version, otherVersion]
               : [otherVersion, version];
    }
}