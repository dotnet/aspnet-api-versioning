// Copyright (c) .NET Foundation and contributors. All rights reserved.

// created by the options infrastructure
#pragma warning disable CA1812

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Grpc;
using global::Grpc.AspNetCore.Server;
using Microsoft.Extensions.Options;

internal sealed class ApiVersioningGrpcOptions : IConfigureOptions<GrpcServiceOptions>
{
    public void Configure( GrpcServiceOptions options ) => options.Interceptors.Add<FieldInterceptor>();
}