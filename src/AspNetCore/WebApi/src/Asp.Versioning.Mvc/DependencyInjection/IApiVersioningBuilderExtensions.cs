// Copyright (c) .NET Foundation and contributors. All rights reserved.

// Ignore Spelling: Mvc
namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.ApplicationModels;
using Asp.Versioning.Conventions;
using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides ASP.NET Core MVC specific extension methods for <see cref="IApiVersioningBuilder"/>.
/// </summary>
#if NETCOREAPP3_1
[CLSCompliant( false )]
#endif
public static class IApiVersioningBuilderExtensions
{
    /// <summary>
    /// Adds ASP.NET Core MVC support for API versioning.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IApiVersioningBuilder AddMvc( this IApiVersioningBuilder builder )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        AddServices( builder.Services );

        return builder;
    }

    /// <summary>
    /// Adds ASP.NET Core MVC support for API versioning.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IApiVersioningBuilder AddMvc( this IApiVersioningBuilder builder, Action<MvcApiVersioningOptions> setupAction )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        var services = builder.Services;

        AddServices( services );
        services.Configure( setupAction );

        return builder;
    }

    private static void AddServices( IServiceCollection services )
    {
        services.AddMvcCore();
        services.TryAddSingleton<IOptionsFactory<MvcApiVersioningOptions>, MvcApiVersioningOptionsFactory<MvcApiVersioningOptions>>();
        services.TryAddSingleton<IControllerNameConvention, DefaultControllerNameConvention>();
        services.TryAddSingleton<IApiVersionConventionBuilder>( sp => new ApiVersionConventionBuilder( sp.GetRequiredService<IControllerNameConvention>() ) );
        services.TryAddSingleton<IApiControllerFilter, DefaultApiControllerFilter>();
        services.TryAddSingleton( sp => new ReportApiVersionsAttribute( sp.GetRequiredService<IReportApiVersions>() ) );
        services.AddSingleton<ApplyContentTypeVersionActionFilter>();
        services.TryAddEnumerable( Transient<IPostConfigureOptions<MvcOptions>, ApiVersioningMvcOptionsSetup>() );
        services.TryAddEnumerable( Transient<IApplicationModelProvider, ApiVersioningApplicationModelProvider>() );
        services.TryAddEnumerable( Transient<IActionDescriptorProvider, ApiVersionCollator>() );
        services.TryAddEnumerable( Transient<IApiControllerSpecification, ApiBehaviorSpecification>() );
        services.TryAddEnumerable( Singleton<IApiVersionMetadataCollationProvider, ActionApiVersionMetadataCollationProvider>() );
        services.Replace( WithUrlHelperFactoryDecorator( services ) );

        if ( !services.TryReplace(
                typeof( DefaultProblemDetailsFactory ),
                Singleton<IProblemDetailsFactory, MvcProblemDetailsFactory>() ) )
        {
            services.TryReplace(
                typeof( ErrorObjectFactory ),
                Singleton<IProblemDetailsFactory, ErrorObjectFactory>(
                    sp => ErrorObjectFactory.Decorate(
                        new MvcProblemDetailsFactory(
                            sp.GetRequiredService<ProblemDetailsFactory>() ) ) ) );
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

    private static ServiceDescriptor WithUrlHelperFactoryDecorator( IServiceCollection services )
    {
        var descriptor = services.FirstOrDefault( sd => sd.ServiceType == typeof( IUrlHelperFactory ) );

        if ( descriptor is DecoratedServiceDescriptor )
        {
            return descriptor;
        }

        var lifetime = ServiceLifetime.Singleton;
        Func<IServiceProvider, object> instantiate = sp => new UrlHelperFactory();

        if ( descriptor != null )
        {
            lifetime = descriptor.Lifetime;
            instantiate = sp => sp.CreateInstance( descriptor );
        }

        IUrlHelperFactory NewFactory( IServiceProvider serviceProvider )
        {
            var decorated = instantiate( serviceProvider );
            var source = serviceProvider.GetRequiredService<IApiVersionParameterSource>();
            var instance = decorated;

            if ( source.VersionsByUrl() )
            {
                var factory = ActivatorUtilities.CreateFactory( typeof( ApiVersionUrlHelperFactory ), new[] { typeof( IUrlHelperFactory ) } );
                instance = factory( serviceProvider, new[] { decorated } );
            }

            return (IUrlHelperFactory) instance;
        }

        return new DecoratedServiceDescriptor( typeof( IUrlHelperFactory ), NewFactory, lifetime );
    }

    private static bool TryReplace( this IServiceCollection services, Type implementationType, ServiceDescriptor descriptor )
    {
        for ( var i = 0; i < services.Count; i++ )
        {
            var service = services[i];
            var match = service.ServiceType == descriptor.ServiceType &&
                      ( service.ImplementationType == implementationType ||
                        service.ImplementationInstance?.GetType() == implementationType ||
                        service.ImplementationFactory?.Method.ReturnType == implementationType );

            if ( match )
            {
                services[i] = descriptor;
                return true;
            }
        }

        return false;
    }

    private sealed class DecoratedServiceDescriptor : ServiceDescriptor
    {
        internal DecoratedServiceDescriptor(
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory,
            ServiceLifetime lifetime )
            : base( serviceType, implementationFactory, lifetime ) { }
    }
}