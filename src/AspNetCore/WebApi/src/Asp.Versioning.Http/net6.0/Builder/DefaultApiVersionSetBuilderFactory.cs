// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Asp.Versioning.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System.Globalization;
using static Asp.Versioning.ApiVersionParameterLocation;

internal sealed class DefaultApiVersionSetBuilderFactory :
    IApiVersionSetBuilderFactory
{
    private readonly IApiVersionParameterSource parameterSource;
    private readonly IOptions<ApiVersioningOptions> options;

    public DefaultApiVersionSetBuilderFactory(
        IApiVersionParameterSource parameterSource,
        IOptions<ApiVersioningOptions> options )
    {
        this.parameterSource = parameterSource;
        this.options = options;
    }

    public ApiVersionSetBuilder Create( string? name = default ) =>
        new( name, parameterSource, options );
}
