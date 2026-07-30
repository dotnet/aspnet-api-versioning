// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning;

using Grpc.AspNetCore.Server;
using Microsoft.Extensions.Options;

internal sealed class ApiVersioningGrpcOptions : IConfigureOptions<GrpcServiceOptions>
{
    public void Configure( GrpcServiceOptions options ) => options.Interceptors.Add<FieldInterceptor>();
}