// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;

/// <content>
/// Provides the implementation for ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public partial class MediaTypeApiVersionReader
{
    /// <inheritdoc />
    public virtual IReadOnlyList<string> Read( HttpRequest request )
    {
        ArgumentNullException.ThrowIfNull( request );

        var headers = request.GetTypedHeaders();
        var contentType = headers.ContentType;
        var version = contentType is null ? default : ReadContentTypeHeader( contentType );
        var accept = headers.Accept;

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