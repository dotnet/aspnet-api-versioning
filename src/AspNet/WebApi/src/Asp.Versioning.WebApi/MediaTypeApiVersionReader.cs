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

        if ( accept is null || accept.Count == 0 )
        {
            return version is null ? [] : [version];
        }

        // TODO: the ranked implementation is the correct way, but ReadAcceptHeader requires a breaking change that
        // cannot ship until the next major version. internally do the right thing, but if ReadAcceptHeader is
        // overridden, then make sure we honor the implementation. the onus is on the implementer.
        if ( acceptHeaderOverridden )
        {
            return Collate( version, ReadAcceptHeader( accept ) );
        }

        return Collate( version, ReadRankedAcceptHeader( version is null ? accept : MediaTypeQuality.MaxRanked( accept ) ) );
    }
}