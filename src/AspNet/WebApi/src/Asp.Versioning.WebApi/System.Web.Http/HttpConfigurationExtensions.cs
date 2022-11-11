// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning;
using Asp.Versioning.Controllers;
using Asp.Versioning.Dispatcher;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using static Asp.Versioning.ApiVersionParameterLocation;

/// <summary>
/// Provides extension methods for the <see cref="HttpConfiguration"/> class.
/// </summary>
public static class HttpConfigurationExtensions
{
    private const string ApiVersioningOptionsKey = "MS_ApiVersioningOptions";

    /// <summary>
    /// Gets the current API versioning options.
    /// </summary>
    /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
    /// <returns>The current <see cref="ApiVersioningOptions">API versioning options</see>.</returns>
    public static ApiVersioningOptions GetApiVersioningOptions( this HttpConfiguration configuration )
    {
        if ( configuration == null )
        {
            throw new ArgumentNullException( nameof( configuration ) );
        }

        return (ApiVersioningOptions) configuration.Properties.GetOrAdd( ApiVersioningOptionsKey, key => new ApiVersioningOptions() );
    }

    /// <summary>
    /// Adds service API versioning to the specified services collection.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use service versioning.</param>
    public static void AddApiVersioning( this HttpConfiguration configuration )
    {
        if ( configuration == null )
        {
            throw new ArgumentNullException( nameof( configuration ) );
        }

        configuration.AddApiVersioning( new ApiVersioningOptions() );
    }

    /// <summary>
    /// Adds service API versioning to the specified services collection.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> that will use service versioning.</param>
    /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
    public static void AddApiVersioning( this HttpConfiguration configuration, Action<ApiVersioningOptions> setupAction )
    {
        if ( configuration == null )
        {
            throw new ArgumentNullException( nameof( configuration ) );
        }

        if ( setupAction == null )
        {
            throw new ArgumentNullException( nameof( setupAction ) );
        }

        var options = new ApiVersioningOptions();

        setupAction( options );
        configuration.AddApiVersioning( options );
    }

    private static void AddApiVersioning( this HttpConfiguration configuration, ApiVersioningOptions options )
    {
        var services = configuration.Services;

        services.Replace( typeof( IHttpControllerSelector ), new ApiVersionControllerSelector( configuration, options ) );
        services.Replace( typeof( IHttpActionSelector ), new ApiVersionActionSelector() );

        if ( options.ReportApiVersions )
        {
            configuration.Filters.Add( new ReportApiVersionsAttribute() );
        }

        var reader = options.ApiVersionReader;

        if ( reader.VersionsByMediaType() )
        {
            var parameterName = reader.GetParameterName( MediaTypeParameter );

            if ( !string.IsNullOrEmpty( parameterName ) )
            {
                configuration.Filters.Add( new ApplyContentTypeVersionActionFilter( reader ) );
            }
        }

        configuration.Properties.AddOrUpdate( ApiVersioningOptionsKey, options, ( key, oldValue ) => options );
        configuration.ParameterBindingRules.Add( typeof( ApiVersion ), ApiVersionParameterBinding.Create );
        SunsetPolicyManager.Default = new SunsetPolicyManager( options );
    }

    internal static IReportApiVersions GetApiVersionReporter( this HttpConfiguration configuration )
    {
        var options = configuration.GetApiVersioningOptions();

        if ( options.ReportApiVersions )
        {
            return configuration.DependencyResolver.GetApiVersionReporter();
        }

        return DoNotReportApiVersions.Instance;
    }
}