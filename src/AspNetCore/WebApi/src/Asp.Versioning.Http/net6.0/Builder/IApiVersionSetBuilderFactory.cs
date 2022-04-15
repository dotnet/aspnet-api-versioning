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

/// <summary>
/// Defines the behavior of a factory to create API version set builders.
/// </summary>
public interface IApiVersionSetBuilderFactory
{
    /// <summary>
    /// Creates and returns a new API version set builder.
    /// </summary>
    /// <param name="name">The name of the API associated with the builder, if any.</param>
    /// <returns>A new <see cref="ApiVersionSetBuilder">API version set builder</see>.</returns>
    ApiVersionSetBuilder Create( string? name = default );
}
