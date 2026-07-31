// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Google.Protobuf.Reflection;
using Grpc.AspNetCore.Server;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

/// <summary>
/// Provides ASP.NET Core gRPC specific extension methods for <see cref="IApiVersioningBuilder"/>.
/// </summary>
public static class IApiVersioningBuilderExtensions
{
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    extension( IApiVersioningBuilder builder )
    {
        /// <summary>
        /// Adds ASP.NET Core gRPC support for API versioning.
        /// </summary>
        public IApiVersioningBuilder AddGrpc()
        {
            ArgumentNullException.ThrowIfNull( builder );

            var services = builder.Services;

            services.AddGrpc();
            services.TryAddSingleton<IAnnotation<FieldDescriptor, ApiVersionRange>, ApiVersionMetadataCache>();
            services.TryAddEnumerable( Transient<IConfigureOptions<GrpcServiceOptions>, ApiVersioningGrpcOptions>() );

            return builder;
        }
    }
}