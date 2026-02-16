// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

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
using static ServiceDescriptor;
using static System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection"/> interface.
/// </summary>
[CLSCompliant( false )]
public static partial class IServiceCollectionExtensions
{
    /// <param name="services">The <see cref="IServiceCollection">services</see> available in the application.</param>
    extension( IServiceCollection services )
    {
        /// <summary>
        /// Adds service API versioning to the specified services collection.
        /// </summary>
        /// <returns>The <see cref="IApiVersioningBuilder">builder</see> used to configure API versioning.</returns>
        public IApiVersioningBuilder AddApiVersioning()
        {
            AddApiVersioningServices( services );
            return new ApiVersioningBuilder( services );
        }

        /// <summary>
        /// Adds service API versioning to the specified services collection.
        /// </summary>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        /// <returns>The <see cref="IApiVersioningBuilder">builder</see> used to configure API versioning.</returns>
        public IApiVersioningBuilder AddApiVersioning( Action<ApiVersioningOptions> setupAction )
        {
            AddApiVersioningServices( services );
            services.Configure( setupAction );
            return new ApiVersioningBuilder( services );
        }

        /// <summary>
        /// Adds error object support in problem details.
        /// </summary>
        /// <param name="setup">The <see cref="JsonOptions">JSON options</see> setup <see cref="Action{T}"/> to perform, if any.</param>
        /// <remarks>
        /// <para>
        /// This method is only intended to provide backward compatibility with previous library versions by converting
        /// <see cref="AspNetCore.Mvc.ProblemDetails"/> into Error Objects that conform to the
        /// <a ref="https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#7102-error-condition-responses">Error Responses</a>
        /// in the Microsoft REST API Guidelines and
        /// <a ref="https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#_Toc38457793">OData Error Responses</a>.
        /// </para>
        /// <para>
        /// This method should be called before <see cref="ProblemDetailsServiceCollectionExtensions.AddProblemDetails(IServiceCollection)"/>.
        /// </para>
        /// </remarks>
        public IServiceCollection AddErrorObjects( Action<JsonOptions>? setup = default ) =>
            AddErrorObjects<ErrorObjectWriter>( services, setup );

        /// <summary>
        /// Adds error object support in problem details.
        /// </summary>
        /// <typeparam name="TWriter">The type of <see cref="ErrorObjectWriter"/>.</typeparam>
        /// <param name="setup">The <see cref="JsonOptions">JSON options</see> setup <see cref="Action{T}"/> to perform, if any.</param>
        /// <remarks>
        /// <para>
        /// This method is only intended to provide backward compatibility with previous library versions by converting
        /// <see cref="AspNetCore.Mvc.ProblemDetails"/> into Error Objects that conform to the
        /// <a ref="https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#7102-error-condition-responses">Error Responses</a>
        /// in the Microsoft REST API Guidelines and
        /// <a ref="https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#_Toc38457793">OData Error Responses</a>.
        /// </para>
        /// <para>
        /// This method should be called before <see cref="ProblemDetailsServiceCollectionExtensions.AddProblemDetails(IServiceCollection)"/>.
        /// </para>
        /// </remarks>
        public IServiceCollection AddErrorObjects<[DynamicallyAccessedMembers( PublicConstructors )] TWriter>(
            Action<JsonOptions>? setup = default )
            where TWriter : ErrorObjectWriter
        {
            ArgumentNullException.ThrowIfNull( services );

            services.TryAddEnumerable( Singleton<IProblemDetailsWriter, TWriter>() );
            services.Configure( setup ?? DefaultErrorObjectJsonConfig );

            return services;
        }
    }

    private static void DefaultErrorObjectJsonConfig( JsonOptions options ) =>
        options.SerializerOptions.TypeInfoResolverChain.Insert( 0, ErrorObjectWriter.ErrorObjectJsonContext.Default );

