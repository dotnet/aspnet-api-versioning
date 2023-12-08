// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection"/> interface.
/// </summary>
[CLSCompliant( false )]
public static partial class IServiceCollectionExtensions
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

    /// <summary>
    /// Enables binding the <see cref="ApiVersion"/> type in Minimal API parameters..
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IApiVersioningBuilder EnableApiVersionBinding( this IApiVersioningBuilder builder )
    {
        ArgumentNullException.ThrowIfNull( builder );

        // currently required because there is no other hook.
        // 1. TryParse does not work because:
        //    a. Parsing is delegated to IApiVersionParser.TryParse
        //    b. The result can come from multiple locations
        //    c. There can be multiple results
        // 2. BindAsync does not work because:
        //    a. It is static and must be on the ApiVersion type
        //    b. It is specific to ASP.NET Core
        builder.Services.AddHttpContextAccessor();

        // this registration is 'truthy'. it is possible for the requested API version to be null; however, but the time this is
        // resolved for a request delegate it can only be null if the API is version-neutral and no API version was requested. this
        // should be a rare and nonsensical scenario. declaring the parameter as ApiVersion? should be expect and solve the issue
        //
        // it should also be noted that this registration allows resolving the requested API version from virtually any context.
        // that is not intended, which is why this extension is not named something more general such as AddApiVersionAsService.
        // if/when a better parameter binding mechanism becomes available, this method is expected to become obsolete, no-op, and
        // eventually go away.
        builder.Services.AddTransient( sp => sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.GetRequestedApiVersion()! );

        return builder;
    }

    private static void AddApiVersioningServices( IServiceCollection services )
    {
        ArgumentNullException.ThrowIfNull( services );

        services.TryAddSingleton<IApiVersionParser, ApiVersionParser>();
        services.AddSingleton( sp => sp.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.ApiVersionReader );
        services.AddSingleton( sp => (IApiVersionParameterSource) sp.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.ApiVersionReader );
        services.AddSingleton( sp => sp.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.ApiVersionSelector );
        services.TryAddSingleton<IReportApiVersions, DefaultApiVersionReporter>();
        services.TryAddSingleton<ISunsetPolicyManager, SunsetPolicyManager>();
        services.TryAddEnumerable( Transient<IValidateOptions<ApiVersioningOptions>, ValidateApiVersioningOptions>() );
        services.TryAddEnumerable( Transient<IPostConfigureOptions<RouteOptions>, ApiVersioningRouteOptionsSetup>() );
        services.TryAddEnumerable( Singleton<MatcherPolicy, ApiVersionMatcherPolicy>() );
        services.TryAddEnumerable( Singleton<IApiVersionMetadataCollationProvider, EndpointApiVersionMetadataCollationProvider>() );
        services.Replace( WithLinkGeneratorDecorator( services ) );
        TryAddProblemDetailsRfc7231Compliance( services );
        TryAddErrorObjectJsonOptions( services );
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
            // REF: https://github.com/dotnet/aspnetcore/blob/main/src/Http/Routing/src/DependencyInjection/RoutingServiceCollectionExtensions.cs#L96
            // REF: https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/ServiceDescriptor.cs#L292
            var decoratedType = descriptor switch
            {
                { ImplementationType: var type } when type is not null => type,
                { ImplementationInstance: var instance } when instance is not null => instance.GetType(),
                _ => throw new InvalidOperationException(),
            };

            services.Replace( Describe( decoratedType, decoratedType, lifetime ) );

            LinkGenerator NewFactory( IServiceProvider serviceProvider )
            {
                var instance = (LinkGenerator) serviceProvider.GetRequiredService( decoratedType! );
                var source = serviceProvider.GetRequiredService<IApiVersionParameterSource>();

                if ( source.VersionsByUrl() )
                {
                    instance = new ApiVersionLinkGenerator( instance );
                }

                return instance;
            }

            return Describe( typeof( LinkGenerator ), NewFactory, lifetime );
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

    // TODO: Remove in .NET 9.0 or .NET 8.0 patch
    // BUG: https://github.com/dotnet/aspnetcore/issues/52577
    private static void TryAddProblemDetailsRfc7231Compliance( IServiceCollection services )
    {
        var descriptor = services.FirstOrDefault( IsDefaultProblemDetailsWriter );

        if ( descriptor == null )
        {
            return;
        }

        var decoratedType = descriptor.ImplementationType!;
        var lifetime = descriptor.Lifetime;

        services.Add( Describe( decoratedType, decoratedType, lifetime ) );
        services.Replace( Describe( typeof( IProblemDetailsWriter ), sp => NewProblemDetailsWriter( sp, decoratedType ), lifetime ) );

        static bool IsDefaultProblemDetailsWriter( ServiceDescriptor serviceDescriptor ) =>
            serviceDescriptor.ServiceType == typeof( IProblemDetailsWriter ) &&
            serviceDescriptor.ImplementationType?.FullName == "Microsoft.AspNetCore.Http.DefaultProblemDetailsWriter";

        static Rfc7231ProblemDetailsWriter NewProblemDetailsWriter( IServiceProvider serviceProvider, Type decoratedType ) =>
            new( (IProblemDetailsWriter) serviceProvider.GetRequiredService( decoratedType ) );
    }

    private static void TryAddErrorObjectJsonOptions( IServiceCollection services )
    {
        var serviceType = typeof( IProblemDetailsWriter );
        var implementationType = typeof( ErrorObjectWriter );

        for ( var i = 0; i < services.Count; i++ )
        {
            var service = services[i];

            // inheritance is intentionally not considered here because it will require a user-defined
            // JsonSerlizerContext and IConfigureOptions<JsonOptions>
            if ( service.ServiceType == serviceType &&
                 service.ImplementationType == implementationType )
            {
                services.TryAddEnumerable( Singleton<IConfigureOptions<JsonOptions>, ErrorObjectJsonOptionsSetup>() );
                return;
            }
        }
    }
}