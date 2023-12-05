// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http.Routing;

using Asp.Versioning.Routing;
using Backport;

/// <summary>
/// Provides extension methods for <see cref="UrlHelper"/>.
/// </summary>
public static class UrlHelperExtensions
{
    /// <summary>
    /// Returns a new URL helper that includes the requested API version.
    /// </summary>
    /// <param name="urlHelper">The extended <see cref="UrlHelper">URL helper</see>.</param>
    /// <returns>A new <see cref="UrlHelper">URL helper</see> that excludes the requested
    /// API version or the original <paramref name="urlHelper">URL helper</paramref> if
    /// unnecessary.</returns>
    /// <remarks>Excluding the requested API version is useful in a limited set of scenarios
    /// such as building a URL from an API that versions by URL segment to an API that is
    /// version-neutral. A version-neutral API would not use the specified route value and
    /// it would be erroneously added as a query string parameter.</remarks>
    public static UrlHelper WithoutApiVersion( this UrlHelper urlHelper )
    {
        ArgumentNullException.ThrowIfNull( urlHelper );

        if ( urlHelper is WithoutApiVersionUrlHelper )
        {
            return urlHelper;
        }

        return new WithoutApiVersionUrlHelper( urlHelper );
    }
}