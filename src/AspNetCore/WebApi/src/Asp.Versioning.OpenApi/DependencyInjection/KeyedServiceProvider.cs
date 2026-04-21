// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812
#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.ApiDescriptions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Represents a service provider that bridges two service providers together to support keyed services for OpenAPI documents.
/// </summary>
/// <remarks>Service resolution must go through this class. DI does not support container/provider chaining. This also
/// requires explicit constructor registration to ensure that happens.</remarks>
internal sealed class KeyedServiceProvider : IKeyedServiceProvider
{
    private readonly ServiceProvider me;
    private readonly IServiceProvider parent;

    public KeyedServiceProvider( IServiceProvider parent, IApiVersionDescriptionProvider provider )
    {
        this.parent = parent;
        var services = new ServiceCollection();
        var descriptions = provider.ApiVersionDescriptions;

        for ( var i = 0; i < descriptions.Count; i++ )
        {
            var description = descriptions[i];
            var documentName = GetDocumentName( description );

            services.Add( KeyedSingleton( documentName, NewOpenApiSchemaService ) );
            services.Add( KeyedSingleton( documentName, NewOpenApiDocumentService ) );
            services.Add( KeyedSingleton( documentName, ResolveDocumentProvider ) );
            services.AddSingleton( new NamedService<OpenApiDocumentService>( documentName ) );
        }

        services.Add( Singleton<IDocumentProvider>( _ => new OpenApiDocumentProvider( this ) ) );
        me = services.BuildServiceProvider();
    }

    public object? GetKeyedService( Type serviceType, object? serviceKey ) =>
        me.GetKeyedService( serviceType, serviceKey ) ?? parent.GetKeyedService( serviceType, serviceKey );

    public object GetRequiredKeyedService( Type serviceType, object? serviceKey ) =>
        me.GetKeyedService( serviceType, serviceKey ) ?? parent.GetRequiredKeyedService( serviceType, serviceKey );

    public object? GetService( Type serviceType ) =>
        me.GetService( serviceType ) ?? parent.GetService( serviceType );

    // REF: https://github.com/dotnet/aspnetcore/blob/319e87fd950a99f3baae2aa79db3d4fb68783d85/src/OpenApi/src/Extensions/OpenApiServiceCollectionExtensions.cs#L64
#pragma warning disable CA1308 // Normalize strings to uppercase
    private static string GetDocumentName( ApiVersionDescription description ) => description.GroupName.ToLowerInvariant();
#pragma warning restore CA1308

    private static IOpenApiDocumentProvider ResolveDocumentProvider( IServiceProvider provider, object key ) =>
        provider.GetRequiredKeyedService<OpenApiDocumentService>( key );

    private OpenApiSchemaService NewOpenApiSchemaService( IServiceProvider provider, object key ) =>
        new(
            key.ToString()!,
            this.GetRequiredService<IOptions<JsonOptions>>(),
            this.GetRequiredService<IOptionsMonitor<OpenApiOptions>>() );

    private OpenApiDocumentService NewOpenApiDocumentService( IServiceProvider provider, object key ) =>
        new(
            key.ToString()!,
            this.GetRequiredService<IApiDescriptionGroupCollectionProvider>(),
            this.GetRequiredService<IHostEnvironment>(),
            this.GetRequiredService<IOptionsMonitor<OpenApiOptions>>(),
            this,
            this.GetService<IServer>() );
}