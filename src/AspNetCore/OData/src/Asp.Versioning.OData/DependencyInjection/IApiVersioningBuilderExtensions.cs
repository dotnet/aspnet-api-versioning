// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.ApplicationModels;
using Asp.Versioning.OData;
using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Runtime.CompilerServices;
using static Asp.Versioning.OData.ODataMultiModelApplicationModelProvider;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides ASP.NET Core OData specific extension methods for <see cref="IApiVersioningBuilder"/>.
/// </summary>
public static class IApiVersioningBuilderExtensions
{
    /// <summary>
    /// Adds ASP.NET Core OData support for API versioning.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IApiVersioningBuilder AddOData( this IApiVersioningBuilder builder )
    {
        ArgumentNullException.ThrowIfNull( builder );
        AddServices( builder.AddMvc().Services );
        return builder;
    }

    /// <summary>
    /// Adds ASP.NET Core OData support for API versioning.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    [CLSCompliant( false )]
    public static IApiVersioningBuilder AddOData( this IApiVersioningBuilder builder, Action<ODataApiVersioningOptions> setupAction )
    {
        ArgumentNullException.ThrowIfNull( builder );

        var services = builder.AddMvc().Services;
        AddServices( services );
        services.Configure( setupAction );
        return builder;
    }

    private static void AddServices( IServiceCollection services )
    {
        services.TryRemoveODataService( typeof( IApplicationModelProvider ), ODataRoutingApplicationModelProviderType );

        var partManager = services.GetOrCreateApplicationPartManager();
        var configured = partManager.ConfigureDefaultFeatureProviders();

        services.AddHttpContextAccessor();
        services.TryAddSingleton<VersionedODataOptions>();
        services.TryReplaceODataService(
            Singleton<IODataTemplateTranslator, VersionedODataTemplateTranslator>(),
            "Microsoft.AspNetCore.OData.Routing.Template.DefaultODataTemplateTranslator" );
        services.Replace( Singleton<IOptions<ODataOptions>>( sp => sp.GetRequiredService<VersionedODataOptions>() ) );
        services.Replace( WithHttpContextFactoryDecorator( services ) );
        services.TryAddTransient<VersionedODataModelBuilder>();
        services.TryAddSingleton<IOptionsFactory<ODataApiVersioningOptions>, ODataApiVersioningOptionsFactory>();
        services.TryAddSingleton<IODataApiVersionCollectionProvider, ODataApiVersionCollectionProvider>();
        services.TryAddEnumerable( Transient<IApiControllerSpecification, ODataControllerSpecification>() );
        services.TryAddEnumerable( Transient<IPostConfigureOptions<ODataOptions>, ODataOptionsPostSetup>() );
        services.TryAddEnumerable( Singleton<MatcherPolicy, DefaultMetadataMatcherPolicy>() );
        services.TryAddEnumerable( Transient<IApplicationModelProvider, ODataApplicationModelProvider>() );
        services.TryAddEnumerable( Transient<IApplicationModelProvider, ODataMultiModelApplicationModelProvider>() );

        if ( configured )
        {
            services.AddModelConfigurationsAsServices( partManager );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Type GetODataType( string typeName )
    {
        var assemblyName = typeof( ODataOptions ).Assembly.GetName().Name;
        return Type.GetType( $"{typeName}, {assemblyName}", throwOnError: true, ignoreCase: false )!;
    }

    private static void TryRemoveODataService( this IServiceCollection services, Type serviceType, Type implementationType )
    {
        for ( var i = 0; i < services.Count; i++ )
        {
            var service = services[i];

            if ( service.ServiceType == serviceType && service.ImplementationType == implementationType )
            {
                services.RemoveAt( i );
                return;
            }
        }

        var message = string.Format(
            CultureInfo.CurrentCulture,
            Format.UnableToFindServices,
            nameof( IMvcBuilder ),
            "AddOData",
            "ConfigureServices(...)" );

        throw new InvalidOperationException( message );
    }

    private static void TryReplaceODataService(
        this IServiceCollection services,
        ServiceDescriptor replacement,
        string implementationTypeName )
    {
        var serviceType = replacement.ServiceType;
        var implementationType = GetODataType( implementationTypeName );

        for ( var i = 0; i < services.Count; i++ )
        {
            var service = services[i];

            if ( service.ServiceType == serviceType && service.ImplementationType == implementationType )
            {
                services[i] = replacement;
                break;
            }
        }
    }

    private static object CreateInstance( this IServiceProvider services, ServiceDescriptor descriptor )
    {
        if ( descriptor.ImplementationInstance != null )
        {
            return descriptor.ImplementationInstance;
        }

        if ( descriptor.ImplementationFactory != null )
        {
            return descriptor.ImplementationFactory( services );
        }

        return ActivatorUtilities.GetServiceOrCreateInstance( services, descriptor.ImplementationType! );
    }

    private static ServiceDescriptor WithHttpContextFactoryDecorator( IServiceCollection services )
    {
        var descriptor = services.First( sd => sd.ServiceType == typeof( IHttpContextFactory ) );
        var lifetime = descriptor.Lifetime;

        IHttpContextFactory NewFactory( IServiceProvider serviceProvider )
        {
            var decorated = (IHttpContextFactory) serviceProvider.CreateInstance( descriptor );
            return new HttpContextFactoryDecorator( decorated );
        }

        return Describe( typeof( IHttpContextFactory ), NewFactory, lifetime );
    }

    private sealed class HttpContextFactoryDecorator( IHttpContextFactory decorated ) : IHttpContextFactory
    {
        public HttpContext Create( IFeatureCollection featureCollection )
        {
            // features do not support cloning or DI, which is precisely why ASP.NET Core no longer supports
            // batching natively. The team states that HTTP/2+ improvements supplants the gains of using
            // batching over HTTP/1.x. The OData team continues to try to shoehorn it in anyway by making a
            // best-effort guess as to which features can or can't be preserved.
            //
            // REF: https://github.com/OData/AspNetCoreOData/blob/main/src/Microsoft.AspNetCore.OData/Batch/ODataBatchReaderExtensions.cs#L193
            //
            // since OData knows nothing about api versioning, it just assumes that it can preserve IApiVersioningFeature,
            // which it can't. by explicitly setting the feature to null, it will clear it from the feature collection
            // each time a HttpContext is created in a batch request.
            featureCollection.Set( default( IApiVersioningFeature ) );

            // backport of bug that was fixed in 8.0.10 so consumers aren't forced to update - yet
            // REF: https://github.com/OData/AspNetCoreOData/issues/349
            featureCollection.Set( default( IQueryFeature ) );

            return decorated.Create( featureCollection );
        }

        public void Dispose( HttpContext httpContext ) => decorated.Dispose( httpContext );
    }
}