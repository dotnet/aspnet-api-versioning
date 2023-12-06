// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Provides extension methods for <see cref="IApiVersionSelector"/>.
/// </summary>
[CLSCompliant( false )]
public static class IApiVersionSelectorExtensions
{
    /// <summary>
    /// Selects an API version given the specified API version information.
    /// </summary>
    /// <param name="selector">The extended <see cref="IApiVersionSelector"/>.</param>
    /// <param name="model">The <see cref="ApiVersionModel">model</see> to select the version from.</param>
    /// <returns>The selected <see cref="ApiVersion">API version</see>.</returns>
    public static ApiVersion SelectVersion( this IApiVersionSelector selector, ApiVersionModel model )
    {
        ArgumentNullException.ThrowIfNull( selector );
        var context = new DefaultHttpContext();
        return selector.SelectVersion( context.Request, model );
    }
}