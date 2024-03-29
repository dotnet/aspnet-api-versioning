﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning.ApiExplorer;
using System.Web.Http.Description;

/// <summary>
/// Provides extension methods for the <see cref="HttpConfiguration"/> class.
/// </summary>
public static class HttpConfigurationExtensions
{
    /// <summary>
    /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports API versioning.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
    /// <returns>The newly registered <see cref="VersionedApiExplorer">versioned API explorer</see>.</returns>
    /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="VersionedApiExplorer"/>.</remarks>
    public static VersionedApiExplorer AddVersionedApiExplorer( this HttpConfiguration configuration ) =>
        configuration.AddVersionedApiExplorer( static _ => { } );

    /// <summary>
    /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports API versioning.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
    /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
    /// <returns>The newly registered <see cref="VersionedApiExplorer">versioned API explorer</see>.</returns>
    /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="VersionedApiExplorer"/>.</remarks>
    public static VersionedApiExplorer AddVersionedApiExplorer( this HttpConfiguration configuration, Action<ApiExplorerOptions> setupAction )
    {
        if ( configuration == null )
        {
            throw new ArgumentNullException( nameof( configuration ) );
        }

        if ( setupAction == null )
        {
            throw new ArgumentNullException( nameof( setupAction ) );
        }

        var options = new ApiExplorerOptions( configuration );

        setupAction( options );

        var apiExplorer = new VersionedApiExplorer( configuration, options );

        configuration.Services.Replace( typeof( IApiExplorer ), apiExplorer );

        return apiExplorer;
    }
}