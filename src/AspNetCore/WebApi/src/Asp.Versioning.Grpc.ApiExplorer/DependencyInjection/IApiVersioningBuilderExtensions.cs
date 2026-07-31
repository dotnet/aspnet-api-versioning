// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Grpc;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides ASP.NET Core API Explorer specific extension methods for <see cref="IApiVersioningBuilder"/>.
/// </summary>
public static class IApiVersioningBuilderExtensions
{
    private const string TrimmingMessage = "The API Explorer does not currently support trimming or native AOT. https://aka.ms/aspnet/trimming";

    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    extension( IApiVersioningBuilder builder )
    {
        /// <summary>
        /// Adds the API Explorer extensions for gRPC.
        /// </summary>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        [RequiresUnreferencedCode( TrimmingMessage )]
        public IApiVersioningBuilder AddGrpcApiExplorer( Action<GrpcApiExplorerOptions> setupAction )
        {
            ArgumentNullException.ThrowIfNull( builder );
            ArgumentNullException.ThrowIfNull( setupAction );

            builder.Services.Configure( setupAction );
            return builder.AddGrpcApiExplorer();
        }

        /// <summary>
        /// Adds the API Explorer extensions for gRPC.
        /// </summary>
        [RequiresUnreferencedCode( TrimmingMessage )]
        public IApiVersioningBuilder AddGrpcApiExplorer()
        {
            ArgumentNullException.ThrowIfNull( builder );

            var services = builder.Services;

            services.AddGrpc().AddJsonTranscoding();
            services.TryAddEnumerable( Transient<IApiDescriptionProvider, GrpcJsonTranscodingDescriptionProvider>() );
            services.AddSingleton<FileDescriptorPool>();
            services.TryAddSingleton( NewGroupCollectionProvider );

            return builder;
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

#pragma warning restore CA1859
#pragma warning disable IDE0079
#pragma warning disable CA1812

    private sealed class EmptyActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
    {
        public ActionDescriptorCollection ActionDescriptors { get; } = new( [], 1 );
    }
}