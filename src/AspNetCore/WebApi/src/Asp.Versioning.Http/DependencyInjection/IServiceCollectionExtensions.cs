// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
#if !NETCOREAPP3_1
using Asp.Versioning.Builder;
#endif
using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection"/> interface.
/// </summary>
[CLSCompliant( false )]
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds service API versioning to the specified services collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection">services</see> available in the application.</param>
    /// <returns>The <see cref="IApiVersioningBuilder">builder</see> used to configure API versioning.</returns>
    public static IApiVersioningBuilder AddApiVersioning( this IServiceCollection services )
    {
        AddApiVersioningServices( services );
        return new ApiVersioningBuilder( services );
    }

    /// <summary>
    /// Adds service API versioning to the specified services collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection">services</see> available in the application.</param>
    /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
    /// <returns>The <see cref="IApiVersioningBuilder">builder</see> used to configure API versioning.</returns>
    public static IApiVersioningBuilder AddApiVersioning( this IServiceCollection services, Action<ApiVersioningOptions> setupAction )
    {
        AddApiVersioningServices( services );
        services.Configure( setupAction );
        return new ApiVersioningBuilder( services );
    }

    private static void AddApiVersioningServices( IServiceCollection services )
    {
        if ( services == null )
        {
            throw new ArgumentNullException( nameof( services ) );
        }

#if !NETCOREAPP3_1
        services.TryAddSingleton<IApiVersionSetBuilderFactory, DefaultApiVersionSetBuilderFactory>();
#endif
        services.TryAddSingleton<IApiVersionParser, ApiVersionParser>();
        services.TryAddSingleton<IProblemDetailsFactory, DefaultProblemDetailsFactory>();
        services.Add( Singleton( sp => sp.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.ApiVersionReader ) );
        services.Add( Singleton( sp => (IApiVersionParameterSource) sp.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.ApiVersionReader ) );
        services.Add( Singleton( sp => sp.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.ApiVersionSelector ) );
        services.TryAddSingleton<IReportApiVersions, DefaultApiVersionReporter>();
        services.TryAddSingleton<ISunsetPolicyManager, SunsetPolicyManager>();
        services.TryAddEnumerable( Transient<IPostConfigureOptions<RouteOptions>, ApiVersioningRouteOptionsSetup>() );
        services.TryAddEnumerable( Singleton<MatcherPolicy, ApiVersionMatcherPolicy>() );
        services.Replace( WithLinkGeneratorDecorator( services ) );
    }

    // REF: https://github.com/dotnet/runtime/blob/master/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/ServiceDescriptor.cs#L125
    private static Type GetImplementationType( this ServiceDescriptor descriptor )
    {
        if ( descriptor.ImplementationType != null )
        {
            return descriptor.ImplementationType;
        }
        else if ( descriptor.ImplementationInstance != null )
        {
            return descriptor.ImplementationInstance.GetType();
        }
        else if ( descriptor.ImplementationFactory != null )
        {
            var typeArguments = descriptor.ImplementationFactory.GetType().GenericTypeArguments;
            return typeArguments[1];
        }

        throw new InvalidOperationException();
    }

    private static ServiceDescriptor WithLinkGeneratorDecorator( IServiceCollection services )
    {
        var descriptor = services.FirstOrDefault( sd => sd.ServiceType == typeof( LinkGenerator ) );

        if ( descriptor == null )
        {
            services.AddRouting();
            descriptor = services.First( sd => sd.ServiceType == typeof( LinkGenerator ) );
        }

        var lifetime = descriptor.Lifetime;
        var factory = descriptor.ImplementationFactory;

        if ( factory == null )
        {
            var decoratedType = descriptor.GetImplementationType();
            var decoratorType = typeof( ApiVersionLinkGenerator<> ).MakeGenericType( decoratedType );

            services.Replace( Describe( decoratedType, decoratedType, lifetime ) );

            return Describe( typeof( LinkGenerator ), decoratorType, lifetime );
        }
        else
        {
            LinkGenerator NewFactory( IServiceProvider serviceProvider )
            {
                var instance = (LinkGenerator) factory( serviceProvider );
                var source = serviceProvider.GetRequiredService<IApiVersionParameterSource>();

                if ( source.VersionsByUrl() )
                {
                    instance = new ApiVersionLinkGenerator( instance );
                }

                return instance;
            }

            return Describe( typeof( LinkGenerator ), NewFactory, lifetime );
        }
    }
}