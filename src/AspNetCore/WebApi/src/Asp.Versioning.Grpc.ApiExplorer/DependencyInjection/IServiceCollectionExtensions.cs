// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Grpc;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection"/> interface.
/// </summary>
[CLSCompliant( false )]
public static class IServiceCollectionExtensions
{
    private const string TrimmingMessage = "MVC does not currently support trimming or native AOT. https://aka.ms/aspnet/trimming";

    /// <param name="services">The extended <see cref="IServiceCollection">service collection</see>.</param>
    /// <returns>The original <paramref name="services"/>.</returns>
    extension( IServiceCollection services )
    {
        /// <summary>
        /// Adds the API Explorer extensions for gRPC.
        /// </summary>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        [RequiresUnreferencedCode( TrimmingMessage )]
        public IServiceCollection AddGrpcApiExplorer( Action<GrpcApiExplorerOptions> setupAction )
        {
            ArgumentNullException.ThrowIfNull( services );
            ArgumentNullException.ThrowIfNull( setupAction );

            return services.Configure( setupAction ).AddGrpcApiExplorer();
        }

        /// <summary>
        /// Adds the API Explorer extensions for gRPC.
        /// </summary>
        [RequiresUnreferencedCode( TrimmingMessage )]
        public IServiceCollection AddGrpcApiExplorer()
        {
            ArgumentNullException.ThrowIfNull( services );

            services.AddGrpc().AddJsonTranscoding();
            services.TryAddEnumerable( Transient<IApiDescriptionProvider, GrpcJsonTranscodingDescriptionProvider>() );
            services.TryAddEnumerable( Transient<IConfigureOptions<GrpcServiceOptions>, ApiVersioningGrpcOptions>() );
            services.AddSingleton<FileDescriptorPool>();
            services.TryAddSingleton( NewGroupCollectionProvider );
            services.AddSingleton( NewMetadataCache );

            return services;
        }
    }

#pragma warning disable CA1859

    private static IApiDescriptionGroupCollectionProvider NewGroupCollectionProvider( IServiceProvider serviceProvider )
    {
        var actionDescriptorCollectionProvider = serviceProvider.GetService<IActionDescriptorCollectionProvider>();
        var apiDescriptionProvider = serviceProvider.GetServices<IApiDescriptionProvider>();

        return new ApiDescriptionGroupCollectionProvider(
            actionDescriptorCollectionProvider ?? new EmptyActionDescriptorCollectionProvider(),
            apiDescriptionProvider );
    }

    private static ApiVersionMetadataCache NewMetadataCache( IServiceProvider serviceProvider ) =>
        new( serviceProvider.GetService<IApiVersionParser>() ?? ApiVersionParser.Default );

#pragma warning restore CA1859
#pragma warning disable IDE0079
#pragma warning disable CA1812

    private sealed class EmptyActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
    {
        public ActionDescriptorCollection ActionDescriptors { get; } = new( [], 1 );
    }
}