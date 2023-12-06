// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Provides extension methods for <see cref="IApiVersionSelector"/>.
/// </summary>
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
        using var request = new HttpRequestMessage();
        return selector.SelectVersion( request, model );
    }
}