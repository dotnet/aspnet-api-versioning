// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using System.Collections.Concurrent;
using System.Web.Http.Description;
using System.Web.Http.Routing;

/// <summary>
/// Provides extension methods for the <see cref="HttpConfiguration"/> class.
/// </summary>
public static class HttpConfigurationExtensions
{
    /// <summary>
    /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports OData and API versioning.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
    /// <returns>The newly registered <see cref="ODataApiExplorer">versioned OData API explorer</see>.</returns>
    /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="ODataApiExplorer"/>. This method also
    /// configures the <see cref="ODataApiExplorer"/> to not use <see cref="ApiExplorerSettingsAttribute"/>, which enables exploring all OData
    /// controllers without additional configuration.</remarks>
    public static ODataApiExplorer AddODataApiExplorer( this HttpConfiguration configuration )
    {
        if ( configuration == null )
        {
            throw new ArgumentNullException( nameof( configuration ) );
        }

        return configuration.AddODataApiExplorer( new ODataApiExplorerOptions( configuration ) );
    }

    /// <summary>
    /// Adds or replaces the configured <see cref="IApiExplorer">API explorer</see> with an implementation that supports OData and API versioning.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> used to add the API explorer.</param>
    /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
    /// <returns>The newly registered <see cref="ODataApiExplorer">versioned API explorer</see>.</returns>
    /// <remarks>This method always replaces the <see cref="IApiExplorer"/> with a new instance of <see cref="ODataApiExplorer"/>.</remarks>
    public static ODataApiExplorer AddODataApiExplorer( this HttpConfiguration configuration, Action<ODataApiExplorerOptions> setupAction )
    {
        if ( configuration == null )
        {
            throw new ArgumentNullException( nameof( configuration ) );
        }

        if ( setupAction == null )
        {
            throw new ArgumentNullException( nameof( setupAction ) );
        }

        var options = new ODataApiExplorerOptions( configuration );

        setupAction( options );
        return configuration.AddODataApiExplorer( options );
    }

    private static ODataApiExplorer AddODataApiExplorer( this HttpConfiguration configuration, ODataApiExplorerOptions options )
    {
        var apiExplorer = new ODataApiExplorer( configuration, options );
        configuration.Services.Replace( typeof( IApiExplorer ), apiExplorer );
        return apiExplorer;
    }

    internal static IServiceProvider GetODataRootContainer( this HttpConfiguration configuration, IHttpRoute route )
    {
        const string RootContainerMappingsKey = "Microsoft.AspNet.OData.RootContainerMappingsKey";
        const string NonODataRootContainerKey = "Microsoft.AspNet.OData.NonODataRootContainerKey";
        var properties = configuration.Properties;
        var containers = (ConcurrentDictionary<string, IServiceProvider>) properties.GetOrAdd( RootContainerMappingsKey, key => new ConcurrentDictionary<string, IServiceProvider>() );
        var routeName = configuration.Routes.GetRouteName( route );

        if ( !string.IsNullOrEmpty( routeName ) && containers.TryGetValue( routeName!, out var serviceProvider ) )
        {
            return serviceProvider;
        }

        if ( route is not ODataRoute &&
             properties.TryGetValue( NonODataRootContainerKey, out var value ) &&
             ( serviceProvider = value as IServiceProvider ) is not null )
        {
            return serviceProvider;
        }

        throw new InvalidOperationException( ODataExpSR.NullContainer );
    }

    internal static ODataUrlKeyDelimiter? GetUrlKeyDelimiter( this HttpConfiguration configuration )
    {
        const string UrlKeyDelimiterKey = "Microsoft.AspNet.OData.UrlKeyDelimiterKey";

        if ( configuration.Properties.TryGetValue( UrlKeyDelimiterKey, out var value ) )
        {
            return value as ODataUrlKeyDelimiter;
        }

        return default;
    }
}