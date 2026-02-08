// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Web.Http.Routing;

using Asp.Versioning.Routing;
using Backport;

/// <summary>
/// Provides extension methods for <see cref="UrlHelper"/>.
/// </summary>
public static class UrlHelperExtensions
{
    /// <param name="urlHelper">The extended <see cref="UrlHelper">URL helper</see>.</param>
    extension( UrlHelper urlHelper )
    {
        /// <summary>
        /// Returns a new URL helper that includes the requested API version.
        /// </summary>
        /// <returns>A new <see cref="UrlHelper">URL helper</see> that excludes the requested
        /// API version or the original URL helper if unnecessary.</returns>
        /// <remarks>Excluding the requested API version is useful in a limited set of scenarios
        /// such as building a URL from an API that versions by URL segment to an API that is
        /// version-neutral. A version-neutral API would not use the specified route value and
        /// it would be erroneously added as a query string parameter.</remarks>
        public UrlHelper WithoutApiVersion()
        {
            ArgumentNullException.ThrowIfNull( urlHelper );

            if ( urlHelper is WithoutApiVersionUrlHelper )
            {
                return urlHelper;
            }

            return new WithoutApiVersionUrlHelper( urlHelper );
        }
    }
}