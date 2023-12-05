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
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides ASP.NET Core MVC specific extension methods for <see cref="IApiVersioningBuilder"/>.
/// </summary>
public static class IApiVersioningBuilderExtensions
{
    /// <summary>
    /// Adds ASP.NET Core MVC support for API versioning.
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IApiVersioningBuilder AddMvc( this IApiVersioningBuilder builder )
    {
        ArgumentNullException.ThrowIfNull( builder );
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
        ArgumentNullException.ThrowIfNull( builder );

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

    [SkipLocalsInit]
    private static DecoratedServiceDescriptor WithUrlHelperFactoryDecorator( IServiceCollection services )
    {
        var descriptor = services.FirstOrDefault( sd => sd.ServiceType == typeof( IUrlHelperFactory ) );

        if ( descriptor is DecoratedServiceDescriptor sd )
        {
            return sd;
        }

        ServiceLifetime lifetime;
        Func<IServiceProvider, object> instantiate;

        if ( descriptor == null )
        {
            lifetime = ServiceLifetime.Singleton;
            instantiate = static sp => new UrlHelperFactory();
        }
        else
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
                var factory = ActivatorUtilities.CreateFactory( typeof( ApiVersionUrlHelperFactory ), [typeof( IUrlHelperFactory )] );
                instance = factory( serviceProvider, [decorated] );
            }

            return (IUrlHelperFactory) instance;
        }

        return new( typeof( IUrlHelperFactory ), NewFactory, lifetime );
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