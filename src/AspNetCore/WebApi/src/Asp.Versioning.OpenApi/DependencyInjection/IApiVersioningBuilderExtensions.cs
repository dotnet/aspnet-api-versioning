// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.OpenApi;
using Asp.Versioning.OpenApi.Configuration;
using Asp.Versioning.OpenApi.Transformers;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.ApiDescriptions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Reflection;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using EM = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions;

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

            AddOpenApiServices( builder, GetAssemblies( Assembly.GetCallingAssembly() ) );

            return builder;
        }

        /// <summary>
        /// Adds OpenAPI support for API versioning.
        /// </summary>
        /// <param name="configureOptions">The function used to configure the
        /// <see cref="VersionedOpenApiOptions">versioned OpenAPI options</see>.</param>
        /// <returns>The original <see cref="IApiVersioningBuilder">builder</see>.</returns>
        public IApiVersioningBuilder AddOpenApi( Action<VersionedOpenApiOptions> configureOptions )
        {
            ArgumentNullException.ThrowIfNull( builder );

            AddOpenApiServices( builder, GetAssemblies( Assembly.GetCallingAssembly() ) );
            builder.Services.Configure( configureOptions );

            return builder;
        }
    }

    [UnconditionalSuppressMessage( "ILLink", "IL2026" )]
    private static void AddOpenApiServices( IApiVersioningBuilder builder, Assembly[] assemblies )
    {
        builder.AddApiExplorer();

        var services = builder.Services;

        services.AddSingleton<KeyedServiceProvider>();
        services.AddSingleton( ResolveDocumentProvider );
        services.AddSingleton<VersionedOpenApiOptionsFactory>();
        services.TryAddEnumerable( Transient<IPostConfigureOptions<OpenApiOptions>, ConfigureOpenApiOptions>() );
        services.TryAdd( Singleton<IOptionsFactory<VersionedOpenApiOptions>>( EM.GetRequiredService<VersionedOpenApiOptionsFactory> ) );
        services.AddTransient( sp => new XmlCommentsFile( assemblies, sp.GetRequiredService<IHostEnvironment>() ) );
        services.TryAddTransient( sp => new XmlCommentsTransformer( sp.GetRequiredService<XmlCommentsFile>() ) );

        if ( GetJsonConfiguration() is { } descriptor )
        {
            services.TryAddEnumerable( descriptor );
        }
    }

    // NOTE: The calling assembly must be captured at the call site that invokes AddOpenApi. In 99% of the cases that
    // should be the entry point to the application. It is technically possible to be invoked from some other assembly -
    // perhaps another extension library. If that were to happen, that library must resolve the path on its own and
    // register another XmlCommentsTransformer with the resolved path
    private static Assembly[] GetAssemblies( Assembly callingAssembly )
    {
        var assemblies = new List<Assembly>( capacity: 2 ) { callingAssembly };

        if ( Assembly.GetEntryAssembly() is { } entryAssembly && entryAssembly != callingAssembly )
        {
            assemblies.Add( entryAssembly );
        }

        return [.. assemblies];
    }

    private static IDocumentProvider ResolveDocumentProvider( IServiceProvider provider ) =>
        provider.GetRequiredService<KeyedServiceProvider>().GetRequiredService<IDocumentProvider>();

    // HACK: the json configuration is internal; this approach negates the use of reflection
    // REF: https://github.com/dotnet/aspnetcore/blob/08a9fc2c3864d99759ab3d71cfda868d852bfc4b/src/OpenApi/src/Extensions/OpenApiServiceCollectionExtensions.cs#L121
    private static ServiceDescriptor? GetJsonConfiguration()
    {
        var services = new ServiceCollection();
        services.AddOpenApi( "*" );
        return services.SingleOrDefault( sd => sd.ServiceType == typeof( IConfigureOptions<JsonOptions> ) );
    }
}