// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.OpenApi;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides OpenAPI specific extension methods for <see cref="IApiVersioningBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IApiVersioningBuilderExtensions
{
    extension( IApiVersioningBuilder builder )
    {
        /// <summary>
        /// Adds OpenAPI support for API versioning.
        /// </summary>
        /// <returns>The original <see cref="IApiVersioningBuilder">builder</see>.</returns>
        public IApiVersioningBuilder AddOpenApi()
        {
            ArgumentNullException.ThrowIfNull( builder );

            var services = builder.Services;

            AddOpenApiServices( services );
            services.TryAddKeyedTransient( typeof( ApiVersion ), NoOptions );

            return builder;
        }

        /// <summary>
        /// Adds OpenAPI support for API versioning.
        /// </summary>
        /// <param name="configureOptions">The function used to configure the target <see cref="OpenApiOptions">options</see>.</param>
        /// <returns>The original <see cref="IApiVersioningBuilder">builder</see>.</returns>
        public IApiVersioningBuilder AddOpenApi( Action<ApiVersionDescription, OpenApiOptions> configureOptions )
        {
            ArgumentNullException.ThrowIfNull( builder );

            var services = builder.Services;

            AddOpenApiServices( services );
            services.TryAddKeyedTransient( typeof( ApiVersion ), ( _, _ ) => configureOptions );

            return builder;
        }

        /// <summary>
        /// Adds OpenAPI support for API versioning.
        /// </summary>
        /// <param name="descriptionOptions">The function used to configure the target
        /// <see cref="OpenApiDocumentDescriptionOptions">title options</see>.</param>
        /// <returns>The original <see cref="IApiVersioningBuilder">builder</see>.</returns>
        public IApiVersioningBuilder AddOpenApi( Action<OpenApiDocumentDescriptionOptions> descriptionOptions )
        {
            ArgumentNullException.ThrowIfNull( builder );

            var services = builder.Services;

            AddOpenApiServices( services );
            services.Configure( descriptionOptions );
            services.TryAddKeyedTransient( typeof( ApiVersion ), NoOptions );

            return builder;
        }

        /// <summary>
        /// Adds OpenAPI support for API versioning.
        /// </summary>
        /// <param name="configureOptions">The function used to configure the target
        /// <see cref="OpenApiOptions">OpenAPI options</see>.</param>
        /// <param name="descriptionOptions">The function used to configure the target
        /// <see cref="OpenApiDocumentDescriptionOptions">title options</see>.</param>
        /// <returns>The original <see cref="IApiVersioningBuilder">builder</see>.</returns>
        public IApiVersioningBuilder AddOpenApi(
            Action<ApiVersionDescription, OpenApiOptions> configureOptions,
            Action<OpenApiDocumentDescriptionOptions> descriptionOptions )
        {
            ArgumentNullException.ThrowIfNull( builder );

            var services = builder.Services;

            AddOpenApiServices( services );
            services.Configure( descriptionOptions );
            services.TryAddKeyedTransient( typeof( ApiVersion ), ( _, _ ) => configureOptions );

            return builder;
        }
    }

    private static void AddOpenApiServices( IServiceCollection services )
    {
        services.AddOptions<OpenApiDocumentDescriptionOptions>();
        services.Add( Singleton<ConfigureOpenApiOptions, ConfigureOpenApiOptions>() );
        services.TryAddEnumerable( Singleton<IConfigureOptions<OpenApiOptions>, ConfigureOpenApiOptions>( static sp => sp.GetRequiredService<ConfigureOpenApiOptions>() ) );

        if ( GetJsonConfiguration() is { } descriptor )
        {
            services.TryAddEnumerable( descriptor );
        }
    }

    // HACK: the json configuration is internal; this approach negates the use of reflection
    // REF: https://github.com/dotnet/aspnetcore/blob/08a9fc2c3864d99759ab3d71cfda868d852bfc4b/src/OpenApi/src/Extensions/OpenApiServiceCollectionExtensions.cs#L121
    private static ServiceDescriptor? GetJsonConfiguration()
    {
        var services = new ServiceCollection();
        services.AddOpenApi( "*" );
        return services.SingleOrDefault( sd => sd.ServiceType == typeof( IConfigureOptions<JsonOptions> ) );
    }

#pragma warning disable IDE0060

    private static Action<ApiVersionDescription, OpenApiOptions> NoOptions( IServiceProvider provider, object key ) => static ( _, _ ) => { };
}