    // HACK: convince DI that ApiVersion can be resolved as a service. this enables ApiVersion to be used as a
    // a parameter without explicitly specifying [FromServices]. DI is not actually expected to resolve ApiVersion
    // because it requires the current HttpContext. an interceptor is inserted in EndpointBuilderFinalizer.Finalize.
    // resolving ApiVersion from DI allows it to be resolved from nearly any context, which is not intended. by the time
    // an endpoint action is invoked, the ApiVersion will be available in the current HttpContext unless the API is
    // version-neutral. in those situations, the parameter can be declared as ApiVersion? instead. this function makes
    // a best effort to be honor DI by resolving the ApiVersion through IHttpContextAccessor if it's available.
    //
    // ultimately, this is required because there is no other hook. if/when a better parameter binding mechanism becomes
    // available, this is expected to go away.
    //
    // 1. TryParse does not work because:
    //    a. Parsing is delegated to IApiVersionParser.TryParse
    //    b. The result can come from multiple locations
    //    c. There can be multiple results
    // 2. BindAsync does not work because:
    //    a. It is static and must be on the ApiVersion type
    //    b. It requires HttpContext, which is specific to ASP.NET Core
    //
    // REF: https://github.com/dotnet/aspnetcore/issues/35489
    // REF: https://github.com/dotnet/aspnetcore/issues/50672
    private static ApiVersion ApiVersionAsService( IServiceProvider provider )
    {
        if ( provider.GetService<IHttpContextAccessor>() is { } accessor && accessor.HttpContext is { } context )
        {
            return context.RequestedApiVersion!;
        }

        return default!;
    }

    private static void AddApiVersioningServices( IServiceCollection services )
    {
        ArgumentNullException.ThrowIfNull( services );

        services.AddTransient( ApiVersionAsService );
        services.TryAddSingleton<IApiVersionParser, ApiVersionParser>();
        services.AddSingleton( static sp => sp.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.ApiVersionReader );
        services.AddSingleton( static sp => (IApiVersionParameterSource) sp.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.ApiVersionReader );
        services.AddSingleton( static sp => sp.GetRequiredService<IOptions<ApiVersioningOptions>>().Value.ApiVersionSelector );
        services.TryAddSingleton<IReportApiVersions, DefaultApiVersionReporter>();
        services.TryAddSingleton<IPolicyManager<SunsetPolicy>, SunsetPolicyManager>();
        services.TryAddSingleton<IPolicyManager<DeprecationPolicy>, DeprecationPolicyManager>();
        services.TryAddEnumerable( Transient<IValidateOptions<ApiVersioningOptions>, ValidateApiVersioningOptions>() );
        services.TryAddEnumerable( Transient<IPostConfigureOptions<RouteOptions>, ApiVersioningRouteOptionsSetup>() );
        services.TryAddEnumerable( Singleton<MatcherPolicy, ApiVersionMatcherPolicy>() );
        services.TryAddEnumerable( Singleton<IApiVersionMetadataCollationProvider, EndpointApiVersionMetadataCollationProvider>() );
        services.TryAddTransient<IEndpointInspector, DefaultEndpointInspector>();
        services.Replace( WithLinkGeneratorDecorator( services ) );
    }

    // REF: https://github.com/dotnet/aspnetcore/blob/main/src/Http/Routing/src/DependencyInjection/RoutingServiceCollectionExtensions.cs#L96
    // REF: https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/ServiceDescriptor.cs#L292
    private static ServiceDescriptor WithLinkGeneratorDecorator( IServiceCollection services )
    {
        var descriptor = services.FirstOrDefault( sd => sd.ServiceType == typeof( LinkGenerator ) );

        if ( descriptor == null )
        {
            services.AddRouting();
            descriptor = services.First( sd => sd.ServiceType == typeof( LinkGenerator ) );
        }

        var lifetime = descriptor.Lifetime;

        if ( descriptor.ImplementationFactory is { } factory )
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
        else
        {
            if ( descriptor.ImplementationType is { } decoratedType )
            {
                services.Replace( Describe( decoratedType, decoratedType, lifetime ) );

                LinkGenerator NewFactory( IServiceProvider serviceProvider )
                {
                    var instance = (LinkGenerator) serviceProvider.GetRequiredService( decoratedType );
                    var source = serviceProvider.GetRequiredService<IApiVersionParameterSource>();

                    if ( source.VersionsByUrl() )
                    {
                        instance = new ApiVersionLinkGenerator( instance );
                    }

                    return instance;
                }

                factory = NewFactory;
            }
            else if ( descriptor.ImplementationInstance is LinkGenerator instance )
            {
                LinkGenerator NewFactory( IServiceProvider serviceProvider )
                {
                    var source = serviceProvider.GetRequiredService<IApiVersionParameterSource>();

                    if ( source.VersionsByUrl() )
                    {
                        instance = new ApiVersionLinkGenerator( instance );
                    }

                    return instance;
                }

                factory = NewFactory;
            }
            else
            {
                throw new InvalidOperationException();
            }

            return Describe( typeof( LinkGenerator ), factory, lifetime );
        }
    }
}