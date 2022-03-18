// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;

internal sealed class ApiVersioningBuilder : IApiVersioningBuilder
{
    public ApiVersioningBuilder( IServiceCollection services ) => Services = services;

    public IServiceCollection Services { get; }
}